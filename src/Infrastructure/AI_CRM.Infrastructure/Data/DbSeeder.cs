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
                new KhachHang { TenKhachHang = "Bệnh viện Chợ Rẫy", NguoiLienHe = "Nguyễn Văn A", SoDienThoai = "0901234567", Email = "contact@choray.vn", DiaChi = "201B Nguyễn Chí Thanh, Q5", LoaiKhachHang = "Bệnh viện công", NgayTao = DateTime.Now.AddMonths(-5) },
                new KhachHang { TenKhachHang = "BV Đại học Y Dược", NguoiLienHe = "Trần Thị B", SoDienThoai = "0912345678", Email = "info@ump.edu.vn", DiaChi = "215 Hồng Bàng, Q5", LoaiKhachHang = "Bệnh viện công", NgayTao = DateTime.Now.AddMonths(-4) },
                new KhachHang { TenKhachHang = "Phòng khám Hoàn Mỹ", NguoiLienHe = "Lê Hoàng C", SoDienThoai = "0923456789", Email = "contact@hoanmy.com", DiaChi = "Phú Nhuận, HCM", LoaiKhachHang = "Tư nhân", NgayTao = DateTime.Now.AddMonths(-3) },
                new KhachHang { TenKhachHang = "BV Tâm Anh", NguoiLienHe = "Phạm D", SoDienThoai = "0934567890", Email = "info@tamanhhospital.vn", DiaChi = "Tân Bình, HCM", LoaiKhachHang = "Tư nhân", NgayTao = DateTime.Now.AddMonths(-2) },
                new KhachHang { TenKhachHang = "Sở Y Tế HCM", NguoiLienHe = "Võ E", SoDienThoai = "0945678901", Email = "syt@tphcm.gov.vn", DiaChi = "Q3, HCM", LoaiKhachHang = "Cơ quan nhà nước", NgayTao = DateTime.Now.AddMonths(-1) }
            };
            context.KhachHangs.AddRange(customers);
            context.SaveChanges();

            // 2. Seed Dự án
            var projects = new[]
            {
                new DuAn { TenDuAn = "Triển khai HIS 2.0", MoTa = "Nâng cấp hệ thống quản lý bệnh viện", NgayBatDau = DateTime.Now.AddMonths(-4), TrangThai = "Đang thực hiện", CustomerId = customers[0].CustomerId },
                new DuAn { TenDuAn = "Hệ thống PACS", MoTa = "Quản lý hình ảnh y tế", NgayBatDau = DateTime.Now.AddMonths(-3), TrangThai = "Hoàn thành", CustomerId = customers[1].CustomerId },
                new DuAn { TenDuAn = "Phần mềm Quản lý Phòng khám", MoTa = "HIS cho tư nhân", NgayBatDau = DateTime.Now.AddMonths(-1), TrangThai = "Mới tạo", CustomerId = customers[2].CustomerId },
                new DuAn { TenDuAn = "Bảo trì LIS", MoTa = "Bảo trì hệ thống xét nghiệm", NgayBatDau = DateTime.Now.AddMonths(-2), TrangThai = "Đang thực hiện", CustomerId = customers[3].CustomerId }
            };
            context.DuAns.AddRange(projects);
            context.SaveChanges();

            // 3. Seed Hợp đồng
            var contracts = new[]
            {
                new HopDong { TenHopDong = "HĐ HIS Chợ Rẫy", GiaTri = 1500000000, NgayKy = DateTime.Now.AddMonths(-4), TrangThai = "Đang hiệu lực", ProjectId = projects[0].ProjectId },
                new HopDong { TenHopDong = "HĐ PACS Y Dược", GiaTri = 2000000000, NgayKy = DateTime.Now.AddMonths(-3), TrangThai = "Đã thanh lý", ProjectId = projects[1].ProjectId },
                new HopDong { TenHopDong = "HĐ Phần mềm Hoàn Mỹ", GiaTri = 500000000, NgayKy = DateTime.Now.AddMonths(-1), TrangThai = "Mới ký", ProjectId = projects[2].ProjectId },
                new HopDong { TenHopDong = "HĐ Bảo trì Tâm Anh", GiaTri = 300000000, NgayKy = DateTime.Now.AddMonths(-2), TrangThai = "Đang hiệu lực", ProjectId = projects[3].ProjectId }
            };
            context.HopDongs.AddRange(contracts);
            context.SaveChanges();
            
            // 4. Seed Employee
            var employees = new[]
            {
                new NhanVien { TenNhanVien = "Trần Văn Dev", Email = "dev1@mq.vn", ChucVu = "Developer", PhongBan = "Kỹ thuật" },
                new NhanVien { TenNhanVien = "Lê Thị Sale", Email = "sale1@mq.vn", ChucVu = "Sales", PhongBan = "Kinh doanh" },
                new NhanVien { TenNhanVien = "Nguyễn Văn BA", Email = "ba1@mq.vn", ChucVu = "Business Analyst", PhongBan = "Phân tích" }
            };
            context.NhanViens.AddRange(employees);
            context.SaveChanges();
        }
    }
}
