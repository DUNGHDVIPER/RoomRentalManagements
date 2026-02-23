using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class RoomPricingHistoryConfig : IEntityTypeConfiguration<RoomPricingHistory>
{
    public void Configure(EntityTypeBuilder<RoomPricingHistory> b)
    {
        b.ToTable("RoomPricingHistory");
        b.HasKey(x => x.PriceId);

        b.Property(x => x.OldPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.NewPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.ChangedAt).HasDefaultValueSql("SYSDATETIME()");

        b.HasOne(x => x.Room)
            .WithMany(r => r.PricingHistories)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
