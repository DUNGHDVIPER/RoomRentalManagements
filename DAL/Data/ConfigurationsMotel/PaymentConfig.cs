using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.HasKey(x => x.PaymentId);

        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Method).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(255);

        b.HasOne(x => x.Bill)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.BillId, x.PaidAt });
    }
}
