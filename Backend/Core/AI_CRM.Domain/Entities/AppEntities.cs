#nullable disable
using System;
using System.Collections.Generic;

namespace AI_CRM.Domain.Entities
{
    public class VaiTro
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        
        public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
    }

    public class NguoiDung
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public VaiTro VaiTro { get; set; }
        public ICollection<NhanVienPhuTrach> NhanVienPhuTrachs { get; set; } = new List<NhanVienPhuTrach>();
        public ICollection<LichSuHoiDap> LichSuHoiDaps { get; set; } = new List<LichSuHoiDap>();
        public ICollection<NhatKyHeThong> NhatKyHeThongs { get; set; } = new List<NhatKyHeThong>();
    }

    public class KhachHang
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CompanyName { get; set; }
        public string Representative { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        public ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();
        public ICollection<DuAn> DuAns { get; set; } = new List<DuAn>();
        public ICollection<LichSuLamViec> LichSuLamViecs { get; set; } = new List<LichSuLamViec>();
    }

    public class TrangThaiHopDong
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }

        public ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();
    }

    public class HopDong
    {
        public int ContractId { get; set; }
        public int CustomerId { get; set; }
        public int StatusId { get; set; }
        public string ContractNumber { get; set; }
        public string ContractName { get; set; }
        public decimal? ContractValue { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        public KhachHang KhachHang { get; set; }
        public TrangThaiHopDong TrangThaiHopDong { get; set; }
        public ICollection<DuAn> DuAns { get; set; } = new List<DuAn>();
    }

    public class TrangThaiDuAn
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }

        public ICollection<DuAn> DuAns { get; set; } = new List<DuAn>();
    }

    public class DuAn
    {
        public int ProjectId { get; set; }
        public int CustomerId { get; set; }
        public int? ContractId { get; set; }
        public int StatusId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Deadline { get; set; }
        public decimal? Budget { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;

        public KhachHang KhachHang { get; set; }
        public HopDong HopDong { get; set; }
        public TrangThaiDuAn TrangThaiDuAn { get; set; }
        public ICollection<DuAnNhanVien> DuAnNhanViens { get; set; } = new List<DuAnNhanVien>();
        public ICollection<TienDoDuAn> TienDoDuAns { get; set; } = new List<TienDoDuAn>();
    }

    public class NhanVienPhuTrach
    {
        public int EmployeeId { get; set; }
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public bool IsActive { get; set; } = true;

        public NguoiDung NguoiDung { get; set; }
        public ICollection<DuAnNhanVien> DuAnNhanViens { get; set; } = new List<DuAnNhanVien>();
        public ICollection<TienDoDuAn> TienDoDuAns { get; set; } = new List<TienDoDuAn>();
        public ICollection<LichSuLamViec> LichSuLamViecs { get; set; } = new List<LichSuLamViec>();
        public ICollection<TaiLieuNoiBo> TaiLieuNoiBos { get; set; } = new List<TaiLieuNoiBo>();
    }

    public class DuAnNhanVien
    {
        public int AssignmentId { get; set; }
        public int ProjectId { get; set; }
        public int EmployeeId { get; set; }
        public string ProjectRole { get; set; }
        public DateTime? JoinDate { get; set; }

        public DuAn DuAn { get; set; }
        public NhanVienPhuTrach NhanVienPhuTrach { get; set; }
    }

    public class TienDoDuAn
    {
        public int ProgressId { get; set; }
        public int ProjectId { get; set; }
        public int? ProgressPercent { get; set; }
        public string Note { get; set; }
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }

        public DuAn DuAn { get; set; }
        public NhanVienPhuTrach NhanVienPhuTrach { get; set; }
    }

    public class LichSuLamViec
    {
        public int WorkLogId { get; set; }
        public int CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public string InteractionType { get; set; }
        public DateTime WorkDate { get; set; } = DateTime.Now;
        public string Content { get; set; }
        public string Result { get; set; }

        public KhachHang KhachHang { get; set; }
        public NhanVienPhuTrach NhanVienPhuTrach { get; set; }
    }

    public class NhomTaiLieu
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public ICollection<TaiLieuNoiBo> TaiLieuNoiBos { get; set; } = new List<TaiLieuNoiBo>();
    }

    public class TaiLieuNoiBo
    {
        public int DocumentId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public int UploadedBy { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public bool IsProcessedForAI { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public NhomTaiLieu NhomTaiLieu { get; set; }
        public NhanVienPhuTrach NhanVienPhuTrach { get; set; }
        public ICollection<TaiLieuTag> TaiLieuTags { get; set; } = new List<TaiLieuTag>();
    }

    public class TaiLieuTag
    {
        public int TagId { get; set; }
        public int DocumentId { get; set; }
        public string TagName { get; set; }

        public TaiLieuNoiBo TaiLieuNoiBo { get; set; }
    }

    public class LichSuHoiDap
    {
        public int ChatHistoryId { get; set; }
        public int UserId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public NguoiDung NguoiDung { get; set; }
        public ICollection<DanhGiaCauTraLoi> DanhGiaCauTraLois { get; set; } = new List<DanhGiaCauTraLoi>();
    }

    public class DanhGiaCauTraLoi
    {
        public int FeedbackId { get; set; }
        public int ChatHistoryId { get; set; }
        public bool IsHelpful { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public LichSuHoiDap LichSuHoiDap { get; set; }
    }

    public class NhatKyHeThong
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordId { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public NguoiDung NguoiDung { get; set; }
    }
}
