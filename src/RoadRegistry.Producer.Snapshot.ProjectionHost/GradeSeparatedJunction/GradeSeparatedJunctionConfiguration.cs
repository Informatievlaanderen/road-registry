namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction
{
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
    {
        private const string TableName = "GradeSeparatedJunction";

        public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> builder)
        {
            builder.ToTable(TableName, WellknownSchemas.GradeSeparatedJunctionProducerSnapshotSchema)
                .HasKey(p => p.Id)
                .IsClustered();

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired();
            
            builder.Property(p => p.LowerRoadSegmentId);
            builder.Property(p => p.UpperRoadSegmentId);
            builder.Property(p => p.Type);
            
            builder.OwnsOne(p => p.Origin);
            builder.Property(p => p.LastChangedTimestamp);
        }
    }
}
