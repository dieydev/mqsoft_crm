using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _geminiApiKey;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ChatbotService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _geminiApiKey = configuration["GeminiApiKey"];
        }

        public async Task<List<LichSuHoiDap>> GetChatHistoryAsync(int userId)
        {
            return await _context.LichSuHoiDaps
                .Include(c => c.DanhGiaCauTraLois)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<LichSuHoiDap> AskQuestionAsync(int userId, string question)
        {
            // Kiểm tra rate limit (30s)
            var lastChat = await _context.LichSuHoiDaps
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefaultAsync();

            if (lastChat != null && (DateTime.Now - lastChat.CreatedDate).TotalSeconds < 30)
            {
                var remain = 30 - (int)(DateTime.Now - lastChat.CreatedDate).TotalSeconds;
                throw new Exception($"Vui lòng đợi {remain} giây nữa trước khi gửi câu hỏi tiếp theo.");
            }

            string answer = await GenerateAnswerAsync(question);

            var chatHistory = new LichSuHoiDap
            {
                UserId = userId,
                Question = question,
                Answer = answer,
                CreatedDate = DateTime.Now
            };

            _context.LichSuHoiDaps.Add(chatHistory);
            await _context.SaveChangesAsync();

            return chatHistory;
        }

        private async Task<string> GenerateAnswerAsync(string question)
        {
            var q = question.ToLower();
            
            // 1. TÀI LIỆU NỘI BỘ (Như cũ)
            var documents = await _context.TaiLieuNoiBos
                .Include(d => d.NhomTaiLieu)
                .Include(d => d.TaiLieuTags)
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            var matchedDocs = documents.Where(d => 
                (d.Title != null && d.Title.ToLower().Contains(q)) ||
                (d.Description != null && d.Description.ToLower().Contains(q)) ||
                d.TaiLieuTags.Any(t => t.TagName != null && t.TagName.ToLower().Contains(q)) ||
                q.Contains(d.Title.ToLower())
            ).Take(3).ToList();

            string contextText = "";
            string docLinks = "";
            if (matchedDocs.Any())
            {
                contextText += "\n--- TÀI LIỆU NỘI BỘ ---\n";
                foreach(var doc in matchedDocs)
                {
                    contextText += $"- Tiêu đề: {doc.Title}, Nội dung/Mô tả: {doc.Description}\n";
                    docLinks += $"- [{doc.Title}]({doc.FilePath})\n";
                }
            }

            // 2. LẤY TOÀN BỘ DATA TỪ DB (Khách hàng, Dự án, Hợp đồng, Nhân viên)
            var customers = await _context.KhachHangs.Where(k => !k.IsDeleted)
                .Select(k => new { k.CompanyName, k.Representative, k.Phone }).ToListAsync();
            var projects = await _context.DuAns.Include(d => d.TrangThaiDuAn).Where(d => !d.IsDeleted)
                .Select(d => new { d.ProjectName, Status = d.TrangThaiDuAn.StatusName, d.Budget }).ToListAsync();
            var contracts = await _context.HopDongs.Include(h => h.KhachHang).Where(h => !h.IsDeleted)
                .Select(h => new { h.ContractName, Customer = h.KhachHang.CompanyName, h.ContractValue }).ToListAsync();
            var employees = await _context.NhanVienPhuTrachs.Where(e => e.IsActive)
                .Select(e => new { e.FullName, e.Department, e.Position }).ToListAsync();

            contextText += "\n--- DỮ LIỆU CƠ SỞ (DATABASE SUMMARY) ---\n";
            contextText += $"Khách hàng: {JsonSerializer.Serialize(customers)}\n";
            contextText += $"Dự án: {JsonSerializer.Serialize(projects)}\n";
            contextText += $"Hợp đồng: {JsonSerializer.Serialize(contracts)}\n";
            contextText += $"Nhân sự: {JsonSerializer.Serialize(employees)}\n";

            // 3. LẤY TOÀN BỘ CẤU TRÚC HTML (CSHTML FILES) ĐỂ HỖ TRỢ UI
            string htmlContext = "";
            try {
                var viewsPath = @"d:\DATT\MQ_AICRM\Frontend\AI_CRM.WebMvc\Views";
                if (System.IO.Directory.Exists(viewsPath)) {
                    var files = System.IO.Directory.GetFiles(viewsPath, "*.cshtml", System.IO.SearchOption.AllDirectories);
                    foreach(var file in files) {
                        var contentHtml = await System.IO.File.ReadAllTextAsync(file);
                        htmlContext += $"\nFile: {System.IO.Path.GetFileName(file)}\nContent:\n{contentHtml}\n";
                    }
                    if (htmlContext.Length > 10000) {
                        htmlContext = htmlContext.Substring(0, 10000); // Giảm kích thước xuống 10,000 để không vượt quá Quota Free Tier
                    }
                }
            } catch { }

            contextText += $"\n--- CẤU TRÚC GIAO DIỆN PROJECT (HTML/CSHTML) ---\n{htmlContext}\n";

            if (string.IsNullOrEmpty(_geminiApiKey) || _geminiApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return "Xin lỗi, API Key của Gemini chưa được cấu hình. Hệ thống không thể xử lý dữ liệu lớn này.";
            }

            string prompt = $"Bạn là trợ lý AI thông minh của phần mềm MQSoft CRM. Nhiệm vụ của bạn là hỗ trợ nhân viên/khách hàng. Trả lời bằng tiếng Việt, lịch sự, chuyên nghiệp. Dưới đây là câu hỏi của người dùng:\n\n{question}\n\n{contextText}";

            // Danh sách các model fallback (Dùng bản Flash để tiết kiệm Quota cho Free Tier)
            string[] models = new[] { 
                "gemini-2.5-flash", 
                "gemini-2.0-flash", 
                "gemini-flash-latest", 
                "gemini-flash-lite-latest" 
            };

            var requestBody = new {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            string lastError = "";

            foreach (var model in models)
            {
                try
                {
                    string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_geminiApiKey}";
                    var response = await _httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(jsonString);
                        var text = doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text").GetString();

                        if (!string.IsNullOrEmpty(docLinks))
                        {
                            text += "\n\n**Tài liệu đính kèm liên quan:**\n" + docLinks;
                        }

                        return text;
                    }
                    else
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        lastError = $"Model {model} failed with status: {response.StatusCode}. Details: {errorBody}";
                        continue; 
                    }
                }
                catch (Exception ex)
                {
                    lastError = $"Model {model} exception: {ex.Message}";
                    continue; 
                }
            }

            return "Xin lỗi, đã xảy ra lỗi từ phía Google Gemini API sau khi thử nhiều Model khác nhau. Chi tiết lỗi cuối: " + lastError;
        }

        public async Task<bool> RateAnswerAsync(int chatHistoryId, bool isHelpful, string comment)
        {
            var chatHistory = await _context.LichSuHoiDaps.FindAsync(chatHistoryId);
            if (chatHistory == null) return false;

            var rating = new DanhGiaCauTraLoi
            {
                ChatHistoryId = chatHistoryId,
                IsHelpful = isHelpful,
                Comment = comment,
                CreatedDate = DateTime.Now
            };

            _context.DanhGiaCauTraLois.Add(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteChatHistoryAsync(int chatHistoryId)
        {
            var chatHistory = await _context.LichSuHoiDaps
                .Include(c => c.DanhGiaCauTraLois)
                .FirstOrDefaultAsync(c => c.ChatHistoryId == chatHistoryId);
                
            if (chatHistory != null)
            {
                if (chatHistory.DanhGiaCauTraLois.Any())
                {
                    _context.DanhGiaCauTraLois.RemoveRange(chatHistory.DanhGiaCauTraLois);
                }
                _context.LichSuHoiDaps.Remove(chatHistory);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ClearAllChatHistoryAsync(int userId)
        {
            var histories = await _context.LichSuHoiDaps
                .Include(c => c.DanhGiaCauTraLois)
                .Where(c => c.UserId == userId).ToListAsync();
                
            if (histories.Any())
            {
                foreach (var history in histories)
                {
                    if (history.DanhGiaCauTraLois.Any())
                    {
                        _context.DanhGiaCauTraLois.RemoveRange(history.DanhGiaCauTraLois);
                    }
                }
                _context.LichSuHoiDaps.RemoveRange(histories);
                await _context.SaveChangesAsync();
            }
            return true;
        }
    }
}
