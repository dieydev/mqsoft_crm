using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<NguoiDung>> GetUsersAsync(string searchString, int? roleFilter, int? statusFilter);
        Task<List<VaiTro>> GetRolesAsync();
        Task<(bool Success, string ErrorMessage)> CreateUserAsync(NguoiDung user, string plainPassword);
        Task<NguoiDung?> GetUserByIdAsync(int id);
        Task<(bool Success, string ErrorMessage)> UpdateUserAsync(int id, string username, int roleId, bool isActive);
        Task<NguoiDung?> GetUserDetailsAsync(int id);
        Task<(bool Success, string StatusMessage)> ToggleUserStatusAsync(int id);
        Task<(bool Success, string ErrorMessage)> DeleteUserAsync(int id, string currentUserId);
        Task<(bool Success, VaiTro? NewRole)> ChangeUserRoleAsync(int userId, int newRoleId);
        Task<bool> ResetUserPasswordAsync(int userId, string newPassword);
        Task<NhanVienPhuTrach?> GetUserProfileByEmailAsync(string email);
        Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
