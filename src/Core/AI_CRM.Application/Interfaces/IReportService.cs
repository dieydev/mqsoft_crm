using System;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> ExportCustomersToExcelAsync();
        Task<byte[]> ExportProjectsToExcelAsync();
        Task<byte[]> ExportContractsToExcelAsync();
        
        Task<byte[]> ExportCustomerReportToPdfAsync(int customerId);
        Task<byte[]> ExportProjectReportToPdfAsync(int projectId);
        Task<byte[]> ExportContractReportToPdfAsync(int contractId);
        Task<byte[]> ExportProgressReportToPdfAsync(int projectId);
    }
}
