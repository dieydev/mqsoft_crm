using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IAdminService
    {
        Task<int> GetTotalCustomersAsync();
        Task<int> GetActiveProjectsAsync();
        Task<int> GetTotalContractsAsync();
        Task<int> GetTotalChatbotQueriesAsync();
        
        Task<System.Collections.Generic.Dictionary<string, decimal>> GetRevenueByMonthAsync();
        Task<System.Collections.Generic.Dictionary<string, int>> GetProjectStatusDistributionAsync();
        Task<System.Collections.Generic.Dictionary<string, decimal>> GetTopContractsAsync();
        Task<System.Collections.Generic.Dictionary<string, int>> GetEmployeeAllocationAsync();
        Task<System.Collections.Generic.List<AI_CRM.Domain.Entities.NhatKyHeThong>> GetRecentActivitiesAsync(int count);
        Task<System.Collections.Generic.List<AI_CRM.Domain.Entities.NhatKyHeThong>> GetAllSystemLogsAsync(int limit = 500);
    }
}
