using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;

        public ChatbotService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LichSuHoiDap>> GetChatHistoryAsync()
        {
            return await _context.LichSuHoiDaps
                .Include(c => c.NguoiDung)
                .ToListAsync();
        }
    }
}
