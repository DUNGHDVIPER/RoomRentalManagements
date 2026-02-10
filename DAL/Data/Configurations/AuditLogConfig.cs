using DAL.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(x => x.Id);

        b.Property(x => x.UserId).HasMaxLength(450);
        b.Property(x => x.Action).HasMaxLength(50).IsRequired();
        b.Property(x => x.EntityName).HasMaxLength(80).IsRequired();
        b.Property(x => x.EntityKey).HasMaxLength(120);
        b.Property(x => x.IpAddress).HasMaxLength(45); // IPv6 safe
        b.Property(x => x.Description).HasMaxLength(2000);

        b.HasIndex(x => new { x.UserId, x.Action, x.CreatedAt });
    }
}
