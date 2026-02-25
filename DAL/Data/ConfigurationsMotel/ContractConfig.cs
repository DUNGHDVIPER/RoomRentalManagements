using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class ContractConfig : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> b)
    {
        b.ToTable("Contracts");
        b.HasKey(x => x.ContractId);

        b.Property(x => x.ContractCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.PaymentCycle).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Note).HasMaxLength(500);

        b.Property(x => x.BaseRent).HasColumnType("decimal(18,2)");
        b.Property(x => x.DepositAmount).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ ONE ACTIVE PER ROOM (lọc theo Status = 'Active')
        b.HasIndex(x => x.RoomId)
            .IsUnique()
            .HasDatabaseName("UX_Contracts_OneActiveContractPerRoom")
            .HasFilter("[Status] = 'Active'");
    }
}
