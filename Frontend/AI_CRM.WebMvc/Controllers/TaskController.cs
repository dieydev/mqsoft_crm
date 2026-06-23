using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public async Task<IActionResult> Index(int? projectId)
        {
            var tasks = await _taskService.GetTasksAsync(projectId);
            ViewBag.ProjectList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveProjectsAsync(), "ProjectId", "ProjectName");
            ViewBag.CurrentProject = projectId;
            return View(tasks);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveProjectsAsync(), "ProjectId", "ProjectName");
            ViewBag.UpdatedBy = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveEmployeesAsync(), "EmployeeId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AI_CRM.Domain.Entities.TienDoDuAn task)
        {
            ModelState.Remove("DuAn");
            ModelState.Remove("NhanVienPhuTrach");

            if (ModelState.IsValid)
            {
                await _taskService.CreateTaskAsync(task);
                TempData["SuccessMessage"] = "Thêm tiến độ thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveProjectsAsync(), "ProjectId", "ProjectName", task.ProjectId);
            ViewBag.UpdatedBy = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveEmployeesAsync(), "EmployeeId", "FullName", task.UpdatedBy);
            return View(task);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _taskService.GetTaskByIdAsync(id.Value);
            if (task == null) return NotFound();

            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveProjectsAsync(), "ProjectId", "ProjectName", task.ProjectId);
            ViewBag.UpdatedBy = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveEmployeesAsync(), "EmployeeId", "FullName", task.UpdatedBy);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AI_CRM.Domain.Entities.TienDoDuAn task)
        {
            if (id != task.ProgressId) return NotFound();

            ModelState.Remove("DuAn");
            ModelState.Remove("NhanVienPhuTrach");

            if (ModelState.IsValid)
            {
                var success = await _taskService.UpdateTaskAsync(task);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Cập nhật tiến độ thành công!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveProjectsAsync(), "ProjectId", "ProjectName", task.ProjectId);
            ViewBag.UpdatedBy = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _taskService.GetActiveEmployeesAsync(), "EmployeeId", "FullName", task.UpdatedBy);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _taskService.DeleteTaskAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa tiến độ thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
