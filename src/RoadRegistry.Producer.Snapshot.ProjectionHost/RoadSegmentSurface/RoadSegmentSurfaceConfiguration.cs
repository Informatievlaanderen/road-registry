namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentSurfaceConfiguration : IEntityTypeConfiguration<RoadSegmentSurfaceRecord>
    {
        private const string TableName = "RoadSegmentSurface";

        public void Configure(EntityTypeBuilder<RoadSegmentSurfaceRecord> builder)
        {
            builder.ToTable(TableName, WellKnownSchemas.RoadSegmentSurfaceProducerSnapshotSchema)
                .HasKey(p => p.Id)
                .IsClustered();

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(p => p.RoadSegmentId);
            builder.Property(p => p.RoadSegmentGeometryVersion);
            builder.Property(p => p.TypeId);
            builder.Property(p => p.TypeDutchName);
            builder.Property(p => p.FromPosition);
            builder.Property(p => p.ToPosition);
            
            builder.OwnsOne(p => p.Origin);
            builder.Property(p => p.LastChangedTimestamp);
        }
    }
}
