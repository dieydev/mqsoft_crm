using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface ITaskService
    {
        Task<List<TienDoDuAn>> GetTasksAsync(int? projectId = null);
        Task<TienDoDuAn> GetTaskByIdAsync(int id);
        Task<bool> CreateTaskAsync(TienDoDuAn task);
        Task<bool> UpdateTaskAsync(TienDoDuAn task);
        Task<bool> DeleteTaskAsync(int id);
        Task<List<DuAn>> GetActiveProjectsAsync();
        Task<List<NhanVienPhuTrach>> GetActiveEmployeesAsync();
    }
}
