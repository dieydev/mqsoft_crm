namespace AI_CRM.Domain.Entities;

public class ThanhVienDuAn : ThucTheCoSo
{
    public Guid DuAnId { get; set; }
    public DuAn? DuAn { get; set; }

    public string NguoiDungId { get; set; } = string.Empty;
    public NguoiDung? NguoiDung { get; set; }

    public string VaiTro { get; set; } = string.Empty; 
}
