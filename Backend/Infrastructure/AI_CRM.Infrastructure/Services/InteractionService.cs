using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly ApplicationDbContext _context;

        public InteractionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LichSuLamViec>> GetInteractionsAsync(int? customerId = null)
        {
            var query = _context.LichSuLamViecs
                .Include(l => l.KhachHang)
                .Include(l => l.NhanVienPhuTrach)
                .AsQueryable();

            if (customerId.HasValue && customerId.Value > 0)
            {
                query = query.Where(l => l.CustomerId == customerId.Value);
            }

            return await query.OrderByDescending(l => l.WorkDate).ToListAsync();
        }

        public async Task<LichSuLamViec> GetInteractionByIdAsync(int id)
        {
            return await _context.LichSuLamViecs.FindAsync(id);
        }

        public async Task<bool> CreateInteractionAsync(LichSuLamViec interaction)
        {
            _context.Add(interaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateInteractionAsync(LichSuLamViec interaction)
        {
            try
            {
                var existing = await _context.LichSuLamViecs.AsNoTracking().FirstOrDefaultAsync(l => l.WorkLogId == interaction.WorkLogId);
                if (existing == null) return false;

                _context.Update(interaction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteInteractionAsync(int id)
        {
            var interaction = await _context.LichSuLamViecs.FindAsync(id);
            if (interaction != null)
            {
                _context.LichSuLamViecs.Remove(interaction);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<KhachHang>> GetActiveCustomersAsync()
        {
            return await _context.KhachHangs.Where(k => !k.IsDeleted).ToListAsync();
        }

        public async Task<List<NhanVienPhuTrach>> GetActiveEmployeesAsync()
        {
            return await _context.NhanVienPhuTrachs.Where(e => e.IsActive).ToListAsync();
        }
    }
}
