using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize]
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public async Task<IActionResult> Index()
        {
            var history = await _chatbotService.GetChatHistoryAsync();
            return View(history);
        }

        public IActionResult Chat()
        {
            return View();
        }
    }
}
