namespace BLL.DTOs.Tenant;

public class TenantDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IdentityUserId { get; set; }
    public bool IsBlacklisted { get; set; }
}
