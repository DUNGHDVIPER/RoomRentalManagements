using DAL.Entities.Common;

namespace DAL.Entities.Contracts;

public class ContractVersion : AuditableEntity<int>
{
    public int ContractId { get; set; }
    public int VersionNo { get; set; }

    // Snapshot (chưa cần chuẩn hoá ngay)
    public string SnapshotJson { get; set; } = "{}";
    public string? ChangedByUserId { get; set; }

    public Contract Contract { get; set; } = null!;
}
