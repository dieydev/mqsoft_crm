using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface ITaskService
    {
        Task<List<TienDoDuAn>> GetTasksAsync();
    }
}
