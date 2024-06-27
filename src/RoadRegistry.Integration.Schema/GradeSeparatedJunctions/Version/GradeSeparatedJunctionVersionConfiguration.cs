namespace RoadRegistry.Integration.Schema.GradeSeparatedJunctions.Version;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeSeparatedJunctionVersionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionVersion>
{
    private const string TableName = "grade_separated_junction_versions";

    public void Configure(EntityTypeBuilder<GradeSeparatedJunctionVersion> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => new { p.Position, p.Id })
            .IsClustered();

        b.Property(p => p.Position).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.UpperRoadSegmentId).HasColumnName("upper_road_segment_id");
        b.Property(x => x.LowerRoadSegmentId).HasColumnName("lower_road_segment_id");
        b.Property(x => x.TypeId).HasColumnName("type_id");
        b.Property(x => x.TypeLabel).HasColumnName("type_label");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.OrganizationName).HasColumnName("organization_name");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.VersionTimestamp).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnTimestamp).HasColumnName("created_on_timestamp");

        b.HasIndex(p => p.Id);
        b.HasIndex(p => p.UpperRoadSegmentId);
        b.HasIndex(p => p.LowerRoadSegmentId);
        b.HasIndex(p => p.TypeId);
        b.HasIndex(p => p.TypeLabel);
        b.HasIndex(p => p.OrganizationId);
        b.HasIndex(p => p.OrganizationName);
        b.HasIndex(p => p.VersionTimestamp);
        b.HasIndex(p => p.IsRemoved);
    }
}
