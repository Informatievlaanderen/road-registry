namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class NationalRoadConfiguration : IEntityTypeConfiguration<NationalRoadRecord>
    {
        private const string TableName = "NationalRoad";

        public void Configure(EntityTypeBuilder<NationalRoadRecord> builder)
        {
            builder.ToTable(TableName, WellKnownSchemas.NationalRoadProducerSnapshotSchema)
                .HasKey(p => p.Id)
                .IsClustered();

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired();
            
            builder.Property(p => p.RoadSegmentId);
            builder.Property(p => p.Number);
            
            builder.OwnsOne(p => p.Origin);
            builder.Property(p => p.LastChangedTimestamp);
        }
    }
}
