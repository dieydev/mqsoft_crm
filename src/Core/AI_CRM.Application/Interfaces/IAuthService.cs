using AI_CRM.Domain.Entities;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IAuthService
    {
        Task<(bool IsValid, string RoleName, int UserId)> ValidateUserAsync(string username, string password);
        Task<(string RoleName, int UserId)> GetUserRoleByEmailAsync(string email);
    }
}
