namespace RoadRegistry.Integration.Schema.Organizations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrganizationConfiguration : IEntityTypeConfiguration<OrganizationLatestItem>
{
    public const string TableName = "organization_latest_items";

    public void Configure(EntityTypeBuilder<OrganizationLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Code)
            .IsClustered();

        b.Property(p => p.Code).ValueGeneratedNever().IsRequired();
        b.Property(p => p.OvoCode).IsRequired(false);
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(p => p.Code).HasColumnName("code");
        b.Property(p => p.Name).HasColumnName("name");
        b.Property(p => p.OvoCode).HasColumnName("ovo_code");
        b.Property(p => p.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.VersionTimestamp).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnTimestamp).HasColumnName("created_on_timestamp");

        b.HasIndex(p => p.OvoCode);
        b.HasIndex(p => p.VersionTimestamp);
        b.HasIndex(p => p.IsRemoved);
    }
}
