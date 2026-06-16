using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var docs = await _context.TaiLieuNoiBos
                .Include(d => d.NhomTaiLieu)
                .Include(d => d.NhanVienPhuTrach)
                .ToListAsync();
            return View(docs);
        }
    }
}
