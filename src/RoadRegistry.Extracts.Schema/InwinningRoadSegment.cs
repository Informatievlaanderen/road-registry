namespace RoadRegistry.Extracts.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InwinningRoadSegment
{
    public int Id { get; private set; }

    public required int RoadSegmentId { get; set; }
    public string? NisCode { get; set; }
    public required bool Completed { get; set; }
}

public class InwinningRoadSegmentConfiguration : IEntityTypeConfiguration<InwinningRoadSegment>
{
    private const string TableName = "InwinningRoadSegments";

    public void Configure(EntityTypeBuilder<InwinningRoadSegment> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.NisCode).HasMaxLength(5).IsRequired(false);
        b.Property(p => p.Completed).IsRequired();

        b.HasIndex(x => x.RoadSegmentId);
        b.HasIndex(x => x.NisCode);
    }
}
