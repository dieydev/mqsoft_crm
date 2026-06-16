namespace AI_CRM.Domain.Entities;

public enum PhanLoaiTaiLieu
{
    QuyTrinh,
    KyThuat,
    HuongDanSuDung,
    Khac
}

public class TaiLieu : ThucTheCoSo
{
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public PhanLoaiTaiLieu NhomTaiLieu { get; set; }
    
    public string DuongDanFile { get; set; } = string.Empty;
    public string NoiDungVanBan { get; set; } = string.Empty; // Dung cho Chatbot AI doc

    public string NguoiTaiLenId { get; set; } = string.Empty;
    public NguoiDung? NguoiTaiLen { get; set; }
}
