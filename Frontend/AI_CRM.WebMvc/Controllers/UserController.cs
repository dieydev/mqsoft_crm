using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Index", "Admin");

            var nhanVien = await _context.NhanVienPhuTrachs
                .Include(nv => nv.NguoiDung)
                .ThenInclude(nd => nd.VaiTro)
                .FirstOrDefaultAsync(nv => nv.Email == email);

            return View(nhanVien);
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
}

