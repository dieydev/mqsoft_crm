using AI_CRM.AI.Interfaces;
using AI_CRM.AI.Models;
using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiAssistantService _aiAssistantService;

        public ChatbotService(ApplicationDbContext context, IAiAssistantService aiAssistantService)
        {
            _context = context;
            _aiAssistantService = aiAssistantService;
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
            
            // 1. TÀI LIỆU NỘI BỘ (RAG Context)
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
                    contextText += $"- Tiêu đề: {doc.Title}, Mô tả nội dung: {doc.Description}, Đường dẫn: {doc.FilePath}\n";
                    docLinks += $"- [{doc.Title}]({doc.FilePath})\n";
                }
            }

            // 2. TRÍCH XUẤT DỮ LIỆU ĐỘNG (RAG)
            contextText += "\n--- DỮ LIỆU CƠ SỞ CHỌN LỌC ---\n";
            bool hasDbData = false;

            if (q.Contains("khách") || q.Contains("công ty") || q.Contains("bệnh viện") || q.Contains("phòng khám"))
            {
                var customers = await _context.KhachHangs.Where(k => !k.IsDeleted)
                    .OrderByDescending(k => k.CreatedDate)
                    .Select(k => new { k.CustomerCode, k.CompanyName, k.FacilityCode, k.HospitalLevel, k.HospitalClass, k.BedCount, k.CurrentSoftware, k.Representative, k.Phone, k.Email }).Take(20).ToListAsync();
                var total = await _context.KhachHangs.CountAsync(k => !k.IsDeleted);
                contextText += $"Tổng Khách hàng (CSKCB): {total}. Danh sách (Top 20 mới nhất): {JsonSerializer.Serialize(customers)}\n";
                hasDbData = true;
            }

            if (q.Contains("dự án") || q.Contains("project") || q.Contains("tiến độ"))
            {
                var projects = await _context.DuAns.Include(d => d.TrangThaiDuAn).Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.CreatedDate)
                    .Select(d => new { d.ProjectName, Status = d.TrangThaiDuAn.StatusName, d.Budget, d.StartDate, d.EndDate, d.Deadline }).Take(20).ToListAsync();
                var total = await _context.DuAns.CountAsync(d => !d.IsDeleted);
                contextText += $"Tổng Dự án: {total}. Danh sách (Top 20 mới nhất): {JsonSerializer.Serialize(projects)}\n";
                hasDbData = true;
            }

            if (q.Contains("hợp đồng") || q.Contains("contract") || q.Contains("giá trị"))
            {
                var contracts = await _context.HopDongs.Include(h => h.KhachHang).Include(h => h.TrangThaiHopDong).Where(h => !h.IsDeleted)
                    .OrderByDescending(h => h.CreatedDate)
                    .Select(h => new { h.ContractNumber, h.ContractName, Customer = h.KhachHang.CompanyName, Status = h.TrangThaiHopDong.StatusName, h.ContractValue, h.SignDate, h.ExpireDate }).Take(20).ToListAsync();
                var total = await _context.HopDongs.CountAsync(h => !h.IsDeleted);
                contextText += $"Tổng Hợp đồng: {total}. Danh sách (Top 20 mới nhất): {JsonSerializer.Serialize(contracts)}\n";
                hasDbData = true;
            }

            if (q.Contains("nhân sự") || q.Contains("nhân viên") || q.Contains("người") || q.Contains("kỹ thuật"))
            {
                var employees = await _context.NhanVienPhuTrachs.Where(e => e.IsActive)
                    .Select(e => new { e.FullName, e.Department, e.Position }).Take(20).ToListAsync();
                var total = await _context.NhanVienPhuTrachs.CountAsync(e => e.IsActive);
                contextText += $"Tổng Nhân sự: {total}. Danh sách (Top 20): {JsonSerializer.Serialize(employees)}\n";
                hasDbData = true;
            }

            if (q.Contains("sự cố") || q.Contains("ticket") || q.Contains("lỗi") || q.Contains("hỗ trợ"))
            {
                var tickets = await _context.Tickets.Include(t => t.Customer).OrderByDescending(t => t.CreatedAt)
                    .Select(t => new { t.Id, t.Title, Customer = t.Customer.CompanyName, Priority = t.Priority.ToString(), Status = t.Status.ToString(), t.CreatedAt }).Take(20).ToListAsync();
                contextText += $"Sự cố Ticket gần nhất: {JsonSerializer.Serialize(tickets)}\n";
                hasDbData = true;
            }

            if (!hasDbData)
            {
                var totalCus = await _context.KhachHangs.CountAsync(k => !k.IsDeleted);
                var totalProj = await _context.DuAns.CountAsync(d => !d.IsDeleted);
                var totalCont = await _context.HopDongs.CountAsync(h => !h.IsDeleted);
                var totalTicket = await _context.Tickets.CountAsync();
                contextText += $"Thống kê tổng quan: {totalCus} Khách hàng CSKCB, {totalProj} Dự án phần mềm, {totalCont} Hợp đồng, {totalTicket} Sự cố Ticket.\n";
            }

            // Lấy 5 tin nhắn gần nhất làm bộ nhớ ngữ cảnh (Memory)
            var history = await _context.LichSuHoiDaps
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .Take(5)
                .ToListAsync();
            history.Reverse();

            var historyItems = history.SelectMany(h => new[]
            {
                new ChatMessageItem { Role = "user", Content = h.Question },
                new ChatMessageItem { Role = "model", Content = h.Answer }
            }).ToList();

            string responseText = await _aiAssistantService.ChatAsync(question, contextText, historyItems);

            if (!string.IsNullOrEmpty(docLinks) && !responseText.Contains("Tài liệu đính kèm liên quan"))
            {
                responseText += "\n\n**Tài liệu tham khảo liên quan:**\n" + docLinks;
            }

            return responseText;
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
