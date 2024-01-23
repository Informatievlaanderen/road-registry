namespace RoadRegistry.Producer.Snapshot.ProjectionHost.GradeSeparatedJunction
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
    {
        private const string TableName = "GradeSeparatedJunction";

        public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> builder)
        {
            builder.ToTable(TableName, WellKnownSchemas.GradeSeparatedJunctionProducerSnapshotSchema)
                .HasKey(p => p.Id)
                .IsClustered();

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired();
            
            builder.Property(p => p.LowerRoadSegmentId);
            builder.Property(p => p.UpperRoadSegmentId);
            builder.Property(p => p.TypeId);
            builder.Property(p => p.TypeDutchName);

            builder.OwnsOne(p => p.Origin);
            builder.Property(p => p.LastChangedTimestamp);
        }
    }
}
