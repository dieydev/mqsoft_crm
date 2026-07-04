using AI_CRM.Application.Interfaces;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, string RoleName, int UserId)> ValidateUserAsync(string username, string password)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user != null)
            {
                bool isValidPassword = false;
                try
                {
                    isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                }
                catch (Exception)
                {
                    // Hash không hợp lệ → từ chối đăng nhập (không fallback plaintext)
                    isValidPassword = false;
                }

                if (isValidPassword)
                {
                    return (true, user.VaiTro?.RoleName ?? "User", user.UserId);
                }
            }

            return (false, string.Empty, 0);
        }

        public async Task<string> GetUserRoleByEmailAsync(string email)
        {
            var nhanVien = await _context.NhanVienPhuTrachs
                .Include(nv => nv.NguoiDung)
                .ThenInclude(nd => nd.VaiTro)
                .FirstOrDefaultAsync(nv => nv.Email == email && nv.IsActive);

            if (nhanVien != null && nhanVien.NguoiDung != null)
            {
                return nhanVien.NguoiDung.VaiTro?.RoleName ?? "Employee";
            }

            return "User";
        }
    }
}
