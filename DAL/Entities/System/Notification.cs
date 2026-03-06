using DAL.Entities.Common;
<<<<<<< HEAD
using DAL.Entities.Tenanting;
using DAL.Entities.Contracts;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574

namespace DAL.Entities.System;

public class Notification : AuditableEntity<long>
{
<<<<<<< HEAD
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public SourceType SourceType { get; set; }

    public int? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public string? ReceiverUserId { get; set; }


    [ForeignKey("ReceiverUserId")]
    public IdentityUser? ReceiverUser { get; set; }
=======
    public int CreatedByUserId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(1000)]
    public string Message { get; set; } = null!;

    [MaxLength(30)]
    public string Type { get; set; } = "Broadcast";

    public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574
}