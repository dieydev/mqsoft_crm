using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TienDoDuAn>> GetTasksAsync()
        {
            return await _context.TienDoDuAns
                .Include(t => t.DuAn)
                .Include(t => t.NhanVienPhuTrach)
                .ToListAsync();
        }
    }
}
