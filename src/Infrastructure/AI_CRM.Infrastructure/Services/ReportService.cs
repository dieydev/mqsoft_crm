using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AI_CRM.Application.Interfaces;
using AI_CRM.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AI_CRM.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
            // Set QuestPDF license to Community
            QuestPDF.Settings.License = LicenseType.Community;
            
            // Đăng ký font Arial để hỗ trợ tiếng Việt trơn tru (tránh crash SkiaSharp)
            try 
            {
                var fontPath = @"C:\Windows\Fonts\arial.ttf";
                if (File.Exists(fontPath))
                {
                    using var stream = File.OpenRead(fontPath);
                    QuestPDF.Drawing.FontManager.RegisterFont(stream);
                }
            } 
            catch { }
        }

        public async Task<byte[]> ExportCustomersToExcelAsync()
        {
            var customers = await _context.KhachHangs.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachKhachHang");

            // Header
            worksheet.Cell(1, 1).Value = "Mã KH";
            worksheet.Cell(1, 2).Value = "Tên doanh nghiệp";
            worksheet.Cell(1, 3).Value = "Người đại diện";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Điện thoại";
            worksheet.Cell(1, 6).Value = "Địa chỉ";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            for (int i = 0; i < customers.Count; i++)
            {
                var row = i + 2;
                var customer = customers[i];
                worksheet.Cell(row, 1).Value = customer.CustomerId.ToString();
                worksheet.Cell(row, 2).Value = customer.CompanyName;
                worksheet.Cell(row, 3).Value = customer.Representative;
                worksheet.Cell(row, 4).Value = customer.Email;
                worksheet.Cell(row, 5).Value = customer.Phone;
                worksheet.Cell(row, 6).Value = customer.Address;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportProjectsToExcelAsync()
        {
            var projects = await _context.DuAns
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDuAn)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachDuAn");

            worksheet.Cell(1, 1).Value = "Mã Dự Án";
            worksheet.Cell(1, 2).Value = "Tên Dự Án";
            worksheet.Cell(1, 3).Value = "Khách Hàng";
            worksheet.Cell(1, 4).Value = "Ngày Bắt Đầu";
            worksheet.Cell(1, 5).Value = "Ngày Kết Thúc";
            worksheet.Cell(1, 6).Value = "Ngân Sách";
            worksheet.Cell(1, 7).Value = "Trạng Thái";

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < projects.Count; i++)
            {
                var row = i + 2;
                var project = projects[i];
                worksheet.Cell(row, 1).Value = project.ProjectId.ToString();
                worksheet.Cell(row, 2).Value = project.ProjectName;
                worksheet.Cell(row, 3).Value = project.KhachHang?.CompanyName ?? "";
                worksheet.Cell(row, 4).Value = project.StartDate?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 5).Value = project.EndDate?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 6).Value = project.Budget;
                worksheet.Cell(row, 7).Value = project.TrangThaiDuAn?.StatusName ?? "";
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportContractsToExcelAsync()
        {
            var contracts = await _context.HopDongs
                .Include(h => h.KhachHang)
                .Include(h => h.TrangThaiHopDong)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("DanhSachHopDong");

            worksheet.Cell(1, 1).Value = "Mã Hợp Đồng";
            worksheet.Cell(1, 2).Value = "Tên Hợp Đồng";
            worksheet.Cell(1, 3).Value = "Khách Hàng";
            worksheet.Cell(1, 4).Value = "Ngày Ký";
            worksheet.Cell(1, 5).Value = "Ngày Hết Hạn";
            worksheet.Cell(1, 6).Value = "Giá Trị";
            worksheet.Cell(1, 7).Value = "Trạng Thái";

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < contracts.Count; i++)
            {
                var row = i + 2;
                var contract = contracts[i];
                worksheet.Cell(row, 1).Value = contract.ContractId.ToString();
                worksheet.Cell(row, 2).Value = contract.ContractName;
                worksheet.Cell(row, 3).Value = contract.KhachHang?.CompanyName ?? "";
                worksheet.Cell(row, 4).Value = contract.SignDate?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 5).Value = contract.ExpireDate?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 6).Value = contract.ContractValue;
                worksheet.Cell(row, 7).Value = contract.TrangThaiHopDong?.StatusName ?? "";
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportCustomerReportToPdfAsync(int customerId)
        {
            var customer = await _context.KhachHangs
                .Include(k => k.DuAns)
                .Include(k => k.HopDongs)
                .FirstOrDefaultAsync(k => k.CustomerId == customerId);

            if (customer == null) throw new Exception("Không tìm thấy khách hàng");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header().Text("BÁO CÁO CHI TIẾT KHÁCH HÀNG")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Item().Text($"Tên doanh nghiệp: {customer.CompanyName}");
                        x.Item().Text($"Người đại diện: {customer.Representative}");
                        x.Item().Text($"Email: {customer.Email}");
                        x.Item().Text($"Điện thoại: {customer.Phone}");
                        x.Item().Text($"Địa chỉ: {customer.Address}");
                        
                        x.Item().PaddingTop(20).Text("Dự án liên quan").SemiBold().FontSize(14);
                        if (customer.DuAns.Any())
                        {
                            foreach(var project in customer.DuAns)
                            {
                                x.Item().Text($"- {project.ProjectName} ({(project.StartDate.HasValue ? project.StartDate.Value.ToString("dd/MM/yyyy") : "N/A")})");
                            }
                        }
                        else
                        {
                            x.Item().Text("Chưa có dự án nào.");
                        }

                        x.Item().PaddingTop(20).Text("Hợp đồng liên quan").SemiBold().FontSize(14);
                        if (customer.HopDongs.Any())
                        {
                            foreach(var contract in customer.HopDongs)
                            {
                                x.Item().Text($"- {contract.ContractName} (Giá trị: {contract.ContractValue:N0} VND)");
                            }
                        }
                        else
                        {
                            x.Item().Text("Chưa có hợp đồng nào.");
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportProjectReportToPdfAsync(int projectId)
        {
            var project = await _context.DuAns
                .Include(d => d.KhachHang)
                .Include(d => d.TrangThaiDuAn)
                .Include(d => d.TienDoDuAns)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null) throw new Exception("Không tìm thấy dự án");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header().Text("BÁO CÁO DỰ ÁN")
                        .SemiBold().FontSize(20).FontColor(Colors.Green.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Item().Text($"Tên dự án: {project.ProjectName}").SemiBold();
                        x.Item().Text($"Khách hàng: {project.KhachHang?.CompanyName ?? "N/A"}");
                        x.Item().Text($"Thời gian: {project.StartDate?.ToString("dd/MM/yyyy") ?? "N/A"} - {project.EndDate?.ToString("dd/MM/yyyy") ?? "N/A"}");
                        x.Item().Text($"Ngân sách: {project.Budget:N0} VND");
                        x.Item().Text($"Trạng thái: {project.TrangThaiDuAn?.StatusName ?? "N/A"}");

                        x.Item().PaddingTop(20).Text("Tiến độ dự án").SemiBold().FontSize(14);
                        if (project.TienDoDuAns.Any())
                        {
                            foreach(var task in project.TienDoDuAns.OrderByDescending(t => t.UpdateDate))
                            {
                                x.Item().Text($"- {task.UpdateDate:dd/MM/yyyy}: Hoàn thành {task.ProgressPercent}% - {task.Note}");
                            }
                        }
                        else
                        {
                            x.Item().Text("Chưa có cập nhật tiến độ.");
                        }
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Trang "); x.CurrentPageNumber(); });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportContractReportToPdfAsync(int contractId)
        {
            var contract = await _context.HopDongs
                .Include(h => h.KhachHang)
                .Include(h => h.TrangThaiHopDong)
                .FirstOrDefaultAsync(h => h.ContractId == contractId);

            if (contract == null) throw new Exception("Không tìm thấy hợp đồng");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header().Text("BÁO CÁO HỢP ĐỒNG")
                        .SemiBold().FontSize(20).FontColor(Colors.Purple.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Item().Text($"Tên hợp đồng: {contract.ContractName}").SemiBold();
                        x.Item().Text($"Khách hàng: {contract.KhachHang?.CompanyName ?? "N/A"}");
                        x.Item().Text($"Ngày ký: {contract.SignDate?.ToString("dd/MM/yyyy") ?? "N/A"}");
                        x.Item().Text($"Ngày hết hạn: {contract.ExpireDate?.ToString("dd/MM/yyyy") ?? "N/A"}");
                        x.Item().Text($"Giá trị hợp đồng: {contract.ContractValue:N0} VND");
                        x.Item().Text($"Trạng thái: {contract.TrangThaiHopDong?.StatusName ?? "N/A"}");
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Trang "); x.CurrentPageNumber(); });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportProgressReportToPdfAsync(int projectId)
        {
             var project = await _context.DuAns
                .Include(d => d.TienDoDuAns)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null) throw new Exception("Không tìm thấy dự án");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header().Text($"BÁO CÁO TIẾN ĐỘ: {project.ProjectName}")
                        .SemiBold().FontSize(20).FontColor(Colors.Orange.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        if (project.TienDoDuAns.Any())
                        {
                            foreach(var task in project.TienDoDuAns.OrderByDescending(t => t.UpdateDate))
                            {
                                x.Item().Text($"- {task.UpdateDate:dd/MM/yyyy}: Mức độ {task.ProgressPercent}%");
                                x.Item().Text($"  Ghi chú: {task.Note}");
                            }
                        }
                        else
                        {
                            x.Item().Text("Chưa có cập nhật tiến độ.");
                        }
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Trang "); x.CurrentPageNumber(); });
                });
            });

            return document.GeneratePdf();
        }
    }
}
