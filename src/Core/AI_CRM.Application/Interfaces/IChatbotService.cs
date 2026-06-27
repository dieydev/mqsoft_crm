using AI_CRM.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_CRM.Application.Interfaces
{
    public interface IChatbotService
    {
        Task<List<LichSuHoiDap>> GetChatHistoryAsync(int userId);
        Task<LichSuHoiDap> AskQuestionAsync(int userId, string question);
        Task<bool> RateAnswerAsync(int chatHistoryId, bool isHelpful, string comment);
        Task<bool> DeleteChatHistoryAsync(int chatHistoryId);
        Task<bool> ClearAllChatHistoryAsync(int userId);
    }
}
