using AI_CRM.Application.Interfaces;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context.KhachHangs.CountAsync(k => !k.IsDeleted);
        }

        public async Task<int> GetActiveProjectsAsync()
        {
            return await _context.DuAns.CountAsync(d => !d.IsDeleted);
        }

        public async Task<int> GetTotalContractsAsync()
        {
            return await _context.HopDongs.CountAsync(h => !h.IsDeleted);
        }

        public async Task<int> GetTotalChatbotQueriesAsync()
        {
            return await _context.LichSuHoiDaps.CountAsync();
        }
    }
}
