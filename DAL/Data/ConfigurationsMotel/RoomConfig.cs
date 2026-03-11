using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("Rooms");

        b.HasKey(x => x.RoomId);

        b.Property(x => x.RoomCode)
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.RoomName)
            .HasMaxLength(100);

        b.Property(x => x.CurrentBasePrice)
            .HasColumnType("decimal(18,2)");

        b.HasIndex(x => x.RoomCode).IsUnique();

        b.HasOne(x => x.Floor)
            .WithMany(f => f.Rooms)
            .HasForeignKey(x => x.FloorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
