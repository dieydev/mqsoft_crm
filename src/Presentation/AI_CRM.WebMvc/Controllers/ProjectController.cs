using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Manager,NhanVien")]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task<IActionResult> Index(string searchString, int? statusId)
        {
            var projects = await _projectService.GetProjectsAsync(searchString, statusId);

            var statuses = await _projectService.GetStatusesAsync();
            ViewBag.StatusList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(statuses, "StatusId", "StatusName");
            ViewBag.SearchString = searchString;
            ViewBag.CurrentStatus = statusId;

            return View(projects);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveCustomersAsync(), "CustomerId", "CompanyName");
            ViewBag.ContractId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveContractsAsync(), "ContractId", "ContractNumber");
            ViewBag.StatusId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetStatusesAsync(), "StatusId", "StatusName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AI_CRM.Domain.Entities.DuAn project)
        {
            ModelState.Remove("KhachHang");
            ModelState.Remove("HopDong");
            ModelState.Remove("TrangThaiDuAn");
            ModelState.Remove("DuAnNhanViens");
            ModelState.Remove("TienDoDuAns");

            if (ModelState.IsValid)
            {
                await _projectService.CreateProjectAsync(project);
                TempData["SuccessMessage"] = "Thêm dự án thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", project.CustomerId);
            ViewBag.ContractId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveContractsAsync(), "ContractId", "ContractNumber", project.ContractId);
            ViewBag.StatusId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetStatusesAsync(), "StatusId", "StatusName", project.StatusId);
            return View(project);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _projectService.GetProjectByIdAsync(id.Value);
            if (project == null || project.IsDeleted) return NotFound();

            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", project.CustomerId);
            ViewBag.ContractId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveContractsAsync(), "ContractId", "ContractNumber", project.ContractId);
            ViewBag.StatusId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetStatusesAsync(), "StatusId", "StatusName", project.StatusId);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AI_CRM.Domain.Entities.DuAn project)
        {
            if (id != project.ProjectId) return NotFound();

            ModelState.Remove("KhachHang");
            ModelState.Remove("HopDong");
            ModelState.Remove("TrangThaiDuAn");
            ModelState.Remove("DuAnNhanViens");
            ModelState.Remove("TienDoDuAns");

            if (ModelState.IsValid)
            {
                var success = await _projectService.UpdateProjectAsync(project);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Cập nhật dự án thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.CustomerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", project.CustomerId);
            ViewBag.ContractId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetActiveContractsAsync(), "ContractId", "ContractNumber", project.ContractId);
            ViewBag.StatusId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetStatusesAsync(), "StatusId", "StatusName", project.StatusId);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _projectService.DeleteProjectAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa dự án thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportExcel(string searchString, int? statusId)
        {
            var fileContent = await _projectService.ExportExcelAsync(searchString, statusId);
            return File(fileContent, "text/csv", "DanhSachDuAn.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var project = await _projectService.GetProjectDetailsAsync(id.Value);
            if (project == null || project.IsDeleted) return NotFound();

            ViewBag.AvailableEmployees = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _projectService.GetAvailableEmployeesForProjectAsync(project.ProjectId), "EmployeeId", "FullName");

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMember(int projectId, int employeeId, string projectRole)
        {
            var success = await _projectService.AssignMemberAsync(projectId, employeeId, projectRole);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã thêm nhân sự vào dự án.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra hoặc nhân sự đã nằm trong dự án.";
            }
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, int assignmentId)
        {
            var success = await _projectService.RemoveMemberAsync(assignmentId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa nhân sự khỏi dự án.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa nhân sự.";
            }
            return RedirectToAction(nameof(Details), new { id = projectId });
        }
    }
}
