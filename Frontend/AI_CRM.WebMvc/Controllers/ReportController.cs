using Microsoft.AspNetCore.Mvc;

namespace AI_CRM.WebMvc.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

