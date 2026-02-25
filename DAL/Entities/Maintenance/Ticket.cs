using System;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using DAL.Entities.Common;
using DAL.Entities.Property;
using DAL.Entities.Tenanting;

namespace DAL.Entities.Maintenance;

public class Ticket : AuditableEntity<int>
{
    public int RoomId { get; set; }
    public int? TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = null!;
    [Required]
    [MaxLength(255)]
    public string? Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public Room Room { get; set; } = null!;
    public Tenant? Tenant { get; set; }


    [Required]
    public string? Category { get; set; } = "Other";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
