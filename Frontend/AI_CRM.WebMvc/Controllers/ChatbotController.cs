using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatbotController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var history = await _context.LichSuHoiDaps
                .Include(c => c.NguoiDung)
                .ToListAsync();
            return View(history);
        }
    }
}
