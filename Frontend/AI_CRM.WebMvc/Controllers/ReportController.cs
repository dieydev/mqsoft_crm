using Microsoft.AspNetCore.Mvc;

namespace AI_CRM.WebMvc.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
