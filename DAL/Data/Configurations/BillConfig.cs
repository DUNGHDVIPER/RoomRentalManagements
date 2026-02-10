using DAL.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class BillConfig : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> b)
    {
        b.ToTable("Bills");
        b.HasKey(x => x.Id);

        b.Property(x => x.TotalAmount).HasPrecision(18, 2);

        b.HasOne(x => x.Contract)
         .WithMany(x => x.Bills)
         .HasForeignKey(x => x.ContractId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.BillItems)
         .WithOne(x => x.Bill)
         .HasForeignKey(x => x.BillId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Payments)
         .WithOne(x => x.Bill)
         .HasForeignKey(x => x.BillId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.ContractId, x.Period }).IsUnique();
    }
}
