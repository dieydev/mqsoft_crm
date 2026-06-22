using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IAdminService
    {
        Task<int> GetTotalCustomersAsync();
        Task<int> GetActiveProjectsAsync();
        Task<int> GetTotalContractsAsync();
        Task<int> GetTotalChatbotQueriesAsync();
    }
}
