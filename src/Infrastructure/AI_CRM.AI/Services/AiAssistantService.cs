using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AI_CRM.AI.Interfaces;
using AI_CRM.AI.Models;

namespace AI_CRM.AI.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly IGeminiClient _geminiClient;

        public AiAssistantService(IGeminiClient geminiClient)
        {
            _geminiClient = geminiClient;
        }

        public async Task<string> ChatAsync(string userQuestion, string systemContext, List<ChatMessageItem> history = null)
        {
            string systemInstruction = @"Bạn là Trợ lý AI thông minh chuyên gia của hệ thống MQSoft CRM - Giải pháp quản trị khách hàng y tế và phần mềm bệnh viện (HIS, LIS, PACS, EMR, Telemedicine).
Các quy tắc cốt lõi:
1. Trả lời bằng tiếng Việt lịch sự, súc tích, văn phong chuyên gia công nghệ y tế (HealthTech).
2. Dựa trên dữ liệu hệ thống được cung cấp để trả lời các câu hỏi cụ thể (khách hàng, dự án, hợp đồng, nhân sự).
3. Nếu dữ liệu không có trong hệ thống, hãy thông báo rõ ràng là thông tin chưa có trong CSDL nội bộ, không tự bịa thông tin nhạy cảm.
4. Trình bày dạng Markdown với danh sách bullet points rõ ràng.";

            string fullInstruction = $"{systemInstruction}\n\n--- DỮ LIỆU TỪ HỆ THỐNG ---\n{systemContext}";
            return await _geminiClient.GenerateTextAsync(fullInstruction, userQuestion, history);
        }

        public async Task<AiAnalysisResult> SummarizeCustomerAsync(CustomerAiContext context)
        {
            string systemInstruction = @"Bạn là Chuyên gia Tư vấn Cấp cao về Giải pháp Công nghệ Y tế (HealthTech Consultant) của MQSoft.
Nhiệm vụ của bạn là phân tích hồ sơ Cơ sở Khám chữa bệnh (Bệnh viện / Phòng khám) và đưa ra bản tóm tắt phân tích chiến lược 3 phần:
1. 🏥 **Tình hình & Hiện trạng CNTT**: Đánh giá quy mô (số giường, lượt khám/ngày), phần mềm hiện tại đang sử dụng.
2. ⚠️ **Rủi ro & Điểm cần lưu ý**: Các vấn đề về hợp đồng, sự cố ticket tồn đọng, mức độ tương tác.
3. 💡 **Cơ hội nâng cấp & Hành động tiếp theo**: Đề xuất giải pháp nâng cấp (chuyển đổi sang HIS 2.0, tích hợp PACS không phim, số hóa EMR theo Thông tư 46/2018/TT-BYT).

Định dạng trả về: Markdown chuẩn, chuyên nghiệp, ngắn gọn (tối đa 400 từ).";

            string userPrompt = $@"Phân tích hồ sơ khách hàng y tế sau:
- Tên đơn vị: {context.CompanyName} (Mã CSKCB: {context.FacilityCode ?? "Chưa rõ"})
- Tuyến bệnh viện: {context.HospitalLevel ?? "Chưa rõ"} - Hạng: {context.HospitalClass ?? "Chưa rõ"} - Hình thức: {context.OwnershipType ?? "Công lập"}
- Quy mô: {context.BedCount?.ToString() ?? "0"} giường bệnh, khoảng {context.DailyOutpatients?.ToString() ?? "0"} lượt khám/ngày
- Phần mềm cũ đang chạy: {context.CurrentSoftware ?? "Chưa ghi nhận"}
- Tình trạng hợp đồng: {context.ActiveContractsCount} hợp đồng hiệu lực (Tổng giá trị: {context.TotalContractValue:N0} VNĐ)
- Dự án đang triển khai: {context.ActiveProjectsCount} dự án
- Sự cố / Ticket đang mở: {context.OpenTicketsCount} ticket
- Nhật ký tương tác gần đây: {context.RecentInteractions ?? "Chưa có tương tác gần đây"}";

            try
            {
                var text = await _geminiClient.GenerateTextAsync(systemInstruction, userPrompt);
                return new AiAnalysisResult { IsSuccess = true, Content = text };
            }
            catch (Exception ex)
            {
                return new AiAnalysisResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<AiAnalysisResult> AnalyzeTicketSolutionAsync(TicketAiContext context)
        {
            string systemInstruction = @"Bạn là Kỹ sư Trưởng Hỗ trợ Kỹ thuật Y tế (HealthTech Lead Support Engineer) của MQSoft.
Nhiệm vụ: Chẩn đoán sự cố phần mềm bệnh viện (HIS, PACS, LIS, EMR) và gợi ý giải pháp xử lý theo các bước:
1. 🎯 **Chẩn đoán nguyên nhân tiềm năng**: Vì sao sự cố xảy ra? (Lỗi kết nối DICOM, đồng bộ dữ liệu BHYT Cổng 4210, lỗi cơ sở dữ liệu, hay thao tác người dùng).
2. 🛠️ **Các bước khắc phục khuyến nghị**: Hướng dẫn kỹ thuật viên xử lý nhanh (step-by-step).
3. ⏱️ **Mức độ khẩn cấp & Khuyến cáo**: Đánh giá xem có ảnh hưởng đến việc khám chữa bệnh trực tiếp hay không và khuyến nghị phòng ngừa.

Định dạng trả về: Markdown rõ ràng, dễ làm theo.";

            string userPrompt = $@"Phân tích sự cố kỹ thuật:
- Mã Ticket: #{context.TicketId}
- Cơ sở y tế: {context.CustomerName}
- Hệ thống bị ảnh hưởng: {context.CurrentSoftware ?? "Hệ thống HIS/PACS y tế"}
- Mức độ ưu tiên ghi nhận: {context.Priority}
- Tiêu đề sự cố: {context.Title}
- Mô tả chi tiết: {context.Description}
- Tài liệu kỹ thuật liên quan:
{context.RelevantKnowledgeBase ?? "Chưa tìm thấy tài liệu phù hợp"}";

            try
            {
                var text = await _geminiClient.GenerateTextAsync(systemInstruction, userPrompt);
                return new AiAnalysisResult { IsSuccess = true, Content = text };
            }
            catch (Exception ex)
            {
                return new AiAnalysisResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
    }
}
