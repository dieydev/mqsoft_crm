using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AI_CRM.Domain.Entities;

namespace AI_CRM.Application.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetTicketsAsync(string searchString = null, TicketStatus? status = null, TicketPriority? priority = null);
        Task<Ticket> GetTicketByIdAsync(int id);
        Task<Ticket> CreateTicketAsync(Ticket ticket, int? currentUserId);
        Task UpdateTicketAsync(Ticket ticket);
        Task DeleteTicketAsync(int id);
        Task AssignTechnicianAsync(int ticketId, int employeeId);
        Task UpdateStatusAsync(int ticketId, TicketStatus status);
        Task<TicketComment> AddCommentAsync(int ticketId, int? userId, string content);
        DateTime CalculateSlaDeadline(TicketPriority priority, DateTime createdAt);
        string GetSlaStatus(Ticket ticket);
    }
}
