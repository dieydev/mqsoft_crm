using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IContractService
    {
        Task<List<HopDong>> GetContractsAsync(string searchString, int? statusId);
        Task<List<TrangThaiHopDong>> GetStatusesAsync();
        Task<List<KhachHang>> GetActiveCustomersAsync();
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string webRootPath);
        Task<HopDong> CreateContractAsync(HopDong contract);
        Task<HopDong?> GetContractByIdAsync(int id);
        Task<bool> UpdateContractAsync(HopDong contract);
        Task<HopDong?> GetContractDetailsAsync(int id);
        Task<bool> DeleteContractAsync(int id);
        Task<byte[]> ExportExcelAsync(string searchString, int? statusId);
    }
}
