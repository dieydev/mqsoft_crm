using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contracts = await _context.HopDongs
                .Include(h => h.KhachHang)
                .Include(h => h.TrangThaiHopDong)
                .ToListAsync();
            return View(contracts);
        }
    }
}
