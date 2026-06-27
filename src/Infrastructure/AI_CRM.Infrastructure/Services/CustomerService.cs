using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_CRM.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<KhachHang>> GetCustomersAsync(string searchString, int? statusFilter)
        {
            var query = _context.KhachHangs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(k => k.CompanyName.Contains(searchString) || 
                                         k.Phone.Contains(searchString) || 
                                         k.Email.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                if (statusFilter == 1) query = query.Where(k => !k.IsDeleted);
                if (statusFilter == 2) query = query.Where(k => k.IsDeleted);
            }

            return await query.OrderByDescending(k => k.CreatedDate).ToListAsync();
        }

        public async Task<byte[]> ExportExcelAsync(string searchString, int? statusFilter)
        {
            var customers = await GetCustomersAsync(searchString, statusFilter);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Mã KH,Tên doanh nghiệp,Người đại diện,Điện thoại,Email,Địa chỉ,Trạng thái");

            foreach (var cus in customers)
            {
                var status = cus.IsDeleted ? "Ngừng hoạt động" : "Hoạt động";
                builder.AppendLine($"\"{cus.CustomerCode}\",\"{cus.CompanyName}\",\"{cus.Representative}\",\"{cus.Phone}\",\"{cus.Email}\",\"{cus.Address}\",\"{status}\"");
            }

            return System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        }

        public async Task<KhachHang> CreateCustomerAsync(KhachHang customer)
        {
            customer.CreatedDate = System.DateTime.Now;
            customer.IsDeleted = false;
            _context.KhachHangs.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<KhachHang?> GetCustomerByIdAsync(int id)
        {
            return await _context.KhachHangs.FindAsync(id);
        }

        public async Task<bool> UpdateCustomerAsync(KhachHang customer)
        {
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CustomerExistsAsync(customer.CustomerId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<KhachHang?> GetCustomerDetailsAsync(int id)
        {
            return await _context.KhachHangs
                .Include(c => c.HopDongs)
                .Include(c => c.DuAns)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.KhachHangs.FindAsync(id);
            if (customer != null)
            {
                customer.IsDeleted = true; // Soft delete
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await _context.KhachHangs.AnyAsync(e => e.CustomerId == id);
        }
    }
}
