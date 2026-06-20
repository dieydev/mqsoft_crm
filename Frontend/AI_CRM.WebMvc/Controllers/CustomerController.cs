using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? statusFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;

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

            var customers = await query.OrderByDescending(k => k.CreatedDate).ToListAsync();
            return View(customers);
        }

        public async Task<IActionResult> ExportExcel(string searchString, int? statusFilter)
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

            var customers = await query.OrderByDescending(k => k.CreatedDate).ToListAsync();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Mã KH,Tên doanh nghiệp,Người đại diện,Điện thoại,Email,Địa chỉ,Trạng thái");

            foreach (var cus in customers)
            {
                var status = cus.IsDeleted ? "Ngừng hoạt động" : "Hoạt động";
                builder.AppendLine($"\"{cus.CustomerCode}\",\"{cus.CompanyName}\",\"{cus.Representative}\",\"{cus.Phone}\",\"{cus.Email}\",\"{cus.Address}\",\"{status}\"");
            }

            return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", "DanhSachKhachHang.csv");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AI_CRM.Domain.Entities.KhachHang model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = System.DateTime.Now;
                model.IsDeleted = false;
                _context.KhachHangs.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.KhachHangs.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AI_CRM.Domain.Entities.KhachHang model)
        {
            if (id != model.CustomerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(model.CustomerId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.KhachHangs
                .Include(c => c.HopDongs)
                .Include(c => c.DuAns)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.KhachHangs.FindAsync(id);
            if (customer != null)
            {
                customer.IsDeleted = true; // Soft delete
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.KhachHangs.Any(e => e.CustomerId == id);
        }
    }
}
