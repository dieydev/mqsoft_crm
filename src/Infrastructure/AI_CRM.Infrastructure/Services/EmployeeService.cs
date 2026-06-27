using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NhanVienPhuTrach>> GetEmployeesAsync(string searchString = null)
        {
            var query = _context.NhanVienPhuTrachs
                .Include(n => n.NguoiDung)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var lowerSearch = searchString.ToLower();
                query = query.Where(n => n.FullName.ToLower().Contains(lowerSearch) ||
                                         n.Email.ToLower().Contains(lowerSearch) ||
                                         n.Phone.Contains(lowerSearch));
            }

            return await query.OrderByDescending(n => n.EmployeeId).ToListAsync();
        }

        public async Task<NhanVienPhuTrach> GetEmployeeByIdAsync(int id)
        {
            return await _context.NhanVienPhuTrachs
                .Include(n => n.NguoiDung)
                .FirstOrDefaultAsync(n => n.EmployeeId == id);
        }

        public async Task<bool> CreateEmployeeAsync(NhanVienPhuTrach employee)
        {
            _context.NhanVienPhuTrachs.Add(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEmployeeAsync(NhanVienPhuTrach employee)
        {
            var existing = await _context.NhanVienPhuTrachs.FindAsync(employee.EmployeeId);
            if (existing == null) return false;

            existing.FullName = employee.FullName;
            existing.Email = employee.Email;
            existing.Phone = employee.Phone;
            existing.Department = employee.Department;
            existing.Position = employee.Position;
            existing.UserId = employee.UserId;
            existing.IsActive = employee.IsActive;

            _context.NhanVienPhuTrachs.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var existing = await _context.NhanVienPhuTrachs.FindAsync(id);
            if (existing == null) return false;

            // Optional: Handle related entities or soft delete
            _context.NhanVienPhuTrachs.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<NguoiDung>> GetAvailableUsersAsync(int? currentEmployeeUserId = null)
        {
            // Get all active users
            var query = _context.NguoiDungs.Where(u => u.IsActive);

            // Get users already assigned to an employee
            var assignedUserIds = await _context.NhanVienPhuTrachs
                .Where(e => e.UserId.HasValue)
                .Select(e => e.UserId.Value)
                .ToListAsync();

            if (currentEmployeeUserId.HasValue)
            {
                assignedUserIds.Remove(currentEmployeeUserId.Value);
            }

            return await query.Where(u => !assignedUserIds.Contains(u.UserId)).ToListAsync();
        }
    }
}
