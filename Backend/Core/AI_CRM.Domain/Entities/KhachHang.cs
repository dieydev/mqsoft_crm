namespace AI_CRM.Domain.Entities;

public class KhachHang : ThucTheCoSo
{
    public string TenDoanhNghiep { get; set; } = string.Empty;
    public string NguoiDaiDien { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? GhiChu { get; set; }

    public ICollection<DuAn> DuAns { get; set; } = new List<DuAn>();
    public ICollection<LichSuChamSoc> LichSuChamSocs { get; set; } = new List<LichSuChamSoc>();
}
