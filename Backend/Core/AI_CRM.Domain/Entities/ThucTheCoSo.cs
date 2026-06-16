namespace AI_CRM.Domain.Entities;

public abstract class ThucTheCoSo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }
}
