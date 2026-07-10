using System;
using System.Linq;
using AI_CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AI_CRM.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static void SeedData(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Đảm bảo DB đã được tạo
            context.Database.EnsureCreated();

            // Nếu đã có khách hàng thì không seed nữa
            if (context.KhachHangs.Any())
            {
                return;
            }

            // 1. Seed Khách hàng
            var customers = new[]
            {
                new KhachHang { CustomerCode = "KH001", CompanyName = "Bệnh viện Chợ Rẫy", Representative = "Nguyễn Văn A", Phone = "0901234567", Email = "contact@choray.vn", Address = "201B Nguyễn Chí Thanh, Q5", OwnershipType = "Công lập", CreatedDate = DateTime.Now.AddMonths(-5) },
                new KhachHang { CustomerCode = "KH002", CompanyName = "BV Đại học Y Dược", Representative = "Trần Thị B", Phone = "0912345678", Email = "info@ump.edu.vn", Address = "215 Hồng Bàng, Q5", OwnershipType = "Công lập", CreatedDate = DateTime.Now.AddMonths(-4) },
                new KhachHang { CustomerCode = "KH003", CompanyName = "Phòng khám Hoàn Mỹ", Representative = "Lê Hoàng C", Phone = "0923456789", Email = "contact@hoanmy.com", Address = "Phú Nhuận, HCM", OwnershipType = "Tư nhân", CreatedDate = DateTime.Now.AddMonths(-3) },
                new KhachHang { CustomerCode = "KH004", CompanyName = "BV Tâm Anh", Representative = "Phạm D", Phone = "0934567890", Email = "info@tamanhhospital.vn", Address = "Tân Bình, HCM", OwnershipType = "Tư nhân", CreatedDate = DateTime.Now.AddMonths(-2) },
                new KhachHang { CustomerCode = "KH005", CompanyName = "Sở Y Tế HCM", Representative = "Võ E", Phone = "0945678901", Email = "syt@tphcm.gov.vn", Address = "Q3, HCM", OwnershipType = "Công lập", CreatedDate = DateTime.Now.AddMonths(-1) }
            };
            context.KhachHangs.AddRange(customers);
            context.SaveChanges();

            // Seed Trạng thái dự án
            var projectStatuses = new[]
            {
                new TrangThaiDuAn { StatusName = "Mới tạo", Description = "Dự án vừa khởi tạo" },
                new TrangThaiDuAn { StatusName = "Đang thực hiện", Description = "Đang trong quá trình code" },
                new TrangThaiDuAn { StatusName = "Hoàn thành", Description = "Đã bàn giao" }
            };
            context.TrangThaiDuAns.AddRange(projectStatuses);
            context.SaveChanges();

            // Seed Trạng thái hợp đồng
            var contractStatuses = new[]
            {
                new TrangThaiHopDong { StatusName = "Mới ký", Description = "Hợp đồng mới" },
                new TrangThaiHopDong { StatusName = "Đang hiệu lực", Description = "Đang thực hiện" },
                new TrangThaiHopDong { StatusName = "Đã thanh lý", Description = "Đã kết thúc" }
            };
            context.TrangThaiHopDongs.AddRange(contractStatuses);
            context.SaveChanges();

            // 2. Seed Dự án
            var projects = new[]
            {
                new DuAn { ProjectName = "Triển khai HIS 2.0", Description = "Nâng cấp hệ thống quản lý bệnh viện", StartDate = DateTime.Now.AddMonths(-4), StatusId = projectStatuses[1].StatusId, CustomerId = customers[0].CustomerId },
                new DuAn { ProjectName = "Hệ thống PACS", Description = "Quản lý hình ảnh y tế", StartDate = DateTime.Now.AddMonths(-3), StatusId = projectStatuses[2].StatusId, CustomerId = customers[1].CustomerId },
                new DuAn { ProjectName = "Phần mềm Quản lý Phòng khám", Description = "HIS cho tư nhân", StartDate = DateTime.Now.AddMonths(-1), StatusId = projectStatuses[0].StatusId, CustomerId = customers[2].CustomerId },
                new DuAn { ProjectName = "Bảo trì LIS", Description = "Bảo trì hệ thống xét nghiệm", StartDate = DateTime.Now.AddMonths(-2), StatusId = projectStatuses[1].StatusId, CustomerId = customers[3].CustomerId }
            };
            context.DuAns.AddRange(projects);
            context.SaveChanges();

            // 3. Seed Hợp đồng
            var contracts = new[]
            {
                new HopDong { ContractNumber = "HD-001", ContractName = "HĐ HIS Chợ Rẫy", ContractValue = 1500000000, SignDate = DateTime.Now.AddMonths(-4), StatusId = contractStatuses[1].StatusId, CustomerId = customers[0].CustomerId },
                new HopDong { ContractNumber = "HD-002", ContractName = "HĐ PACS Y Dược", ContractValue = 2000000000, SignDate = DateTime.Now.AddMonths(-3), StatusId = contractStatuses[2].StatusId, CustomerId = customers[1].CustomerId },
                new HopDong { ContractNumber = "HD-003", ContractName = "HĐ Phần mềm Hoàn Mỹ", ContractValue = 500000000, SignDate = DateTime.Now.AddMonths(-1), StatusId = contractStatuses[0].StatusId, CustomerId = customers[2].CustomerId },
                new HopDong { ContractNumber = "HD-004", ContractName = "HĐ Bảo trì Tâm Anh", ContractValue = 300000000, SignDate = DateTime.Now.AddMonths(-2), StatusId = contractStatuses[1].StatusId, CustomerId = customers[3].CustomerId }
            };
            context.HopDongs.AddRange(contracts);
            context.SaveChanges();
            
            // 4. Seed Employee
            var employees = new[]
            {
                new NhanVienPhuTrach { FullName = "Trần Văn Dev", Email = "dev1@mq.vn", Position = "Developer", Department = "Kỹ thuật", Phone = "0111222333" },
                new NhanVienPhuTrach { FullName = "Lê Thị Sale", Email = "sale1@mq.vn", Position = "Sales", Department = "Kinh doanh", Phone = "0222333444" },
                new NhanVienPhuTrach { FullName = "Nguyễn Văn BA", Email = "ba1@mq.vn", Position = "Business Analyst", Department = "Phân tích", Phone = "0333444555" }
            };
            context.NhanVienPhuTrachs.AddRange(employees);
            context.SaveChanges();
        }
    }
}
