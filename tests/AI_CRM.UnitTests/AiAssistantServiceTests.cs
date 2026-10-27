using System.Threading.Tasks;
using AI_CRM.AI.Interfaces;
using AI_CRM.AI.Models;
using AI_CRM.AI.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AI_CRM.UnitTests
{
    public class AiAssistantServiceTests
    {
        private readonly Mock<IGeminiClient> _geminiClientMock;
        private readonly AiAssistantService _aiAssistantService;

        public AiAssistantServiceTests()
        {
            _geminiClientMock = new Mock<IGeminiClient>();
            _aiAssistantService = new AiAssistantService(_geminiClientMock.Object);
        }

        [Fact]
        public async Task SummarizeCustomerAsync_ShouldCallGeminiClient_AndReturnSuccess()
        {
            // Arrange
            var context = new CustomerAiContext
            {
                CompanyName = "Bệnh viện Đa khoa Tỉnh Thái Bình",
                HospitalLevel = "Tỉnh",
                HospitalClass = "Hạng I",
                BedCount = 800,
                DailyOutpatients = 1500,
                CurrentSoftware = "Phần mềm cũ HIS 1.0",
                ActiveContractsCount = 1,
                TotalContractValue = 500000000
            };

            _geminiClientMock
                .Setup(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync("### Đánh giá bệnh viện\n- Quy mô lớn cần nâng cấp HIS 2.0\n- Khuyến nghị triển khai thêm PACS không phim.");

            // Act
            var result = await _aiAssistantService.SummarizeCustomerAsync(context);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Content.Should().Contain("PACS không phim");
            _geminiClientMock.Verify(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>(), null), Times.Once);
        }

        [Fact]
        public async Task AnalyzeTicketSolutionAsync_ShouldGenerateDiagnosticSolution()
        {
            // Arrange
            var context = new TicketAiContext
            {
                TicketId = 101,
                Title = "Mất kết nối máy chụp CT sang hệ thống PACS",
                Description = "Bác sĩ phòng chẩn đoán hình ảnh không tải được ảnh DICOM",
                CustomerName = "Bệnh viện Đa khoa Khu vực",
                Priority = "Critical"
            };

            _geminiClientMock
                .Setup(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync("### Chẩn đoán sự cố DICOM:\n1. Kiểm tra Port 104 DICOM Storage.\n2. Khởi động lại PACS Gateway Service.");

            // Act
            var result = await _aiAssistantService.AnalyzeTicketSolutionAsync(context);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Content.Should().Contain("DICOM Storage");
        }
    }
}
