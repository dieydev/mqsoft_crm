using AI_CRM.Application.Interfaces;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context.KhachHangs.CountAsync(k => !k.IsDeleted);
        }

        public async Task<int> GetActiveProjectsAsync()
        {
            return await _context.DuAns.CountAsync(d => !d.IsDeleted);
        }

        public async Task<int> GetTotalContractsAsync()
        {
            return await _context.HopDongs.CountAsync(h => !h.IsDeleted);
        }

        public async Task<int> GetTotalChatbotQueriesAsync()
        {
            return await _context.LichSuHoiDaps.CountAsync();
        }

        public async Task<System.Collections.Generic.Dictionary<string, decimal>> GetRevenueByMonthAsync()
        {
            var dict = new System.Collections.Generic.Dictionary<string, decimal>();
            var today = System.DateTime.Today;
            
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                string monthLabel = $"Tháng {monthDate.Month}";
                
                var revenue = await _context.HopDongs
                    .Where(h => !h.IsDeleted && h.SignDate.HasValue && h.SignDate.Value.Month == monthDate.Month && h.SignDate.Value.Year == monthDate.Year)
                    .SumAsync(h => h.ContractValue ?? 0);
                    
                dict.Add(monthLabel, revenue / 1000000); // Triệu VNĐ
            }
            return dict;
        }

        public async Task<System.Collections.Generic.Dictionary<string, int>> GetProjectStatusDistributionAsync()
        {
            var dict = new System.Collections.Generic.Dictionary<string, int>();
            var distribution = await _context.DuAns
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.TrangThaiDuAn.StatusName)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in distribution)
            {
                dict.Add(item.Status ?? "Không rõ", item.Count);
            }
            return dict;
        }

        public async Task<System.Collections.Generic.Dictionary<string, decimal>> GetTopContractsAsync()
        {
            var contracts = await _context.HopDongs
                .Where(h => !h.IsDeleted)
                .Include(h => h.KhachHang)
                .OrderByDescending(h => h.ContractValue)
                .Take(5)
                .ToListAsync();
                
            var dict = new System.Collections.Generic.Dictionary<string, decimal>();
            foreach (var contract in contracts)
            {
                var label = contract.KhachHang != null ? contract.KhachHang.CompanyName : contract.ContractName;
                if(string.IsNullOrEmpty(label)) label = "N/A";
                if(label.Length > 20) label = label.Substring(0, 17) + "...";
                
                if (!dict.ContainsKey(label)) {
                    dict.Add(label, (contract.ContractValue ?? 0) / 1000000000); // Tỷ VNĐ
                }
            }
            return dict;
        }

        public async Task<System.Collections.Generic.Dictionary<string, int>> GetEmployeeAllocationAsync()
        {
            var allocations = await _context.DuAnNhanViens
                .GroupBy(d => d.ProjectRole)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();
                
            var dict = new System.Collections.Generic.Dictionary<string, int>();
            foreach(var item in allocations)
            {
                var roleName = string.IsNullOrEmpty(item.Role) ? "Khác" : item.Role;
                if (!dict.ContainsKey(roleName)) {
                    dict.Add(roleName, item.Count);
                }
            }
            return dict;
        }
    }
}
