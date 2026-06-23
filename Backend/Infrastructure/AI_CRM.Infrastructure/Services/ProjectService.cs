using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DuAn>> GetProjectsAsync(string searchString = null, int? statusId = null)
        {
            var query = _context.DuAns
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDuAn)
                .Include(d => d.HopDong)
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.ProjectName.Contains(searchString) || 
                                         d.KhachHang.CompanyName.Contains(searchString));
            }
            
            if (statusId.HasValue && statusId.Value > 0)
            {
                query = query.Where(d => d.StatusId == statusId.Value);
            }

            return await query.OrderByDescending(d => d.CreatedDate).ToListAsync();
        }

        public async Task<DuAn> GetProjectByIdAsync(int id)
        {
            return await _context.DuAns.FindAsync(id);
        }

        public async Task<DuAn> GetProjectDetailsAsync(int id)
        {
            return await _context.DuAns
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDuAn)
                .Include(d => d.HopDong)
                .Include(d => d.DuAnNhanViens).ThenInclude(dn => dn.NhanVienPhuTrach)
                .Include(d => d.TienDoDuAns).ThenInclude(td => td.NhanVienPhuTrach)
                .FirstOrDefaultAsync(d => d.ProjectId == id && !d.IsDeleted);
        }

        public async Task<DuAn> CreateProjectAsync(DuAn project)
        {
            project.CreatedDate = System.DateTime.Now;
            _context.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> UpdateProjectAsync(DuAn project)
        {
            try
            {
                var existing = await _context.DuAns.AsNoTracking().FirstOrDefaultAsync(d => d.ProjectId == project.ProjectId);
                if (existing == null) return false;

                project.CreatedDate = existing.CreatedDate;
                _context.Update(project);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.DuAns.AnyAsync(e => e.ProjectId == project.ProjectId))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.DuAns.FindAsync(id);
            if (project != null)
            {
                project.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<TrangThaiDuAn>> GetStatusesAsync()
        {
            return await _context.TrangThaiDuAns.ToListAsync();
        }

        public async Task<List<KhachHang>> GetActiveCustomersAsync()
        {
            return await _context.KhachHangs.Where(k => !k.IsDeleted).ToListAsync();
        }

        public async Task<List<HopDong>> GetActiveContractsAsync()
        {
            return await _context.HopDongs.Where(h => !h.IsDeleted).ToListAsync();
        }

        public async Task<byte[]> ExportExcelAsync(string searchString, int? statusId)
        {
            var projects = await GetProjectsAsync(searchString, statusId);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Tên dự án,Khách hàng,Hợp đồng,Ngày bắt đầu,Deadline,Ngân sách (VNĐ),Trạng thái");

            foreach (var d in projects)
            {
                var startDate = d.StartDate?.ToString("dd/MM/yyyy") ?? "";
                var deadline = d.Deadline?.ToString("dd/MM/yyyy") ?? "";
                var status = d.TrangThaiDuAn?.StatusName ?? "";
                var budget = d.Budget?.ToString("N0") ?? "0";
                
                builder.AppendLine($"\"{d.ProjectName}\",\"{d.KhachHang?.CompanyName}\",\"{d.HopDong?.ContractNumber}\",\"{startDate}\",\"{deadline}\",\"{budget}\",\"{status}\"");
            }

            return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        }
    }
}
