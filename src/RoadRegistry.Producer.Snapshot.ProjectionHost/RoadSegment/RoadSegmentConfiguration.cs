namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
    {
        private const string TableName = "RoadSegment";

        public void Configure(EntityTypeBuilder<RoadSegmentRecord> builder)
        {
            builder.ToTable(TableName, WellKnownSchemas.RoadSegmentProducerSnapshotSchema)
                .HasKey(p => p.Id)
                .IsClustered();

            builder.Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(p => p.Version);

            builder.Property(p => p.RoadSegmentVersion);
            builder.Property(p => p.Geometry).HasColumnType("Geometry");
            builder.Property(p => p.GeometryVersion);
            builder.Property(p => p.BeginRoadNodeId);
            builder.Property(p => p.EndRoadNodeId);

            builder.Property(p => p.TransactionId);

            builder.Property(p => p.AccessRestrictionId);
            builder.Property(p => p.AccessRestrictionDutchName);
            builder.Property(p => p.MorphologyId);
            builder.Property(p => p.MorphologyDutchName);
            builder.Property(p => p.StatusId);
            builder.Property(p => p.StatusDutchName);
            builder.Property(p => p.CategoryId);
            builder.Property(p => p.CategoryDutchName);
            builder.Property(p => p.MethodId);
            builder.Property(p => p.MethodDutchName);

            builder.Property(p => p.MaintainerId);
            builder.Property(p => p.MaintainerName);
            
            builder.Property(p => p.LeftSideStreetNameId);
            builder.Property(p => p.RightSideStreetNameId);
            builder.Property(p => p.LeftSideStreetName);
            builder.Property(p => p.RightSideStreetName);
            builder.Property(p => p.LeftSideMunicipalityId);
            builder.Property(p => p.RightSideMunicipalityId);

            builder.Property(p => p.RecordingDate);

            builder.Property(p => p.LeftSideMunicipalityNisCode);
            builder.Property(p => p.RightSideMunicipalityNisCode);
            
            builder.OwnsOne(p => p.Origin);
            builder.Property(p => p.LastChangedTimestamp);

            // The projection renames an organization and relabels a street name by looking every road segment of that
            // organization or street name up by these columns and paging through the result, so each one carries an
            // index. Without one the paging turns into a table scan per page, which is what the other road segment
            // projections already avoid this way.
            builder.HasIndex(p => p.MaintainerId)
                .IsClustered(false);
            builder.HasIndex(p => p.LeftSideStreetNameId)
                .IsClustered(false);
            builder.HasIndex(p => p.RightSideStreetNameId)
                .IsClustered(false);
        }
    }
}
