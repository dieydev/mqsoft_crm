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

        public async Task<List<TienDoDuAn>> GetTasksAsync(int? projectId = null)
        {
            var query = _context.TienDoDuAns
                .Include(t => t.DuAn)
                .Include(t => t.NhanVienPhuTrach)
                .AsQueryable();

            if (projectId.HasValue && projectId.Value > 0)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            return await query.OrderByDescending(t => t.UpdateDate).ToListAsync();
        }

        public async Task<TienDoDuAn> GetTaskByIdAsync(int id)
        {
            return await _context.TienDoDuAns.FindAsync(id);
        }

        public async Task<bool> CreateTaskAsync(TienDoDuAn task)
        {
            task.UpdateDate = System.DateTime.Now;
            _context.Add(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTaskAsync(TienDoDuAn task)
        {
            try
            {
                var existing = await _context.TienDoDuAns.AsNoTracking().FirstOrDefaultAsync(t => t.ProgressId == task.ProgressId);
                if (existing == null) return false;

                task.UpdateDate = existing.UpdateDate; // keep original date or maybe update it? Let's update it.
                task.UpdateDate = System.DateTime.Now;
                
                _context.Update(task);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.TienDoDuAns.FindAsync(id);
            if (task != null)
            {
                _context.TienDoDuAns.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<DuAn>> GetActiveProjectsAsync()
        {
            return await _context.DuAns.Where(d => !d.IsDeleted).ToListAsync();
        }

        public async Task<List<NhanVienPhuTrach>> GetActiveEmployeesAsync()
        {
            return await _context.NhanVienPhuTrachs.Where(e => e.IsActive).ToListAsync();
        }
    }
}
