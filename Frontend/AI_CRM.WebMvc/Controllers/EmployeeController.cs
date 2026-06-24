using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            ViewBag.SearchString = searchString;
            var employees = await _employeeService.GetEmployeesAsync(searchString);
            return View(employees);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.UserId = new SelectList(await _employeeService.GetAvailableUsersAsync(), "UserId", "Username");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVienPhuTrach employee)
        {
            ModelState.Remove("NguoiDung");
            ModelState.Remove("DuAnNhanViens");
            ModelState.Remove("TienDoDuAns");
            ModelState.Remove("LichSuLamViecs");
            ModelState.Remove("TaiLieuNoiBos");

            if (ModelState.IsValid)
            {
                await _employeeService.CreateEmployeeAsync(employee);
                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.UserId = new SelectList(await _employeeService.GetAvailableUsersAsync(), "UserId", "Username", employee.UserId);
            return View(employee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            ViewBag.UserId = new SelectList(await _employeeService.GetAvailableUsersAsync(employee.UserId), "UserId", "Username", employee.UserId);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhanVienPhuTrach employee)
        {
            if (id != employee.EmployeeId) return NotFound();

            ModelState.Remove("NguoiDung");
            ModelState.Remove("DuAnNhanViens");
            ModelState.Remove("TienDoDuAns");
            ModelState.Remove("LichSuLamViecs");
            ModelState.Remove("TaiLieuNoiBos");

            if (ModelState.IsValid)
            {
                var success = await _employeeService.UpdateEmployeeAsync(employee);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserId = new SelectList(await _employeeService.GetAvailableUsersAsync(employee.UserId), "UserId", "Username", employee.UserId);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _employeeService.DeleteEmployeeAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa nhân viên.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
