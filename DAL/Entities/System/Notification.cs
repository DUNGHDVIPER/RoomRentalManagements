using DAL.Entities.Common;

namespace DAL.Entities.System;

public class Notification : AuditableEntity<long>
{
    public string? UserId { get; set; }          // Identity user id
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
