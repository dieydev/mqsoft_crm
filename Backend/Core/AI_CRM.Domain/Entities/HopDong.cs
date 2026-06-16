namespace AI_CRM.Domain.Entities;

public enum TrangThaiHopDong
{
    HieuLuc,
    HetHan,
    DaThanhLy,
    DaGiaHan
}

public class HopDong : ThucTheCoSo
{
    public string SoHopDong { get; set; } = string.Empty;
    
    public Guid DuAnId { get; set; }
    public DuAn? DuAn { get; set; }

    public string? DuongDanFile { get; set; }
    
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    
    public decimal GiaTri { get; set; }
    public TrangThaiHopDong TrangThai { get; set; } = TrangThaiHopDong.HieuLuc;
}
