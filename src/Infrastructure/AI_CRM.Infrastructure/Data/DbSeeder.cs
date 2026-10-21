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

            // Đảm bảo bảng Ticket và TicketComment tồn tại (kể cả khi DB đã được tạo từ trước)
            try
            {
                string createTicketTablesSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ticket')
BEGIN
    CREATE TABLE [Ticket] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CustomerId] int NOT NULL,
        [CreatedByUserId] int NULL,
        [AssignedToEmployeeId] int NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ResolvedAt] datetime2 NULL,
        [SlaDeadline] datetime2 NULL,
        CONSTRAINT [PK_Ticket] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ticket_KhachHang_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [KhachHang] ([CustomerId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ticket_NguoiDung_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [NguoiDung] ([UserId]) ON DELETE SET NULL,
        CONSTRAINT [FK_Ticket_NhanVienPhuTrach_AssignedToEmployeeId] FOREIGN KEY ([AssignedToEmployeeId]) REFERENCES [NhanVienPhuTrach] ([EmployeeId]) ON DELETE SET NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TicketComment')
BEGIN
    CREATE TABLE [TicketComment] (
        [Id] int NOT NULL IDENTITY,
        [TicketId] int NOT NULL,
        [UserId] int NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketComment] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketComment_Ticket_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Ticket] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TicketComment_NguoiDung_UserId] FOREIGN KEY ([UserId]) REFERENCES [NguoiDung] ([UserId]) ON DELETE SET NULL
    );
