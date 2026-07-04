using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NguoiDung>> GetUsersAsync(string searchString, int? roleFilter, int? statusFilter)
        {
            var query = _context.NguoiDungs
                .Include(u => u.VaiTro)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.Username.Contains(searchString));
            }

            if (roleFilter.HasValue && roleFilter.Value > 0)
            {
                query = query.Where(u => u.RoleId == roleFilter.Value);
            }

            if (statusFilter.HasValue)
            {
                if (statusFilter == 1) query = query.Where(u => u.IsActive);
                if (statusFilter == 2) query = query.Where(u => !u.IsActive);
            }

            return await query.OrderByDescending(u => u.CreatedDate).ToListAsync();
        }

        public async Task<List<VaiTro>> GetRolesAsync()
        {
            return await _context.VaiTros.ToListAsync();
        }

        public async Task<(bool Success, string ErrorMessage)> CreateUserAsync(NguoiDung user, string plainPassword)
        {
            var existingUser = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
            {
                return (false, "Tên đăng nhập đã tồn tại trong hệ thống.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            user.CreatedDate = DateTime.Now;

            _context.NguoiDungs.Add(user);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        public async Task<NguoiDung?> GetUserByIdAsync(int id)
        {
            return await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateUserAsync(int id, string username, int roleId, bool isActive)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            var existingUser = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.Username == username && u.UserId != id);
            if (existingUser != null)
            {
                return (false, "Tên đăng nhập đã tồn tại trong hệ thống.");
            }

            user.Username = username;
            user.RoleId = roleId;
            user.IsActive = isActive;

            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<NguoiDung?> GetUserDetailsAsync(int id)
        {
            return await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .Include(u => u.NhanVienPhuTrachs)
                .Include(u => u.NhatKyHeThongs.OrderByDescending(n => n.Timestamp).Take(10))
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<(bool Success, string StatusMessage)> ToggleUserStatusAsync(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            var status = user.IsActive ? "kích hoạt" : "khóa";
            return (true, $"Đã {status} tài khoản \"{user.Username}\" thành công!");
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteUserAsync(int id, string currentUserId)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            if (currentUserId == id.ToString())
            {
                return (false, "Bạn không thể xóa chính tài khoản của mình!");
            }

            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        public async Task<(bool Success, VaiTro? NewRole)> ChangeUserRoleAsync(int userId, int newRoleId)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return (false, null);

            user.RoleId = newRoleId;
            await _context.SaveChangesAsync();

            var newRole = await _context.VaiTros.FindAsync(newRoleId);
            return (true, newRole);
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<NhanVienPhuTrach?> GetUserProfileByEmailAsync(string email)
        {
            return await _context.NhanVienPhuTrachs
                .Include(nv => nv.NguoiDung)
                .ThenInclude(nd => nd.VaiTro)
                .FirstOrDefaultAsync(nv => nv.Email == email);
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.NguoiDungs.FindAsync(userId);
            if (user == null) return (false, "Tài khoản không tồn tại.");

            bool isValidOldPassword = false;
            try
            {
                isValidOldPassword = BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash);
            }
            catch
            {
                // Hash không hợp lệ → từ chối (không fallback plaintext)
                isValidOldPassword = false;
            }

            if (!isValidOldPassword)
            {
                return (false, "Mật khẩu hiện tại không đúng.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }
    }
}
