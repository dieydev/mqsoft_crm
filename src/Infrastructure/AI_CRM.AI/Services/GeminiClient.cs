using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AI_CRM.AI.Interfaces;
using AI_CRM.AI.Models;
using Microsoft.Extensions.Configuration;

namespace AI_CRM.AI.Services
{
    public class GeminiClient : IGeminiClient
    {
        private readonly string _apiKey;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string[] FallbackModels = new[]
        {
            "gemini-2.5-flash",
            "gemini-2.0-flash",
            "gemini-flash-latest",
            "gemini-flash-lite-latest"
        };

        public GeminiClient(IConfiguration configuration)
        {
            _apiKey = configuration["GeminiApiKey"];
        }

        public async Task<string> GenerateTextAsync(string systemInstruction, string userPrompt, List<ChatMessageItem> history = null)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                return "⚠️ Hệ thống chưa được cấu hình Google Gemini API Key trong appsettings.json. Vui lòng cấu hình API Key để kích hoạt tính năng AI.";
            }

            var contentsList = new List<object>();

            // Lịch sử trao đổi nếu có
            if (history != null && history.Count > 0)
            {
                foreach (var item in history)
                {
                    contentsList.Add(new
                    {
                        role = item.Role == "user" ? "user" : "model",
                        parts = new[] { new { text = item.Content } }
                    });
                }
            }

            // Tin nhắn hiện tại kèm hướng dẫn hệ thống
            string finalPrompt = string.IsNullOrWhiteSpace(systemInstruction)
                ? userPrompt
                : $"{systemInstruction}\n\n---\nYÊU CẦU NGƯỜI DÙNG:\n{userPrompt}";

            contentsList.Add(new
            {
                role = "user",
                parts = new[] { new { text = finalPrompt } }
            });

            var requestBody = new { contents = contentsList };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            string lastError = string.Empty;

            foreach (var model in FallbackModels)
            {
                try
                {
                    string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(jsonString);

                        if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var candidate = candidates[0];
                            if (candidate.TryGetProperty("content", out var contentProp) &&
                                contentProp.TryGetProperty("parts", out var parts) &&
                                parts.GetArrayLength() > 0)
                            {
                                var text = parts[0].GetProperty("text").GetString();
                                return text?.Trim() ?? string.Empty;
                            }
                        }
                    }
                    else
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        lastError = $"[{model}] Status: {response.StatusCode}, Details: {errorBody}";
                    }
                }
                catch (Exception ex)
                {
                    lastError = $"[{model}] Exception: {ex.Message}";
                }
            }

            return $"Xin lỗi, dịch vụ AI tạm thời không phản hồi. Chi tiết kỹ thuật: {lastError}";
        }
    }
}
