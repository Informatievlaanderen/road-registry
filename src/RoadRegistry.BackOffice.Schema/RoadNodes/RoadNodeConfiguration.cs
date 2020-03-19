namespace RoadRegistry.BackOffice.Schema.RoadNodes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNodeConfiguration : IEntityTypeConfiguration<RoadNodeRecord>
    {
        private const string TableName = "RoadNode";

        public void Configure(EntityTypeBuilder<RoadNodeRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.BackOfficeSchema)
                .HasKey(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.ShapeRecordContent);
            b.Property(p => p.ShapeRecordContentLength);
            b.Property(p => p.DbaseRecord);

            b.OwnsOne(p => p.BoundingBox);
        }

    }
}
