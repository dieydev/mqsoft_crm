namespace AI_CRM.Domain.Entities;

public enum TrangThaiDuAn
{
    DangLenKeHoach,
    DangTrienKhai,
    HoanThanh,
    TamHoan,
    DaHuy
}

public class DuAn : ThucTheCoSo
{
    public string TenDuAn { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    
    public Guid KhachHangId { get; set; }
    public KhachHang? KhachHang { get; set; }

    public TrangThaiDuAn TrangThai { get; set; } = TrangThaiDuAn.DangLenKeHoach;
    public int PhanTramHoanThanh { get; set; } = 0;

    public DateTime NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }

    public ICollection<ThanhVienDuAn> ThanhViens { get; set; } = new List<ThanhVienDuAn>();
    public ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();
}
