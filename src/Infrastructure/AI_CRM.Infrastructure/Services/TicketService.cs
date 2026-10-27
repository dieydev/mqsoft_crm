using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;

namespace AI_CRM.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly AI_CRM.AI.Interfaces.IAiAssistantService _aiAssistantService;
        private readonly IDocumentService _documentService;

        public TicketService(
            ITicketRepository ticketRepository,
            AI_CRM.AI.Interfaces.IAiAssistantService aiAssistantService,
            IDocumentService documentService)
        {
            _ticketRepository = ticketRepository;
            _aiAssistantService = aiAssistantService;
            _documentService = documentService;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(string searchString = null, TicketStatus? status = null, TicketPriority? priority = null)
        {
            var tickets = await _ticketRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var search = searchString.Trim().ToLower();
                tickets = tickets.Where(t => 
                    (t.Title != null && t.Title.ToLower().Contains(search)) ||
                    (t.Description != null && t.Description.ToLower().Contains(search)) ||
                    (t.Customer != null && t.Customer.CompanyName.ToLower().Contains(search)) ||
                    (t.AssignedToEmployee != null && t.AssignedToEmployee.FullName.ToLower().Contains(search))
                );
            }

            if (status.HasValue)
            {
                tickets = tickets.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                tickets = tickets.Where(t => t.Priority == priority.Value);
            }

            return tickets;
        }

        public async Task<Ticket> GetTicketByIdAsync(int id)
        {
            return await _ticketRepository.GetByIdAsync(id);
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket, int? currentUserId)
        {
            ticket.CreatedAt = DateTime.Now;
            ticket.CreatedByUserId = currentUserId;
            ticket.Status = TicketStatus.New;
            ticket.SlaDeadline = CalculateSlaDeadline(ticket.Priority, ticket.CreatedAt);

            return await _ticketRepository.AddAsync(ticket);
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            var existing = await _ticketRepository.GetByIdAsync(ticket.Id);
            if (existing != null)
            {
                existing.Title = ticket.Title;
                existing.Description = ticket.Description;
                existing.CustomerId = ticket.CustomerId;
                existing.AssignedToEmployeeId = ticket.AssignedToEmployeeId;
                
                // Cập nhật lại SLA nếu thay đổi mức độ ưu tiên
                if (existing.Priority != ticket.Priority)
                {
                    existing.Priority = ticket.Priority;
                    existing.SlaDeadline = CalculateSlaDeadline(ticket.Priority, existing.CreatedAt);
                }

                // Cập nhật trạng thái
                if (existing.Status != ticket.Status)
                {
                    existing.Status = ticket.Status;
                    if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
                    {
                        existing.ResolvedAt = DateTime.Now;
                    }
                    else
                    {
                        existing.ResolvedAt = null;
                    }
                }

                await _ticketRepository.UpdateAsync(existing);
            }
        }

        public async Task DeleteTicketAsync(int id)
        {
            await _ticketRepository.DeleteAsync(id);
        }

        public async Task AssignTechnicianAsync(int ticketId, int employeeId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket != null)
            {
                ticket.AssignedToEmployeeId = employeeId;
                if (ticket.Status == TicketStatus.New)
                {
                    ticket.Status = TicketStatus.InProgress;
                }
                await _ticketRepository.UpdateAsync(ticket);
            }
        }

        public async Task UpdateStatusAsync(int ticketId, TicketStatus status)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket != null)
            {
                ticket.Status = status;
                if (status == TicketStatus.Resolved || status == TicketStatus.Closed)
                {
                    ticket.ResolvedAt = DateTime.Now;
                }
                else
                {
                    ticket.ResolvedAt = null;
                }
                await _ticketRepository.UpdateAsync(ticket);
            }
        }

        public async Task<TicketComment> AddCommentAsync(int ticketId, int? userId, string content)
        {
            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            return await _ticketRepository.AddCommentAsync(comment);
        }

        public DateTime CalculateSlaDeadline(TicketPriority priority, DateTime createdAt)
        {
            return priority switch
            {
                TicketPriority.Critical => createdAt.AddHours(4),   // Khẩn cấp: xử lý trong 4 giờ
                TicketPriority.High => createdAt.AddHours(12),       // Mức cao: xử lý trong 12 giờ
                TicketPriority.Medium => createdAt.AddHours(24),     // Trung bình: xử lý trong 24 giờ
                TicketPriority.Low => createdAt.AddHours(48),        // Mức thấp: xử lý trong 48 giờ
                _ => createdAt.AddHours(24)
            };
        }

        public string GetSlaStatus(Ticket ticket)
        {
            if (ticket == null || !ticket.SlaDeadline.HasValue) return "Không áp dụng";

            if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            {
                var resolvedTime = ticket.ResolvedAt ?? DateTime.Now;
                return resolvedTime <= ticket.SlaDeadline.Value ? "Đạt chuẩn SLA" : "Trễ hạn SLA";
            }

            var now = DateTime.Now;
            if (now > ticket.SlaDeadline.Value)
            {
                var overdueSpan = now - ticket.SlaDeadline.Value;
                int hours = (int)overdueSpan.TotalHours;
                return hours > 0 ? $"Vi phạm SLA (Quá hạn {hours}h)" : "Vi phạm SLA";
            }

            var remainSpan = ticket.SlaDeadline.Value - now;
            int remainHours = (int)remainSpan.TotalHours;
            if (remainHours <= 2)
            {
                return $"Sắp hết hạn SLA (Còn {remainHours}h)";
            }

            return $"Đúng hạn (Còn {remainHours}h)";
        }

        public async Task<string> GetTicketAiSolutionAsync(int ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return "Không tìm thấy thông tin sự cố Ticket.";
            }

            // Lấy các tài liệu kỹ thuật nội bộ liên quan
            var docs = await _documentService.GetDocumentsAsync();
            var relevantDocs = docs.Take(5).Select(d => $"- {d.Title}: {d.Description}");

            var context = new AI_CRM.AI.Models.TicketAiContext
            {
                TicketId = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                CustomerName = ticket.Customer?.CompanyName ?? "Khách hàng y tế",
                Priority = ticket.Priority.ToString(),
                CurrentSoftware = ticket.Customer?.CurrentSoftware,
                RelevantKnowledgeBase = string.Join("\n", relevantDocs)
            };

            var result = await _aiAssistantService.AnalyzeTicketSolutionAsync(context);
            return result.IsSuccess ? result.Content : $"Lỗi khi phân tích giải pháp: {result.ErrorMessage}";
        }
    }
}
