using DAL.Entities.Property;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class AmenityConfig : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> b)
    {
        b.ToTable("Amenities");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(80).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}
