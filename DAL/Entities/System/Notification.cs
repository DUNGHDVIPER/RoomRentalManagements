using DAL.Entities.Common;
using DAL.Entities.Tenanting;
using DAL.Entities.Contracts;

namespace DAL.Entities.System;

public class Notification : AuditableEntity<long>
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public SourceType SourceType { get; set; }

    // 🔥 chỉ cần Contract
    public int? ContractId { get; set; }
    public Contract? Contract { get; set; }
}