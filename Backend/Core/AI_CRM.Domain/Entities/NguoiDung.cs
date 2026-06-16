using Microsoft.AspNetCore.Identity;

namespace AI_CRM.Domain.Entities;

public class NguoiDung : IdentityUser
{
    public string HoTen { get; set; } = string.Empty;
    public string? AnhDaiDien { get; set; }
    public string? PhongBan { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    
    public ICollection<ThanhVienDuAn> ThanhVienDuAns { get; set; } = new List<ThanhVienDuAn>();
    public ICollection<LichSuChamSoc> LichSuChamSocs { get; set; } = new List<LichSuChamSoc>();
}
