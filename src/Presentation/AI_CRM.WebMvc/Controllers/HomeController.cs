using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AI_CRM.WebMvc.Models;
using Microsoft.Extensions.Logging;

namespace AI_CRM.WebMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult HIS()
    {
        return View();
    }

    public IActionResult RISPACS()
    {
        return View();
    }

    public IActionResult LIS()
    {
        return View();
    }

    public IActionResult EMR()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Route("Home/Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        if (statusCode.HasValue)
        {
            if (statusCode.Value == 404)
            {
                return View("Error404");
            }
            else if (statusCode.Value == 403)
            {
                return View("Error403");
            }
        }
        return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
