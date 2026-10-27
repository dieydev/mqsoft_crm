using System;
using System.Threading.Tasks;
using AI_CRM.AI.Interfaces;
using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using AI_CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AI_CRM.UnitTests
{
    public class CustomerServiceTests
    {
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldSetCreatedDateAndNotDeleted()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var aiMock = new Mock<IAiAssistantService>();
            var service = new CustomerService(context, aiMock.Object);

            var customer = new KhachHang
            {
                CustomerCode = "BV-BACHMAI",
                CompanyName = "Bệnh viện Bạch Mai",
                Representative = "PGS.TS Nguyễn Văn A",
                HospitalLevel = "Trung ương",
                HospitalClass = "Đặc biệt"
            };

            // Act
            var created = await service.CreateCustomerAsync(customer);

            // Assert
            created.CustomerId.Should().BeGreaterThan(0);
            created.IsDeleted.Should().BeFalse();
            created.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));

            var inDb = await context.KhachHangs.FindAsync(created.CustomerId);
            inDb.Should().NotBeNull();
            inDb.CompanyName.Should().Be("Bệnh viện Bạch Mai");
        }

        [Fact]
        public async Task GetCustomersAsync_WithSearchString_ShouldFilterProperly()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var aiMock = new Mock<IAiAssistantService>();
            var service = new CustomerService(context, aiMock.Object);

            context.KhachHangs.AddRange(
                new KhachHang { CustomerCode = "BV01", CompanyName = "Bệnh viện Chợ Rẫy", Phone = "0901234567" },
                new KhachHang { CustomerCode = "BV02", CompanyName = "Bệnh viện Việt Đức", Phone = "0987654321" },
                new KhachHang { CustomerCode = "PK01", CompanyName = "Phòng khám Đa khoa Medlatec", Phone = "0911223344" }
            );
            await context.SaveChangesAsync();

            // Act
            var results = await service.GetCustomersAsync("Việt Đức", null);

            // Assert
            results.Should().HaveCount(1);
            results[0].CompanyName.Should().Be("Bệnh viện Việt Đức");
        }

        [Fact]
        public async Task DeleteCustomerAsync_ShouldSetIsDeletedTrue_SoftDelete()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var aiMock = new Mock<IAiAssistantService>();
            var service = new CustomerService(context, aiMock.Object);

            var customer = new KhachHang
            {
                CustomerCode = "BV-TEST",
                CompanyName = "Bệnh viện Test Xóa",
                IsDeleted = false
            };
            context.KhachHangs.Add(customer);
            await context.SaveChangesAsync();

            // Act
            var result = await service.DeleteCustomerAsync(customer.CustomerId);

            // Assert
            result.Should().BeTrue();
            var inDb = await context.KhachHangs.FindAsync(customer.CustomerId);
            inDb.Should().NotBeNull();
            inDb.IsDeleted.Should().BeTrue();
        }
    }
}
