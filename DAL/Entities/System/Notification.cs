using DAL.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities.System;

public class Notification : AuditableEntity<long>
{
    public int CreatedByUserId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(1000)]
    public string Message { get; set; } = null!;

    [MaxLength(30)]
    public string Type { get; set; } = "Broadcast";

    public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
}