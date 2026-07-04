using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AI_CRM.WebMvc.Models;
using AI_CRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers;

[Authorize(Roles = "Admin,Employee")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly IAdminService _adminService;

    public AdminController(ILogger<AdminController> logger, IAdminService adminService)
    {
        _logger = logger;
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var currentRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";
        if (currentRole == "User")
        {
            // Nếu vô tình lọt vào (do cookie cũ hoặc gõ tay url), thì đuổi ra và báo lỗi
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["ErrorMessage"] = "Tài khoản của bạn không có quyền truy cập trang Quản trị.";
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.TotalCustomers = await _adminService.GetTotalCustomersAsync();
        ViewBag.ActiveProjects = await _adminService.GetActiveProjectsAsync();
        ViewBag.TotalContracts = await _adminService.GetTotalContractsAsync();
        ViewBag.ChatbotQueries = await _adminService.GetTotalChatbotQueriesAsync();
        
        ViewBag.RevenueData = await _adminService.GetRevenueByMonthAsync();
        ViewBag.ProjectStatusData = await _adminService.GetProjectStatusDistributionAsync();
        ViewBag.TopContractsData = await _adminService.GetTopContractsAsync();
        ViewBag.EmployeeAllocationData = await _adminService.GetEmployeeAllocationAsync();

        return View();
    }
}
