using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class AmenityConfig : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> b)
    {
        b.ToTable("Amenities");
        b.HasKey(x => x.AmenityId);

        b.Property(x => x.AmenityName)
            .HasMaxLength(100)
            .IsRequired();

        b.HasIndex(x => x.AmenityName).IsUnique();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
    }
}
