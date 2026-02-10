using DAL.Entities.Common;

namespace DAL.Entities.Contracts;

public class ContractReminder : AuditableEntity<int>
{
    public int ContractId { get; set; }
    public DateTime RemindAt { get; set; }
    public string Type { get; set; } = "Expiry"; // Expiry/Payment/Other
    public bool IsSent { get; set; }

    public Contract Contract { get; set; } = null!;
}
