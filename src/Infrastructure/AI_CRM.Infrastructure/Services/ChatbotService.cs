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
                .Include(c => c.NguoiDung)
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

            string answer = await GenerateAnswerAsync(userId, question);

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

        private async Task<string> GenerateAnswerAsync(int userId, string question)
        {
            var q = question.ToLower();
            
            // 1. TÀI LIỆU NỘI BỘ (Như cũ)
            var documents = await _context.TaiLieuNoiBos
                .Include(d => d.NhomTaiLieu)
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            string contextText = "";
            string docLinks = "";
            
            if (documents.Any())
            {
                contextText += "\n--- TÀI LIỆU NỘI BỘ (Để tra cứu) ---\n";
                foreach(var doc in documents.Take(20))
                {
                    contextText += $"- Tiêu đề: {doc.Title}, Mô tả nội dung: {doc.Description}, Đường dẫn file: {doc.FilePath}\n";
                    docLinks += $"- [{doc.Title}]({doc.FilePath})\n";
                }
            }

            // 2. TỐI ƯU HÓA: TRÍCH XUẤT DỮ LIỆU ĐỘNG (RAG)
            contextText += "\n--- DỮ LIỆU CƠ SỞ CHỌN LỌC ---\n";
            bool hasDbData = false;

            if (q.Contains("khách") || q.Contains("công ty"))
            {
                var customers = await _context.KhachHangs.Where(k => !k.IsDeleted)
                    .OrderByDescending(k => k.CreatedDate)
                    .Select(k => new { k.CustomerCode, k.CompanyName, k.Representative, k.Phone, k.Email, k.Address }).Take(20).ToListAsync();
                var total = await _context.KhachHangs.CountAsync(k => !k.IsDeleted);
                contextText += $"Tổng Khách hàng: {total}. Danh sách (Top 20 mới nhất): {JsonSerializer.Serialize(customers)}\n";
                hasDbData = true;
            }

            if (q.Contains("dự án") || q.Contains("project"))
            {
                var projects = await _context.DuAns.Include(d => d.TrangThaiDuAn).Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.CreatedDate)
                    .Select(d => new { d.ProjectName, Status = d.TrangThaiDuAn.StatusName, d.Budget, d.StartDate, d.EndDate, d.Deadline }).Take(20).ToListAsync();
                var total = await _context.DuAns.CountAsync(d => !d.IsDeleted);
                contextText += $"Tổng Dự án: {total}. Danh sách (Top 20 mới nhất): {JsonSerializer.Serialize(projects)}\n";
                hasDbData = true;
            }

            if (q.Contains("hợp đồng") || q.Contains("contract"))
            {
                var contracts = await _context.HopDongs.Include(h => h.KhachHang).Include(h => h.TrangThaiHopDong).Where(h => !h.IsDeleted)
                    .OrderByDescending(h => h.CreatedDate)
                    .Select(h => new { h.ContractNumber, h.ContractName, Customer = h.KhachHang.CompanyName, Status = h.TrangThaiHopDong.StatusName, h.ContractValue, h.SignDate, h.ExpireDate }).Take(20).ToListAsync();
                var total = await _context.HopDongs.CountAsync(h => !h.IsDeleted);
                contextText += $"Tổng Hợp đồng: {total}. Danh sách (Top 20 mới nhất): {JsonSerializer.Serialize(contracts)}\n";
                hasDbData = true;
            }

            if (q.Contains("nhân sự") || q.Contains("nhân viên") || q.Contains("người"))
            {
                var employees = await _context.NhanVienPhuTrachs.Where(e => e.IsActive)
                    .Select(e => new { e.FullName, e.Department, e.Position }).Take(20).ToListAsync();
                var total = await _context.NhanVienPhuTrachs.CountAsync(e => e.IsActive);
                contextText += $"Tổng Nhân sự: {total}. Danh sách (Top 20): {JsonSerializer.Serialize(employees)}\n";
                hasDbData = true;
            }

            if (!hasDbData)
            {
                var totalCus = await _context.KhachHangs.CountAsync(k => !k.IsDeleted);
                var totalProj = await _context.DuAns.CountAsync(d => !d.IsDeleted);
                var totalCont = await _context.HopDongs.CountAsync(h => !h.IsDeleted);
                contextText += $"Thống kê tổng quan: {totalCus} Khách hàng, {totalProj} Dự án, {totalCont} Hợp đồng.\n";
            }
            
            // Đã gỡ bỏ phần nạp mã nguồn HTML (CSHTML) vì không cần thiết và tốn token.

            if (string.IsNullOrEmpty(_geminiApiKey) || _geminiApiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return "Xin lỗi, API Key của Gemini chưa được cấu hình. Hệ thống không thể xử lý dữ liệu lớn này.";
            }

            string prompt = $@"Bạn là Trợ lý AI thông minh của hệ thống MQSoft CRM (phần mềm quản lý khách hàng chuyên cho lĩnh vực Y tế như HIS, LIS, PACS, EMR). 
Tuyệt đối tuân thủ các quy tắc sau:
1. Nếu người dùng hỏi về các dữ liệu cụ thể (như danh sách khách hàng, thông tin dự án, hợp đồng, nhân sự cụ thể), hãy trả lời dựa trên phần DỮ LIỆU TỪ HỆ THỐNG bên dưới.
2. Nếu thông tin cụ thể mà người dùng hỏi KHÔNG CÓ trong DỮ LIỆU TỪ HỆ THỐNG, bạn hãy trả lời: ""Thông tin này hiện chưa có trong cơ sở dữ liệu hệ thống hoặc tôi không có quyền truy cập"", tuyệt đối không tự bịa ra dữ liệu (hallucinate).
3. Nếu người dùng hỏi các câu hỏi chung chung (ví dụ: chào hỏi, hỏi về tính năng phần mềm CRM, hỏi cách dùng hệ thống, hỏi khái niệm về y tế/công nghệ), bạn ĐƯỢC PHÉP tự do trả lời dựa trên kiến thức của bạn.
4. Trả lời bằng tiếng Việt, lịch sự, chuyên nghiệp, thân thiện và ngắn gọn.
5. Từ chối lịch sự mọi câu hỏi nằm ngoài phạm vi công nghệ, y tế, hoặc quản trị doanh nghiệp.

DỮ LIỆU HIỆN TẠI TỪ HỆ THỐNG:
{contextText}

Câu hỏi của người dùng:
{question}";

            // Lấy 5 tin nhắn gần nhất làm bộ nhớ ngữ cảnh (Memory)
            var history = await _context.LichSuHoiDaps
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .Take(5)
                .ToListAsync();
            history.Reverse(); // Đảo ngược để theo thứ tự thời gian từ cũ đến mới

            var contentsList = new List<object>();
            
            // Đổ lịch sử vào (role: user, role: model)
            foreach(var h in history)
            {
                contentsList.Add(new { role = "user", parts = new[] { new { text = h.Question } } });
                contentsList.Add(new { role = "model", parts = new[] { new { text = h.Answer } } });
            }
            
            // Câu hỏi hiện tại
            contentsList.Add(new { role = "user", parts = new[] { new { text = prompt } } });

            // Danh sách các model fallback (Dùng bản Flash để tiết kiệm Quota cho Free Tier)
            string[] models = new[] { 
                "gemini-2.5-flash", 
                "gemini-2.0-flash", 
                "gemini-flash-latest", 
                "gemini-flash-lite-latest" 
            };

            var requestBody = new {
                contents = contentsList
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
