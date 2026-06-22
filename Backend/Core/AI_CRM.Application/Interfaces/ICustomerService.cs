using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<List<KhachHang>> GetCustomersAsync(string searchString, int? statusFilter);
        Task<byte[]> ExportExcelAsync(string searchString, int? statusFilter);
        Task<KhachHang> CreateCustomerAsync(KhachHang customer);
        Task<KhachHang?> GetCustomerByIdAsync(int id);
        Task<bool> UpdateCustomerAsync(KhachHang customer);
        Task<KhachHang?> GetCustomerDetailsAsync(int id);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> CustomerExistsAsync(int id);
    }
}
