namespace RoadRegistry.Extracts.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InwinningRoadSegment
{
    public required int RoadSegmentId { get; set; }
    public required bool Completed { get; set; }
}

public class InwinningRoadSegmentConfiguration : IEntityTypeConfiguration<InwinningRoadSegment>
{
    private const string TableName = "InwinningRoadSegments";

    public void Configure(EntityTypeBuilder<InwinningRoadSegment> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.RoadSegmentId)
            .IsClustered();

        b.Property(p => p.RoadSegmentId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.Completed).IsRequired();
    }
}
