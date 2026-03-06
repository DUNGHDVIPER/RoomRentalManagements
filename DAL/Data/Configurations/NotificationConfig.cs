using DAL.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(x => x.Id);

        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
<<<<<<< HEAD
        b.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        //b.Property(x => x.UserId).HasMaxLength(450); // Identity key length safe

        b.HasIndex(x => new { x.IsRead, x.CreatedAt });
=======
        b.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Type).HasMaxLength(450); // Identity key length safe

        b.HasIndex(x => new { x.Type, x.CreatedAt });
>>>>>>> 600f8e0e5ea5cddd3d355e4e0373beb5ad375574
    }
}
