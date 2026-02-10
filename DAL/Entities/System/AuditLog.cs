using DAL.Entities.Common;

namespace DAL.Entities.System;

public class AuditLog : AuditableEntity<long>
{
    public string? UserId { get; set; }
    public string Action { get; set; } = null!;     // Create/Update/Delete/Login…
    public string EntityName { get; set; } = null!; // Room/Contract/Bill…
    public string? EntityKey { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
}
