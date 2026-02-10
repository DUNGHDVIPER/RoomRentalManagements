using DAL.Entities.Billing;
using DAL.Entities.Contracts;
using DAL.Entities.Common;
using DAL.Entities.Property;
using DAL.Entities.Tenanting;

namespace DAL.Entities.Contracts;

public class Contract : AuditableEntity<int>
{
    public int RoomId { get; set; }
    public int TenantId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal Deposit { get; set; }
    public decimal Rent { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    // Rule: chỉ 1 active contract per room (enforced by filtered unique index)
    public bool IsActive { get; set; }

    public Room Room { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;

    public ICollection<ContractVersion> Versions { get; set; } = new List<ContractVersion>();
    public ICollection<ContractReminder> Reminders { get; set; } = new List<ContractReminder>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
