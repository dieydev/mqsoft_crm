using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI_CRM.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToEmployee)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ticket> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToEmployee)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket> AddAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TicketComment> AddCommentAsync(TicketComment comment)
        {
            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<IEnumerable<TicketComment>> GetCommentsByTicketIdAsync(int ticketId)
        {
            return await _context.TicketComments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
