using Microsoft.EntityFrameworkCore;
using AI_CRM.Domain.Entities;

namespace AI_CRM.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly AI_CRM.Application.Interfaces.ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, AI_CRM.Application.Interfaces.ICurrentUserService currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public override async System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not NhatKyHeThong && 
                        e.Entity is not LichSuHoiDap && 
                        e.Entity is not DanhGiaCauTraLoi &&
                        e.Entity is not LichSuLamViec &&
                        e.Entity is not TienDoDuAn &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        if (entries.Any())
        {
            int? userId = _currentUserService?.UserId;

            foreach (var entry in entries)
            {
                string action = entry.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                };

                var tableName = entry.Entity.GetType().Name;
                var details = $"Hệ thống tự động ghi nhận: {action} dữ liệu ở bảng {tableName}";

                int? recordId = null;
                var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                if (keyProperty != null && keyProperty.CurrentValue is int idVal)
                {
                    recordId = idVal;
                    details += $" (Mã bản ghi: {recordId})";
                }

                NhatKyHeThongs.Add(new NhatKyHeThong
                {
                    UserId = userId,
                    Action = action,
                    TableName = tableName,
                    RecordId = recordId,
                    Details = details,
                    Timestamp = System.DateTime.Now
                });
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<VaiTro> VaiTros { get; set; }
    public DbSet<NguoiDung> NguoiDungs { get; set; }
    public DbSet<KhachHang> KhachHangs { get; set; }
    public DbSet<TrangThaiHopDong> TrangThaiHopDongs { get; set; }
    public DbSet<HopDong> HopDongs { get; set; }
    public DbSet<TrangThaiDuAn> TrangThaiDuAns { get; set; }
    public DbSet<DuAn> DuAns { get; set; }
    public DbSet<NhanVienPhuTrach> NhanVienPhuTrachs { get; set; }
    public DbSet<DuAnNhanVien> DuAnNhanViens { get; set; }
    public DbSet<TienDoDuAn> TienDoDuAns { get; set; }
    public DbSet<LichSuLamViec> LichSuLamViecs { get; set; }
    public DbSet<NhomTaiLieu> NhomTaiLieus { get; set; }
    public DbSet<TaiLieuNoiBo> TaiLieuNoiBos { get; set; }
    public DbSet<TaiLieuTag> TaiLieuTags { get; set; }
    public DbSet<LichSuHoiDap> LichSuHoiDaps { get; set; }
    public DbSet<DanhGiaCauTraLoi> DanhGiaCauTraLois { get; set; }
    public DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Map explicitly to singular table names as in CSDL.txt
        builder.Entity<VaiTro>().ToTable("VaiTro");
        builder.Entity<NguoiDung>().ToTable("NguoiDung");
        builder.Entity<KhachHang>().ToTable("KhachHang");
        builder.Entity<TrangThaiHopDong>().ToTable("TrangThaiHopDong");
        builder.Entity<HopDong>().ToTable("HopDong");
        builder.Entity<TrangThaiDuAn>().ToTable("TrangThaiDuAn");
        builder.Entity<DuAn>().ToTable("DuAn");
        builder.Entity<NhanVienPhuTrach>().ToTable("NhanVienPhuTrach");
        builder.Entity<DuAnNhanVien>().ToTable("DuAnNhanVien");
        builder.Entity<TienDoDuAn>().ToTable("TienDoDuAn");
        builder.Entity<LichSuLamViec>().ToTable("LichSuLamViec");
        builder.Entity<NhomTaiLieu>().ToTable("NhomTaiLieu");
        builder.Entity<TaiLieuNoiBo>().ToTable("TaiLieuNoiBo");
        builder.Entity<TaiLieuTag>().ToTable("TaiLieuTag");
        builder.Entity<LichSuHoiDap>().ToTable("LichSuHoiDap");
        builder.Entity<DanhGiaCauTraLoi>().ToTable("DanhGiaCauTraLoi");
        builder.Entity<NhatKyHeThong>().ToTable("NhatKyHeThong");

        // NguoiDung -> VaiTro
        builder.Entity<NguoiDung>()
            .HasOne(u => u.VaiTro)
            .WithMany(r => r.NguoiDungs)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // HopDong -> KhachHang
        builder.Entity<HopDong>()
            .HasOne(h => h.KhachHang)
            .WithMany(k => k.HopDongs)
            .HasForeignKey(h => h.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // HopDong -> TrangThaiHopDong
        builder.Entity<HopDong>()
            .HasOne(h => h.TrangThaiHopDong)
            .WithMany(t => t.HopDongs)
            .HasForeignKey(h => h.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // DuAn -> KhachHang
        builder.Entity<DuAn>()
            .HasOne(d => d.KhachHang)
            .WithMany(k => k.DuAns)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // DuAn -> HopDong
        builder.Entity<DuAn>()
            .HasOne(d => d.HopDong)
            .WithMany(h => h.DuAns)
            .HasForeignKey(d => d.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        // DuAn -> TrangThaiDuAn
        builder.Entity<DuAn>()
            .HasOne(d => d.TrangThaiDuAn)
            .WithMany(t => t.DuAns)
            .HasForeignKey(d => d.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // NhanVienPhuTrach -> NguoiDung
        builder.Entity<NhanVienPhuTrach>()
            .HasOne(nv => nv.NguoiDung)
            .WithMany(u => u.NhanVienPhuTrachs)
            .HasForeignKey(nv => nv.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // DuAnNhanVien -> DuAn & NhanVienPhuTrach
        builder.Entity<DuAnNhanVien>()
            .HasKey(danv => danv.AssignmentId);

        builder.Entity<DuAnNhanVien>()
            .HasOne(danv => danv.DuAn)
            .WithMany(d => d.DuAnNhanViens)
            .HasForeignKey(danv => danv.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DuAnNhanVien>()
            .HasOne(danv => danv.NhanVienPhuTrach)
            .WithMany(nv => nv.DuAnNhanViens)
            .HasForeignKey(danv => danv.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // TienDoDuAn
        builder.Entity<TienDoDuAn>()
            .HasKey(td => td.ProgressId);
        
        builder.Entity<TienDoDuAn>()
            .HasOne(td => td.DuAn)
            .WithMany(d => d.TienDoDuAns)
            .HasForeignKey(td => td.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TienDoDuAn>()
            .HasOne(td => td.NhanVienPhuTrach)
            .WithMany(nv => nv.TienDoDuAns)
            .HasForeignKey(td => td.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // LichSuLamViec
        builder.Entity<LichSuLamViec>()
            .HasKey(ls => ls.WorkLogId);

        builder.Entity<LichSuLamViec>()
            .HasOne(ls => ls.KhachHang)
            .WithMany(k => k.LichSuLamViecs)
            .HasForeignKey(ls => ls.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LichSuLamViec>()
            .HasOne(ls => ls.NhanVienPhuTrach)
            .WithMany(nv => nv.LichSuLamViecs)
            .HasForeignKey(ls => ls.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaiLieuNoiBo
        builder.Entity<TaiLieuNoiBo>()
            .HasKey(tl => tl.DocumentId);

        builder.Entity<TaiLieuNoiBo>()
            .HasOne(tl => tl.NhomTaiLieu)
            .WithMany(nt => nt.TaiLieuNoiBos)
            .HasForeignKey(tl => tl.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TaiLieuNoiBo>()
            .HasOne(tl => tl.NhanVienPhuTrach)
            .WithMany(nv => nv.TaiLieuNoiBos)
            .HasForeignKey(tl => tl.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // TaiLieuTag
        builder.Entity<TaiLieuTag>()
            .HasKey(t => t.TagId);

        builder.Entity<TaiLieuTag>()
            .HasOne(t => t.TaiLieuNoiBo)
            .WithMany(tl => tl.TaiLieuTags)
            .HasForeignKey(t => t.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // LichSuHoiDap
        builder.Entity<LichSuHoiDap>()
            .HasKey(ls => ls.ChatHistoryId);
            
        builder.Entity<LichSuHoiDap>()
            .HasOne(ls => ls.NguoiDung)
            .WithMany(u => u.LichSuHoiDaps)
            .HasForeignKey(ls => ls.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // DanhGiaCauTraLoi
        builder.Entity<DanhGiaCauTraLoi>()
            .HasKey(dg => dg.FeedbackId);

        builder.Entity<DanhGiaCauTraLoi>()
            .HasOne(dg => dg.LichSuHoiDap)
            .WithMany(ls => ls.DanhGiaCauTraLois)
            .HasForeignKey(dg => dg.ChatHistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // NhatKyHeThong
        builder.Entity<NhatKyHeThong>()
            .HasKey(nk => nk.LogId);

        builder.Entity<NhatKyHeThong>()
            .HasOne(nk => nk.NguoiDung)
            .WithMany(u => u.NhatKyHeThongs)
            .HasForeignKey(nk => nk.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Setting PKs for simple tables
        builder.Entity<VaiTro>().HasKey(v => v.RoleId);
        builder.Entity<NguoiDung>().HasKey(n => n.UserId);
        builder.Entity<KhachHang>().HasKey(k => k.CustomerId);
        builder.Entity<TrangThaiHopDong>().HasKey(t => t.StatusId);
        builder.Entity<HopDong>().HasKey(h => h.ContractId);
        builder.Entity<TrangThaiDuAn>().HasKey(t => t.StatusId);
        builder.Entity<DuAn>().HasKey(d => d.ProjectId);
        builder.Entity<NhanVienPhuTrach>().HasKey(n => n.EmployeeId);
        builder.Entity<NhomTaiLieu>().HasKey(n => n.CategoryId);
    }
}
