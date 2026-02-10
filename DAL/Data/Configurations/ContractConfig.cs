using DAL.Entities.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.Configurations;

public class ContractConfig : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> b)
    {
        b.ToTable("Contracts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Deposit).HasPrecision(18, 2);
        b.Property(x => x.Rent).HasPrecision(18, 2);

        b.HasOne(x => x.Room)
         .WithMany(x => x.Contracts)
         .HasForeignKey(x => x.RoomId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Tenant)
         .WithMany(x => x.Contracts)
         .HasForeignKey(x => x.TenantId)
         .OnDelete(DeleteBehavior.Restrict);

        // Rule: chỉ 1 active contract per room
        b.HasIndex(x => x.RoomId)
         .IsUnique()
         .HasFilter("[IsActive] = 1")
         .HasDatabaseName("UX_Contracts_Room_ActiveOnly");

        b.HasIndex(x => new { x.TenantId, x.StartDate });
    }
}
