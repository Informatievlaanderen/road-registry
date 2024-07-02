namespace RoadRegistry.Integration.Schema.GradeSeparatedJunctions;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeSeparatedJunctionLatestItemConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionLatestItem>
{
    private const string TableName = "grade_separated_junction_latest_items";

    public void Configure(EntityTypeBuilder<GradeSeparatedJunctionLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

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
        b.Property(x => x.VersionAsString).HasColumnName("version_as_string");
        b.Property(GradeSeparatedJunctionLatestItem.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
        b.Property(GradeSeparatedJunctionLatestItem.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

        b.Ignore(x => x.VersionTimestamp);
        b.Ignore(x => x.CreatedOnTimestamp);

        b.HasIndex(p => p.UpperRoadSegmentId);
        b.HasIndex(p => p.LowerRoadSegmentId);
        b.HasIndex(p => p.TypeId);
        b.HasIndex(p => p.TypeLabel);
        b.HasIndex(p => p.OrganizationId);
        b.HasIndex(p => p.OrganizationName);
        b.HasIndex(GradeSeparatedJunctionLatestItem.VersionTimestampBackingPropertyName);
        b.HasIndex(p => p.IsRemoved);
    }
}
