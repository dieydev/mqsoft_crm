using System.Collections.Generic;
using System.Threading.Tasks;
using AI_CRM.AI.Models;

namespace AI_CRM.AI.Interfaces
{
    public interface IAiAssistantService
    {
        Task<string> ChatAsync(string userQuestion, string systemContext, List<ChatMessageItem> history = null);
        Task<AiAnalysisResult> SummarizeCustomerAsync(CustomerAiContext context);
        Task<AiAnalysisResult> AnalyzeTicketSolutionAsync(TicketAiContext context);
    }
}
