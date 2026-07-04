using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ICustomerService _customerService;
        private readonly IProjectService _projectService;
        private readonly IContractService _contractService;

        public ReportController(
            IReportService reportService, 
            ICustomerService customerService,
            IProjectService projectService,
            IContractService contractService)
        {
            _reportService = reportService;
            _customerService = customerService;
            _projectService = projectService;
            _contractService = contractService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Customers = await _customerService.GetCustomersAsync(null, null);
            ViewBag.Projects = await _projectService.GetProjectsAsync(null, null);
            ViewBag.Contracts = await _contractService.GetContractsAsync(null, null);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportCustomersExcel()
        {
            var content = await _reportService.ExportCustomersToExcelAsync();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachKhachHang.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportProjectsExcel()
        {
            var content = await _reportService.ExportProjectsToExcelAsync();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDuAn.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportContractsExcel()
        {
            var content = await _reportService.ExportContractsToExcelAsync();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachHopDong.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportCustomerPdf(int id)
        {
            try
            {
                var content = await _reportService.ExportCustomerReportToPdfAsync(id);
                return File(content, "application/pdf", $"KhachHang_{id}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportProjectPdf(int id)
        {
            try
            {
                var content = await _reportService.ExportProjectReportToPdfAsync(id);
                return File(content, "application/pdf", $"DuAn_{id}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportContractPdf(int id)
        {
            try
            {
                var content = await _reportService.ExportContractReportToPdfAsync(id);
                return File(content, "application/pdf", $"HopDong_{id}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportProgressPdf(int id)
        {
            try
            {
                var content = await _reportService.ExportProgressReportToPdfAsync(id);
                return File(content, "application/pdf", $"TienDo_{id}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
