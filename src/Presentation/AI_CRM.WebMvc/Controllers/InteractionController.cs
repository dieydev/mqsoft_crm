using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class InteractionController : Controller
    {
        private readonly IInteractionService _interactionService;

        public InteractionController(IInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        public async Task<IActionResult> Index(int? customerId)
        {
            var interactions = await _interactionService.GetInteractionsAsync(customerId);
            ViewBag.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveCustomersAsync(), "CustomerId", "CompanyName");
            ViewBag.CurrentCustomer = customerId;
            return View(interactions);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveCustomersAsync(), "CustomerId", "CompanyName");
            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveEmployeesAsync(), "EmployeeId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AI_CRM.Domain.Entities.LichSuLamViec interaction)
        {
            ModelState.Remove("KhachHang");
            ModelState.Remove("NhanVienPhuTrach");

            if (ModelState.IsValid)
            {
                await _interactionService.CreateInteractionAsync(interaction);
                TempData["SuccessMessage"] = "Thêm lịch sử làm việc thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", interaction.CustomerId);
            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveEmployeesAsync(), "EmployeeId", "FullName", interaction.EmployeeId);
            return View(interaction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var interaction = await _interactionService.GetInteractionByIdAsync(id.Value);
            if (interaction == null) return NotFound();

            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", interaction.CustomerId);
            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveEmployeesAsync(), "EmployeeId", "FullName", interaction.EmployeeId);
            return View(interaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AI_CRM.Domain.Entities.LichSuLamViec interaction)
        {
            if (id != interaction.WorkLogId) return NotFound();

            ModelState.Remove("KhachHang");
            ModelState.Remove("NhanVienPhuTrach");

            if (ModelState.IsValid)
            {
                var success = await _interactionService.UpdateInteractionAsync(interaction);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Cập nhật lịch sử thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", interaction.CustomerId);
            ViewBag.EmployeeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _interactionService.GetActiveEmployeesAsync(), "EmployeeId", "FullName", interaction.EmployeeId);
            return View(interaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _interactionService.DeleteInteractionAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa lịch sử thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
