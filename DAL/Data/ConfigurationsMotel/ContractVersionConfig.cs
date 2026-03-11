using DAL.Entities.Motel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Data.ConfigurationsMotel;

public class ContractVersionConfig : IEntityTypeConfiguration<ContractVersion>
{
    public void Configure(EntityTypeBuilder<ContractVersion> b)
    {
        b.ToTable("ContractVersions");
        b.HasKey(x => x.VersionId);

        b.Property(x => x.ChangeNote).HasMaxLength(255);
        b.Property(x => x.SnapshotJson).IsRequired();

        b.HasOne(x => x.Contract)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.ContractId, x.VersionNumber }).IsUnique();
    }
}
