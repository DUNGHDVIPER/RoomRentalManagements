using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class TenantConfig : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("Tenants");
        b.HasKey(x => x.TenantId);

        b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(30).IsRequired();
        b.Property(x => x.Email).HasMaxLength(150);

        // Tenant.Documents (TenantDocument)
        b.HasMany(x => x.Documents)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tenant.RoomResidents (RoomResident)
        b.HasMany(x => x.RoomResidents)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
