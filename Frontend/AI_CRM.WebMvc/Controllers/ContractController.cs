using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using AI_CRM.Domain.Entities;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContractController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string searchString, int? statusId)
        {
            var query = _context.HopDongs
                .Include(h => h.KhachHang)
                .Include(h => h.TrangThaiHopDong)
                .Where(h => !h.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => h.ContractNumber.Contains(searchString) || h.ContractName.Contains(searchString) || h.KhachHang.CompanyName.Contains(searchString));
            }
            if (statusId.HasValue && statusId.Value > 0)
            {
                query = query.Where(h => h.StatusId == statusId.Value);
            }

            var statuses = await _context.TrangThaiHopDongs.ToListAsync();
            ViewBag.StatusList = new SelectList(statuses, "StatusId", "StatusName");
            ViewBag.SearchString = searchString;
            ViewBag.CurrentStatus = statusId;

            return View(await query.OrderByDescending(h => h.CreatedDate).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CustomerId = new SelectList(await _context.KhachHangs.Where(x => !x.IsDeleted).ToListAsync(), "CustomerId", "CompanyName");
            ViewBag.StatusId = new SelectList(await _context.TrangThaiHopDongs.ToListAsync(), "StatusId", "StatusName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HopDong hopDong, IFormFile pdfFile)
        {
            if (ModelState.IsValid)
            {
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(pdfFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }
                    hopDong.FilePath = "/uploads/contracts/" + uniqueFileName;
                }

                hopDong.CreatedDate = DateTime.Now;
                _context.Add(hopDong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CustomerId = new SelectList(await _context.KhachHangs.Where(x => !x.IsDeleted).ToListAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
            ViewBag.StatusId = new SelectList(await _context.TrangThaiHopDongs.ToListAsync(), "StatusId", "StatusName", hopDong.StatusId);
            return View(hopDong);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong == null || hopDong.IsDeleted) return NotFound();

            ViewBag.CustomerId = new SelectList(await _context.KhachHangs.Where(x => !x.IsDeleted).ToListAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
            ViewBag.StatusId = new SelectList(await _context.TrangThaiHopDongs.ToListAsync(), "StatusId", "StatusName", hopDong.StatusId);
            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HopDong hopDong, IFormFile pdfFile)
        {
            if (id != hopDong.ContractId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingContract = await _context.HopDongs.AsNoTracking().FirstOrDefaultAsync(h => h.ContractId == id);
                    if (existingContract == null) return NotFound();

                    hopDong.CreatedDate = existingContract.CreatedDate;
                    hopDong.FilePath = existingContract.FilePath;

                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(pdfFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await pdfFile.CopyToAsync(fileStream);
                        }
                        hopDong.FilePath = "/uploads/contracts/" + uniqueFileName;
                    }

                    _context.Update(hopDong);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật hợp đồng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HopDongExists(hopDong.ContractId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CustomerId = new SelectList(await _context.KhachHangs.Where(x => !x.IsDeleted).ToListAsync(), "CustomerId", "CompanyName", hopDong.CustomerId);
            ViewBag.StatusId = new SelectList(await _context.TrangThaiHopDongs.ToListAsync(), "StatusId", "StatusName", hopDong.StatusId);
            return View(hopDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong != null)
            {
                hopDong.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa hợp đồng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HopDongExists(int id)
        {
            return _context.HopDongs.Any(e => e.ContractId == id);
        }
    }
}
