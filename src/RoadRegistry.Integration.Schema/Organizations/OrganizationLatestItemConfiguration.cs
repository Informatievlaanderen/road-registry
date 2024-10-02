namespace RoadRegistry.Integration.Schema.Organizations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrganizationLatestItemConfiguration : IEntityTypeConfiguration<OrganizationLatestItem>
{
    public const string TableName = "organization_latest_items";

    public void Configure(EntityTypeBuilder<OrganizationLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Code)
            .IsClustered();

        b.Property(p => p.Code).ValueGeneratedNever().IsRequired();
        b.Property(p => p.OvoCode).IsRequired(false);
        b.Property(p => p.KboNumber).IsRequired(false);
        b.Property(p => p.IsMaintainer).HasDefaultValue(false).IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(p => p.Code).HasColumnName("code");
        b.Property(p => p.Name).HasColumnName("name");
        b.Property(p => p.OvoCode).HasColumnName("ovo_code");
        b.Property(p => p.KboNumber).HasColumnName("kbo_number");
        b.Property(p => p.IsMaintainer).HasColumnName("is_maintainer");
        b.Property(p => p.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.VersionAsString).HasColumnName("version_as_string");
        b.Property(OrganizationLatestItem.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
        b.Property(OrganizationLatestItem.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

        b.Ignore(x => x.VersionTimestamp);
        b.Ignore(x => x.CreatedOnTimestamp);

        b.HasIndex(p => p.OvoCode);
        b.HasIndex(p => p.IsMaintainer);
        b.HasIndex(OrganizationLatestItem.VersionTimestampBackingPropertyName);
        b.HasIndex(p => p.IsRemoved);
    }
}
