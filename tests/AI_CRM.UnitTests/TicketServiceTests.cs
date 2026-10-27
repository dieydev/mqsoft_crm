using System;
using System.Threading.Tasks;
using AI_CRM.AI.Interfaces;
using AI_CRM.Application.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AI_CRM.UnitTests
{
    public class TicketServiceTests
    {
        private readonly Mock<ITicketRepository> _ticketRepoMock;
        private readonly Mock<IAiAssistantService> _aiServiceMock;
        private readonly Mock<IDocumentService> _docServiceMock;
        private readonly TicketService _ticketService;

        public TicketServiceTests()
        {
            _ticketRepoMock = new Mock<ITicketRepository>();
            _aiServiceMock = new Mock<IAiAssistantService>();
            _docServiceMock = new Mock<IDocumentService>();

            _ticketService = new TicketService(_ticketRepoMock.Object, _aiServiceMock.Object, _docServiceMock.Object);
        }

        [Fact]
        public void CalculateSlaDeadline_CriticalPriority_ShouldAdd4Hours()
        {
            // Arrange
            var createdTime = new DateTime(2026, 1, 1, 8, 0, 0);

            // Act
            var deadline = _ticketService.CalculateSlaDeadline(TicketPriority.Critical, createdTime);

            // Assert
            deadline.Should().Be(createdTime.AddHours(4));
        }

        [Fact]
        public void CalculateSlaDeadline_MediumPriority_ShouldAdd24Hours()
        {
            // Arrange
            var createdTime = new DateTime(2026, 1, 1, 8, 0, 0);

            // Act
            var deadline = _ticketService.CalculateSlaDeadline(TicketPriority.Medium, createdTime);

            // Assert
            deadline.Should().Be(createdTime.AddHours(24));
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldSetDefaultStatusNew_AndCalculateSlaDeadline()
        {
            // Arrange
            var ticket = new Ticket
            {
                Title = "Lỗi kết nối cổng thanh toán BHYT",
                Description = "Không gửi được XML giám định 4210",
                CustomerId = 1,
                Priority = TicketPriority.Critical
            };

            _ticketRepoMock.Setup(r => r.AddAsync(It.IsAny<Ticket>()))
                .ReturnsAsync((Ticket t) => t);

            // Act
            var result = await _ticketService.CreateTicketAsync(ticket, 1);

            // Assert
            result.Status.Should().Be(TicketStatus.New);
            result.CreatedByUserId.Should().Be(1);
            result.SlaDeadline.Should().NotBeNull();
            result.SlaDeadline.Value.Should().BeCloseTo(DateTime.Now.AddHours(4), TimeSpan.FromMinutes(1));
            _ticketRepoMock.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
        }

        [Fact]
        public void GetSlaStatus_WhenOverdue_ShouldReturnBreached()
        {
            // Arrange
            var ticket = new Ticket
            {
                Status = TicketStatus.InProgress,
                SlaDeadline = DateTime.Now.AddHours(-3) // Quá hạn 3 tiếng
            };

            // Act
            var status = _ticketService.GetSlaStatus(ticket);

            // Assert
            status.Should().Contain("Vi phạm SLA");
        }

        [Fact]
        public void GetSlaStatus_WhenResolvedWithinDeadline_ShouldReturnAchieved()
        {
            // Arrange
            var deadline = DateTime.Now.AddHours(5);
            var ticket = new Ticket
            {
                Status = TicketStatus.Resolved,
                SlaDeadline = deadline,
                ResolvedAt = DateTime.Now
            };

            // Act
            var status = _ticketService.GetSlaStatus(ticket);

            // Assert
            status.Should().Be("Đạt chuẩn SLA");
        }
    }
}
