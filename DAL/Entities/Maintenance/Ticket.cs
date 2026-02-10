using DAL.Entities.Common;
using DAL.Entities.Property;
using DAL.Entities.Tenanting;

namespace DAL.Entities.Maintenance;

public class Ticket : AuditableEntity<int>
{
    public int RoomId { get; set; }
    public int? TenantId { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public Room Room { get; set; } = null!;
    public Tenant? Tenant { get; set; }
}
