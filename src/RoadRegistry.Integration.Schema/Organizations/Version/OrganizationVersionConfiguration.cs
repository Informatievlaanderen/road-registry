namespace RoadRegistry.Integration.Schema.Organizations.Version;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OrganizationVersionConfiguration : IEntityTypeConfiguration<OrganizationVersion>
{
    public const string TableName = "organization_versions";

    public void Configure(EntityTypeBuilder<OrganizationVersion> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Position)
            .IsClustered();

        b.Property(p => p.Position).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Code).IsRequired();
        b.Property(p => p.OvoCode).IsRequired(false);
        b.Property(p => p.KboNumber).IsRequired(false);
        b.Property(p => p.IsMaintainer).HasDefaultValue(false).IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(p => p.Position).HasColumnName("position");
        b.Property(p => p.Code).HasColumnName("code");
        b.Property(p => p.Name).HasColumnName("name");
        b.Property(p => p.OvoCode).HasColumnName("ovo_code");
        b.Property(p => p.KboNumber).HasColumnName("kbo_number");
        b.Property(p => p.IsMaintainer).HasColumnName("is_maintainer");
        b.Property(p => p.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.VersionAsString).HasColumnName("version_as_string");
        b.Property(OrganizationVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
        b.Property(OrganizationVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

        b.Ignore(x => x.VersionTimestamp);
        b.Ignore(x => x.CreatedOnTimestamp);

        b.HasIndex(p => p.Code);
        b.HasIndex(p => p.OvoCode);
        b.HasIndex(p => p.IsMaintainer);
        b.HasIndex(OrganizationVersion.VersionTimestampBackingPropertyName);
        b.HasIndex(p => p.IsRemoved);
    }
}
