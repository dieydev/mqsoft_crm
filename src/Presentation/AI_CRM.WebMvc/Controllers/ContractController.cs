using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using AI_CRM.Domain.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Manager,NhanVien")]
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IWebHostEnvironment _env;

        public ContractController(IContractService contractService, IWebHostEnvironment env)
        {
            _contractService = contractService;
            _env = env;
        }

        public async Task<IActionResult> Index(string searchString, int? statusId)
        {
            var contracts = await _contractService.GetContractsAsync(searchString, statusId);

            var statuses = await _contractService.GetStatusesAsync();
            ViewBag.StatusList = new SelectList(statuses, "StatusId", "StatusName");
            ViewBag.SearchString = searchString;
            ViewBag.CurrentStatus = statusId;

            return View(contracts);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName");
            ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HopDong hopDong, IFormFile pdfFile)
        {
            ModelState.Remove("KhachHang");
            ModelState.Remove("TrangThaiHopDong");
            ModelState.Remove("DuAns");

            if (ModelState.IsValid)
            {
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    var ext = System.IO.Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
                    if (ext != ".pdf")
                    {
                        ModelState.AddModelError("FilePath", "Chỉ cho phép upload file định dạng PDF.");
                        ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
                        ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName", hopDong.StatusId);
                        return View(hopDong);
                    }
                    if (pdfFile.Length > 50 * 1024 * 1024)
                    {
                        ModelState.AddModelError("FilePath", "Dung lượng file không được vượt quá 50MB.");
                        ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
                        ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName", hopDong.StatusId);
                        return View(hopDong);
                    }

                    using (var stream = pdfFile.OpenReadStream())
                    {
                        hopDong.FilePath = await _contractService.SaveFileAsync(stream, pdfFile.FileName, _env.WebRootPath);
                    }
                }

                await _contractService.CreateContractAsync(hopDong);
                TempData["SuccessMessage"] = "Thêm hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
            ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName", hopDong.StatusId);
            return View(hopDong);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hopDong = await _contractService.GetContractByIdAsync(id.Value);
            if (hopDong == null || hopDong.IsDeleted) return NotFound();

            ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
            ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName", hopDong.StatusId);
            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HopDong hopDong, IFormFile pdfFile)
        {
            if (id != hopDong.ContractId) return NotFound();

            ModelState.Remove("KhachHang");
            ModelState.Remove("TrangThaiHopDong");
            ModelState.Remove("DuAns");

            if (ModelState.IsValid)
            {
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    var ext = System.IO.Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
                    if (ext != ".pdf")
                    {
                        ModelState.AddModelError("FilePath", "Chỉ cho phép upload file định dạng PDF.");
                        ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
                        ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName", hopDong.StatusId);
                        return View(hopDong);
                    }
                    if (pdfFile.Length > 50 * 1024 * 1024)
                    {
                        ModelState.AddModelError("FilePath", "Dung lượng file không được vượt quá 50MB.");
                        ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
                        ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName", hopDong.StatusId);
                        return View(hopDong);
                    }

                    using (var stream = pdfFile.OpenReadStream())
                    {
                        hopDong.FilePath = await _contractService.SaveFileAsync(stream, pdfFile.FileName, _env.WebRootPath);
                    }
                }

                var success = await _contractService.UpdateContractAsync(hopDong);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Cập nhật hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CustomerId = new SelectList(await _contractService.GetActiveCustomersAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
            ViewBag.StatusId = new SelectList(await _contractService.GetStatusesAsync(), "StatusId", "StatusName", hopDong.StatusId);
            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _contractService.DeleteContractAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa hợp đồng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportExcel(string searchString, int? statusId)
        {
            var fileContent = await _contractService.ExportExcelAsync(searchString, statusId);
            return File(fileContent, "text/csv", "DanhSachHopDong.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var hopDong = await _contractService.GetContractDetailsAsync(id.Value);
            if (hopDong == null || hopDong.IsDeleted) return NotFound();

            return View(hopDong);
        }
    }
}
