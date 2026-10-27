using System.Collections.Generic;
using System.Threading.Tasks;
using AI_CRM.AI.Models;

namespace AI_CRM.AI.Interfaces
{
    public interface IGeminiClient
    {
        Task<string> GenerateTextAsync(string systemInstruction, string userPrompt, List<ChatMessageItem> history = null);
    }
}
