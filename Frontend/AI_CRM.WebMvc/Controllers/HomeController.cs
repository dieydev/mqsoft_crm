using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AI_CRM.WebMvc.Models;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AI_CRM.WebMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
