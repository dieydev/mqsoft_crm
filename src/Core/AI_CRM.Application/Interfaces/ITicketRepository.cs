using System.Collections.Generic;
using System.Threading.Tasks;
using AI_CRM.Domain.Entities;

namespace AI_CRM.Application.Interfaces
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<Ticket> GetByIdAsync(int id);
        Task<Ticket> AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
        Task DeleteAsync(int id);
        Task<TicketComment> AddCommentAsync(TicketComment comment);
        Task<IEnumerable<TicketComment>> GetCommentsByTicketIdAsync(int ticketId);
    }
}
