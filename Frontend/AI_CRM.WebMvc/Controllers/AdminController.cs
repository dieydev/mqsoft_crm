using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AI_CRM.WebMvc.Models;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly ApplicationDbContext _context;

    public AdminController(ILogger<AdminController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalCustomers = await _context.KhachHangs.CountAsync(k => !k.IsDeleted);
        ViewBag.ActiveProjects = await _context.DuAns.CountAsync(d => !d.IsDeleted);
        ViewBag.TotalContracts = await _context.HopDongs.CountAsync(h => !h.IsDeleted);
        ViewBag.ChatbotQueries = await _context.LichSuHoiDaps.CountAsync();

        return View();
    }
}
