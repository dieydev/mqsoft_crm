using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AI_CRM.Domain.Entities;

namespace AI_CRM.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<NguoiDung>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<DuAn> DuAns => Set<DuAn>();
    public DbSet<ThanhVienDuAn> ThanhVienDuAns => Set<ThanhVienDuAn>();
    public DbSet<HopDong> HopDongs => Set<HopDong>();
    public DbSet<LichSuChamSoc> LichSuChamSocs => Set<LichSuChamSoc>();
    public DbSet<TaiLieu> TaiLieus => Set<TaiLieu>();
    public DbSet<LichSuChat> LichSuChats => Set<LichSuChat>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ThanhVienDuAn>()
            .HasKey(pm => pm.Id);

        builder.Entity<ThanhVienDuAn>()
            .HasOne(pm => pm.DuAn)
            .WithMany(p => p.ThanhViens)
            .HasForeignKey(pm => pm.DuAnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ThanhVienDuAn>()
            .HasOne(pm => pm.NguoiDung)
            .WithMany(u => u.ThanhVienDuAns)
            .HasForeignKey(pm => pm.NguoiDungId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LichSuChamSoc>()
            .HasOne(ci => ci.KhachHang)
            .WithMany(c => c.LichSuChamSocs)
            .HasForeignKey(ci => ci.KhachHangId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LichSuChamSoc>()
            .HasOne(ci => ci.NhanVienPhuTrach)
            .WithMany(u => u.LichSuChamSocs)
            .HasForeignKey(ci => ci.NhanVienPhuTrachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HopDong>()
            .HasOne(c => c.DuAn)
            .WithMany(p => p.HopDongs)
            .HasForeignKey(c => c.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DuAn>()
            .HasOne(p => p.KhachHang)
            .WithMany(c => c.DuAns)
            .HasForeignKey(p => p.KhachHangId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
