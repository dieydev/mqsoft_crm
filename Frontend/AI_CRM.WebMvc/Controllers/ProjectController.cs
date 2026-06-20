using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _context.DuAns
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDuAn)
                .ToListAsync();
            return View(projects);
        }

        public IActionResult Create()
        {
            return View();
        }
    }
}

