using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DuAn>> GetProjectsAsync()
        {
            return await _context.DuAns
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDuAn)
                .ToListAsync();
        }
    }
}
