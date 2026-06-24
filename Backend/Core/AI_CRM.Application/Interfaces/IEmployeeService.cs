using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<NhanVienPhuTrach>> GetEmployeesAsync(string searchString = null);
        Task<NhanVienPhuTrach> GetEmployeeByIdAsync(int id);
        Task<bool> CreateEmployeeAsync(NhanVienPhuTrach employee);
        Task<bool> UpdateEmployeeAsync(NhanVienPhuTrach employee);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<List<NguoiDung>> GetAvailableUsersAsync(int? currentEmployeeUserId = null);
    }
}
