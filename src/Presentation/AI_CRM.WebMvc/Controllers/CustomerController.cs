using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AI_CRM.WebMvc.Models;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Manager,NhanVien")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(string searchString, int? statusFilter, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;

            var customers = await _customerService.GetCustomersAsync(searchString, statusFilter);
            
            int pageSize = 10;
            var paginatedCustomers = PaginatedList<KhachHang>.Create(customers, pageNumber ?? 1, pageSize);

            return View(paginatedCustomers);
        }

        public async Task<IActionResult> ExportExcel(string searchString, int? statusFilter)
        {
            var fileContent = await _customerService.ExportExcelAsync(searchString, statusFilter);
            return File(fileContent, "text/csv", "DanhSachKhachHang.csv");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHang model)
        {
            if (ModelState.IsValid)
            {
                await _customerService.CreateCustomerAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _customerService.GetCustomerByIdAsync(id.Value);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhachHang model)
        {
            if (id != model.CustomerId) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _customerService.UpdateCustomerAsync(model);
                if (!success) return NotFound();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _customerService.GetCustomerDetailsAsync(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
