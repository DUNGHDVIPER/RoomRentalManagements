using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class BillConfig : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> b)
    {
        b.ToTable("Bills");
        b.HasKey(x => x.BillId);

        b.Property(x => x.MonthKey).HasColumnType("char(7)").IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();

        b.HasIndex(x => new { x.RoomId, x.MonthKey }).IsUnique();

        b.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId);

        b.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId);

        b.HasOne(x => x.Contract)
    .WithMany(c => c.Bills)
    .HasForeignKey(x => x.ContractId)
    .OnDelete(DeleteBehavior.Restrict);


        // Navigation collections (nếu entity Bill có khai báo)
        b.HasMany(x => x.Items)
            .WithOne(x => x.Bill)
            .HasForeignKey(x => x.BillId);

        b.HasMany(x => x.Payments)
            .WithOne(x => x.Bill)
            .HasForeignKey(x => x.BillId);

        b.HasMany(x => x.StatusHistories)
            .WithOne(x => x.Bill)
            .HasForeignKey(x => x.BillId);
    }
}