END
";
                context.Database.ExecuteSqlRaw(createTicketTablesSql);

                // Seed Tickets mẫu nếu chưa có
                if (!context.Tickets.Any() && context.KhachHangs.Any())
                {
                    var existingCustomers = context.KhachHangs.Take(4).ToList();
                    var existingEmp = context.NhanVienPhuTrachs.FirstOrDefault();
                    int? empId = existingEmp?.EmployeeId;

                    var sampleTickets = new[]
                    {
                        new Ticket
                        {
                            Title = "Lỗi kết nối máy xét nghiệm sinh hóa tự động AU680 với hệ thống LIS",
                            Description = "Khoa Xét nghiệm Bệnh viện Chợ Rẫy báo máy AU680 không truyền được kết quả về hệ thống LIS từ 8h30 sáng nay, làm chậm trễ trả kết quả cho bệnh nhân ngoại trú.",
                            CustomerId = existingCustomers[0].CustomerId,
                            Priority = TicketPriority.Critical,
                            Status = TicketStatus.InProgress,
                            AssignedToEmployeeId = empId,
                            CreatedAt = DateTime.Now.AddHours(-3),
                            SlaDeadline = DateTime.Now.AddHours(1)
                        },
                        new Ticket
                        {
                            Title = "Máy trạm đọc ảnh PACS tại Khoa CĐHA không tải được ảnh chụp CT 128 lát",
                            Description = "Bác sĩ phản ánh khi mở ca chụp CT Scanner 128 lát cắt thì ứng dụng PACS Viewer bị đơ và tải chậm quá 5 phút.",
                            CustomerId = existingCustomers.Count > 1 ? existingCustomers[1].CustomerId : existingCustomers[0].CustomerId,
                            Priority = TicketPriority.High,
                            Status = TicketStatus.New,
                            AssignedToEmployeeId = empId,
                            CreatedAt = DateTime.Now.AddHours(-5),
                            SlaDeadline = DateTime.Now.AddHours(7)
                        },
                        new Ticket
                        {
                            Title = "Không in được đơn thuốc BHYT mẫu mới theo Thông tư Bộ Y tế",
                            Description = "Phòng khám báo mẫu in đơn thuốc BHYT ngoại trú bị lỗi nhảy dòng mã vạch và thiếu chữ ký số bác sĩ.",
                            CustomerId = existingCustomers.Count > 2 ? existingCustomers[2].CustomerId : existingCustomers[0].CustomerId,
                            Priority = TicketPriority.Medium,
                            Status = TicketStatus.Resolved,
                            AssignedToEmployeeId = empId,
                            CreatedAt = DateTime.Now.AddDays(-1),
                            ResolvedAt = DateTime.Now.AddHours(-2),
                            SlaDeadline = DateTime.Now
                        },
                        new Ticket
                        {
                            Title = "Yêu cầu cấu hình thêm 5 tài khoản điều dưỡng Khoa Cấp cứu",
                            Description = "Bệnh viện tiếp nhận thêm nhân sự mới cho khoa Cấp cứu, cần tạo tài khoản phân quyền ghi nhận bệnh án điện tử EMR.",
                            CustomerId = existingCustomers.Count > 3 ? existingCustomers[3].CustomerId : existingCustomers[0].CustomerId,
                            Priority = TicketPriority.Low,
                            Status = TicketStatus.Closed,
                            AssignedToEmployeeId = empId,
                            CreatedAt = DateTime.Now.AddDays(-3),
                            ResolvedAt = DateTime.Now.AddDays(-2),
                            SlaDeadline = DateTime.Now.AddDays(-1)
                        },
                        new Ticket
                        {
                            Title = "Hệ thống HIS bị chậm giờ cao điểm tiếp đón bệnh nhân (7h30 - 8h30)",
                            Description = "Khu tiếp đón bệnh nhân báo phần mềm phản hồi chậm khi quét thẻ BHYT có gắn chip VssID vào đầu giờ sáng.",
                            CustomerId = existingCustomers[0].CustomerId,
                            Priority = TicketPriority.Critical,
                            Status = TicketStatus.InProgress,
                            AssignedToEmployeeId = empId,
                            CreatedAt = DateTime.Now.AddHours(-1),
                            SlaDeadline = DateTime.Now.AddHours(3)
                        }
                    };

                    context.Tickets.AddRange(sampleTickets);
                    context.SaveChanges();

                    context.TicketComments.Add(new TicketComment
                    {
                        TicketId = sampleTickets[0].Id,
                        Content = "Kỹ sư đã liên hệ với phụ trách IT Bệnh viện, đang kiểm tra lại cổng COM và driver máy xét nghiệm Beckman Coulter.",
                        CreatedAt = DateTime.Now.AddHours(-2)
                    });
                    context.TicketComments.Add(new TicketComment
                    {
                        TicketId = sampleTickets[0].Id,
                        Content = "Đã tìm ra nguyên nhân: Xung đột địa chỉ IP tĩnh trên switch mạng tầng 2. Đang cấu hình lại gateway.",
                        CreatedAt = DateTime.Now.AddMinutes(-30)
                    });
                    context.SaveChanges();
                }
            }
            catch { }

            // Nếu đã có khách hàng thì không seed dữ liệu CRM cũ nữa
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

            // 5. Seed Ticket Sự cố Y tế (Helpdesk)
            if (!context.Tickets.Any())
            {
                var dev = employees[0];
                var sampleTickets = new[]
                {
                    new Ticket
                    {
                        Title = "Lỗi kết nối máy xét nghiệm sinh hóa tự động AU680 với hệ thống LIS",
                        Description = "Khoa Xét nghiệm Bệnh viện Chợ Rẫy báo máy AU680 không truyền được kết quả về hệ thống LIS từ 8h30 sáng nay, làm chậm trễ trả kết quả cho bệnh nhân ngoại trú.",
                        CustomerId = customers[0].CustomerId,
                        Priority = TicketPriority.Critical,
                        Status = TicketStatus.InProgress,
                        AssignedToEmployeeId = dev.EmployeeId,
                        CreatedAt = DateTime.Now.AddHours(-3),
                        SlaDeadline = DateTime.Now.AddHours(1)
                    },
                    new Ticket
                    {
                        Title = "Máy trạm đọc ảnh PACS tại Khoa CĐHA không tải được ảnh chụp CT 128 lát",
                        Description = "Bác sĩ phản ánh khi mở ca chụp CT Scanner 128 lát cắt thì ứng dụng PACS Viewer bị đơ và tải chậm quá 5 phút.",
                        CustomerId = customers[1].CustomerId,
                        Priority = TicketPriority.High,
                        Status = TicketStatus.New,
                        AssignedToEmployeeId = dev.EmployeeId,
                        CreatedAt = DateTime.Now.AddHours(-5),
                        SlaDeadline = DateTime.Now.AddHours(7)
                    },
                    new Ticket
                    {
                        Title = "Không in được đơn thuốc BHYT mẫu mới theo Thông tư Bộ Y tế",
                        Description = "Phòng khám báo mẫu in đơn thuốc BHYT ngoại trú bị lỗi nhảy dòng mã vạch và thiếu chữ ký số bác sĩ.",
                        CustomerId = customers[2].CustomerId,
                        Priority = TicketPriority.Medium,
                        Status = TicketStatus.Resolved,
                        AssignedToEmployeeId = dev.EmployeeId,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        ResolvedAt = DateTime.Now.AddHours(-2),
                        SlaDeadline = DateTime.Now
                    },
                    new Ticket
                    {
                        Title = "Yêu cầu cấu hình thêm 5 tài khoản điều dưỡng Khoa Cấp cứu",
                        Description = "Bệnh viện tiếp nhận thêm nhân sự mới cho khoa Cấp cứu, cần tạo tài khoản phân quyền ghi nhận bệnh án điện tử EMR.",
                        CustomerId = customers[3].CustomerId,
                        Priority = TicketPriority.Low,
                        Status = TicketStatus.Closed,
                        AssignedToEmployeeId = dev.EmployeeId,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        ResolvedAt = DateTime.Now.AddDays(-2),
                        SlaDeadline = DateTime.Now.AddDays(-1)
                    },
                    new Ticket
                    {
                        Title = "Hệ thống HIS bị chậm giờ cao điểm tiếp đón bệnh nhân (7h30 - 8h30)",
                        Description = "Khu tiếp đón bệnh nhân báo phần mềm phản hồi chậm khi quét thẻ BHYT có gắn chip VssID vào đầu giờ sáng.",
                        CustomerId = customers[0].CustomerId,
                        Priority = TicketPriority.Critical,
                        Status = TicketStatus.InProgress,
                        AssignedToEmployeeId = dev.EmployeeId,
                        CreatedAt = DateTime.Now.AddHours(-1),
                        SlaDeadline = DateTime.Now.AddHours(3)
                    }
                };

                context.Tickets.AddRange(sampleTickets);
                context.SaveChanges();

                context.TicketComments.Add(new TicketComment
                {
                    TicketId = sampleTickets[0].Id,
                    Content = "Kỹ sư đã liên hệ với phụ trách IT Bệnh viện Chợ Rẫy, đang kiểm tra lại cổng COM và driver máy xét nghiệm Beckman Coulter.",
                    CreatedAt = DateTime.Now.AddHours(-2)
                });
                context.TicketComments.Add(new TicketComment
                {
                    TicketId = sampleTickets[0].Id,
                    Content = "Đã tìm ra nguyên nhân: Xung đột địa chỉ IP tĩnh trên switch mạng tầng 2. Đang cấu hình lại gateway.",
                    CreatedAt = DateTime.Now.AddMinutes(-30)
                });
                context.SaveChanges();
            }
        }
    }
}
