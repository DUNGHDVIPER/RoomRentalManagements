/*using DAL.Entities.Motel;
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
        b.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        b.Property(x => x.UserId).HasMaxLength(450); // Identity key length safe

        b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
    }
}
*/