using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IProjectService
    {
        Task<List<DuAn>> GetProjectsAsync(string searchString = null, int? statusId = null);
        Task<DuAn> GetProjectByIdAsync(int id);
        Task<DuAn> GetProjectDetailsAsync(int id);
        Task<DuAn> CreateProjectAsync(DuAn project);
        Task<bool> UpdateProjectAsync(DuAn project);
        Task<bool> DeleteProjectAsync(int id);
        Task<List<TrangThaiDuAn>> GetStatusesAsync();
        Task<List<KhachHang>> GetActiveCustomersAsync();
        Task<List<HopDong>> GetActiveContractsAsync();
        Task<byte[]> ExportExcelAsync(string searchString, int? statusId);

        // Phân công nhân sự
        Task<bool> AssignMemberAsync(int projectId, int employeeId, string role);
        Task<bool> RemoveMemberAsync(int assignmentId);
        Task<List<NhanVienPhuTrach>> GetAvailableEmployeesForProjectAsync(int projectId);
    }
}
