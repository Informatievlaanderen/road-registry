namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadNode
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNodeConfiguration : IEntityTypeConfiguration<RoadNodeRecord>
    {
        private const string TableName = "RoadNode";

        public void Configure(EntityTypeBuilder<RoadNodeRecord> builder)
        {
            builder.ToTable(TableName, WellknownSchemas.RoadNodeProducerSnapshotSchema)
                .HasKey(p => p.Id)
                .IsClustered();

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder
                .Property(p => p.Geometry)
                .HasColumnType("Geometry")
                .IsRequired();
            builder.Property(p => p.TypeId);
            builder.Property(p => p.TypeDutchName);

            builder.OwnsOne(p => p.Origin);
            builder.Property(p => p.LastChangedTimestamp);
        }
    }
}
