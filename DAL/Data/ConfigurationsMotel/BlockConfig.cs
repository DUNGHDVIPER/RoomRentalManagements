using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class BlockConfig : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> b)
    {
        b.ToTable("Blocks");
        b.HasKey(x => x.BlockId);

        b.Property(x => x.BlockName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Address).HasMaxLength(255);
        b.Property(x => x.Note).HasMaxLength(255);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
    }
}
