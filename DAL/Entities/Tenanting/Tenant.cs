using DAL.Entities.Tenanting;
using DAL.Entities.Common;
using DAL.Entities.Contracts;

namespace DAL.Entities.Tenanting;

public class Tenant : AuditableEntity<int>
{
    public string FullName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Optional link to Identity User
    public string? IdentityUserId { get; set; }

    public ICollection<TenantIdDoc> IdDocs { get; set; } = new List<TenantIdDoc>();
    public ICollection<StayHistory> StayHistories { get; set; } = new List<StayHistory>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
