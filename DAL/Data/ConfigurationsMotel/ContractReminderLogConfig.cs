using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class ContractReminderLogConfig : IEntityTypeConfiguration<ContractReminderLog>
{
    public void Configure(EntityTypeBuilder<ContractReminderLog> b)
    {
        b.HasIndex(x => new { x.ContractId, x.RemindType, x.RemindAtDate })
         .IsUnique();

        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}