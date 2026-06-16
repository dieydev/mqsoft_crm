using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await _context.TienDoDuAns
                .Include(t => t.DuAn)
                .Include(t => t.NhanVienPhuTrach)
                .ToListAsync();
            return View(tasks);
        }
    }
}
