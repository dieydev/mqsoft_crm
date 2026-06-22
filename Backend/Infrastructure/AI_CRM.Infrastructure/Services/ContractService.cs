using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _context;

        public ContractService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<HopDong>> GetContractsAsync(string searchString, int? statusId)
        {
            var query = _context.HopDongs
                .Include(h => h.KhachHang)
                .Include(h => h.TrangThaiHopDong)
                .Where(h => !h.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.ContractNumber.Contains(searchString) || h.ContractName.Contains(searchString) || h.KhachHang.CompanyName.Contains(searchString));
            }
            if (statusId.HasValue && statusId.Value > 0)
            {
                query = query.Where(h => h.StatusId == statusId.Value);
            }

            return await query.OrderByDescending(h => h.CreatedDate).ToListAsync();
        }

        public async Task<List<TrangThaiHopDong>> GetStatusesAsync()
        {
            return await _context.TrangThaiHopDongs.ToListAsync();
        }

        public async Task<List<KhachHang>> GetActiveCustomersAsync()
        {
            return await _context.KhachHangs.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string webRootPath)
        {
            string uploadsFolder = Path.Combine(webRootPath, "uploads", "contracts");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var newFileStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(newFileStream);
            }
            
            return "/uploads/contracts/" + uniqueFileName;
        }

        public async Task<HopDong> CreateContractAsync(HopDong contract)
        {
            contract.CreatedDate = DateTime.Now;
            _context.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<HopDong?> GetContractByIdAsync(int id)
        {
            return await _context.HopDongs.FindAsync(id);
        }

        public async Task<bool> UpdateContractAsync(HopDong contract)
        {
            try
            {
                var existingContract = await _context.HopDongs.AsNoTracking().FirstOrDefaultAsync(h => h.ContractId == contract.ContractId);
                if (existingContract == null) return false;

                contract.CreatedDate = existingContract.CreatedDate;
                if (string.IsNullOrEmpty(contract.FilePath))
                {
                    contract.FilePath = existingContract.FilePath;
                }

                _context.Update(contract);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ContractExistsAsync(contract.ContractId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<HopDong?> GetContractDetailsAsync(int id)
        {
            return await _context.HopDongs
                .Include(h => h.KhachHang)
                .Include(h => h.TrangThaiHopDong)
                .Include(h => h.DuAns)
                .FirstOrDefaultAsync(m => m.ContractId == id);
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong != null)
            {
                hopDong.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<byte[]> ExportExcelAsync(string searchString, int? statusId)
        {
            var contracts = await GetContractsAsync(searchString, statusId);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Số hợp đồng,Khách hàng,Tên hợp đồng,Giá trị (VNĐ),Ngày ký,Hết hạn,Trạng thái");

            foreach (var h in contracts)
            {
                var signDate = h.SignDate?.ToString("dd/MM/yyyy") ?? "";
                var expDate = h.ExpireDate?.ToString("dd/MM/yyyy") ?? "";
                var status = h.TrangThaiHopDong?.StatusName ?? "";
                var val = h.ContractValue?.ToString("N0") ?? "0";
                
                builder.AppendLine($"\"{h.ContractNumber}\",\"{h.KhachHang?.CompanyName}\",\"{h.ContractName}\",\"{val}\",\"{signDate}\",\"{expDate}\",\"{status}\"");
            }

            return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        }

        private async Task<bool> ContractExistsAsync(int id)
        {
            return await _context.HopDongs.AnyAsync(e => e.ContractId == id);
        }
    }
}
