using Microsoft.AspNetCore.Mvc;
using AI_CRM.Infrastructure.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AI_CRM.WebMvc.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy 15 log mới nhất từ NhatKyHeThong
            var logs = await _context.NhatKyHeThongs
                .Include(n => n.NguoiDung)
                .OrderByDescending(n => n.Timestamp)
                .Take(15)
                .ToListAsync();

            // Tính số lượng "mới" (trong 24h qua)
            var newCount = logs.Count(l => (System.DateTime.Now - l.Timestamp).TotalHours <= 24);
            
            ViewBag.NewCount = newCount;
            return View(logs);
        }
    }
}
