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

        public async Task<List<LichSuLamViec>> GetInteractionsAsync()
        {
            return await _context.LichSuLamViecs
                .Include(l => l.KhachHang)
                .Include(l => l.NhanVienPhuTrach)
                .ToListAsync();
        }
    }
}
