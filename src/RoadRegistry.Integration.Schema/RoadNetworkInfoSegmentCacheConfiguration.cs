namespace RoadRegistry.Integration.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNetworkInfoSegmentCacheConfiguration : IEntityTypeConfiguration<RoadNetworkInfoSegmentCache>
{
    public const string TableName = "RoadNetworkInfoSegmentCache";

    public void Configure(EntityTypeBuilder<RoadNetworkInfoSegmentCache> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasIndex(p => p.RoadSegmentId)
            .IsClustered(false);

        b.HasKey(p => p.RoadSegmentId);
        b.Property(p => p.RoadSegmentId).ValueGeneratedNever().IsRequired();

        b.Property(p => p.SurfacesLength);
        b.Property(p => p.LanesLength);
        b.Property(p => p.ShapeLength);
        b.Property(p => p.WidthsLength);
        b.Property(p => p.PartOfEuropeanRoadsLength);
        b.Property(p => p.PartOfNationalRoadsLength);
        b.Property(p => p.PartOfNumberedRoadsLength);
    }
}
