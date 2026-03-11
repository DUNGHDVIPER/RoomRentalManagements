using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class FloorConfig : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> b)
    {
        b.ToTable("Floors");
        b.HasKey(x => x.FloorId);

        b.Property(x => x.FloorName).HasMaxLength(50).IsRequired();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");

        b.HasOne(x => x.Block)
            .WithMany(bk => bk.Floors)
            .HasForeignKey(x => x.BlockId)
            .OnDelete(DeleteBehavior.Restrict);

        // MotelEntities KHÔNG có Level => bỏ index BlockId+Level
    }
}
