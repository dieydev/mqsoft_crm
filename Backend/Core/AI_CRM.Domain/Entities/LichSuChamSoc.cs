namespace AI_CRM.Domain.Entities;

public enum LoaiTuongTac
{
    GoiDien,
    Email,
    HopTrucTiep,
    Khac
}

public class LichSuChamSoc : ThucTheCoSo
{
    public Guid KhachHangId { get; set; }
    public KhachHang? KhachHang { get; set; }

    public string NhanVienPhuTrachId { get; set; } = string.Empty;
    public NguoiDung? NhanVienPhuTrach { get; set; }

    public LoaiTuongTac Loai { get; set; }
    public string? NoiDungTraoDoi { get; set; }
    public string? KetQua { get; set; }
    public DateTime NgayTuongTac { get; set; } = DateTime.UtcNow;
}
