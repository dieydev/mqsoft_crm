using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    public class InteractionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InteractionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.LichSuLamViecs
                .Include(l => l.KhachHang)
                .Include(l => l.NhanVienPhuTrach)
                .ToListAsync();
            return View(logs);
        }
    }
}
