namespace AI_CRM.Domain.Entities;

public class LichSuChat : ThucTheCoSo
{
    public string NguoiDungId { get; set; } = string.Empty;
    public NguoiDung? NguoiDung { get; set; }

    public string CauHoi { get; set; } = string.Empty;
    public string CauTraLoiAi { get; set; } = string.Empty;
    
    public bool? HuuIch { get; set; }
    public string? PhanHoi { get; set; }
}
