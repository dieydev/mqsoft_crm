using Microsoft.AspNetCore.Mvc;
using AI_CRM.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;

namespace AI_CRM.WebMvc.Controllers
{
    [Authorize(Roles = "Admin,Manager,NhanVien")]
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) 
            {
                userId = 1; // Fallback cho tài khoản đăng nhập bằng Google hoặc Admin cứng
            }

            var history = await _chatbotService.GetChatHistoryAsync(userId);
            return View(history);
        }

        public async Task<IActionResult> Chat()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) 
            {
                userId = 1; // Fallback cho tài khoản đăng nhập bằng Google hoặc Admin cứng
            }

            var history = await _chatbotService.GetChatHistoryAsync(userId);
            return View(history);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var success = await _chatbotService.DeleteChatHistoryAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa lịch sử chat thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại. Không tìm thấy cuộc hội thoại này.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) userId = 1;

            await _chatbotService.ClearAllChatHistoryAsync(userId);
            TempData["SuccessMessage"] = "Đã xóa toàn bộ lịch sử trò chuyện.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return BadRequest("Câu hỏi không được để trống.");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) 
            {
                userId = 1; // Fallback cho tài khoản đăng nhập bằng Google hoặc Admin cứng
            }

            try
            {
                var result = await _chatbotService.AskQuestionAsync(userId, request.Question);
                return Json(new { success = true, answer = result.Answer, historyId = result.ChatHistoryId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Rate([FromBody] RateRequest request)
        {
            try
            {
                var success = await _chatbotService.RateAnswerAsync(request.HistoryId, request.IsHelpful, request.Comment);
                return Json(new { success });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }

    public class ChatRequest
    {
        public string Question { get; set; }
    }

    public class RateRequest
    {
        public int HistoryId { get; set; }
        public bool IsHelpful { get; set; }
        public string Comment { get; set; }
    }
}
