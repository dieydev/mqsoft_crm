using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize]
    public class InteractionController : Controller
    {
        private readonly IInteractionService _interactionService;

        public InteractionController(IInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _interactionService.GetInteractionsAsync();
            return View(logs);
        }
    }
}
