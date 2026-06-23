using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IInteractionService
    {
        Task<List<LichSuLamViec>> GetInteractionsAsync(int? customerId = null);
        Task<LichSuLamViec> GetInteractionByIdAsync(int id);
        Task<bool> CreateInteractionAsync(LichSuLamViec interaction);
        Task<bool> UpdateInteractionAsync(LichSuLamViec interaction);
        Task<bool> DeleteInteractionAsync(int id);
        Task<List<KhachHang>> GetActiveCustomersAsync();
        Task<List<NhanVienPhuTrach>> GetActiveEmployeesAsync();
    }
}
