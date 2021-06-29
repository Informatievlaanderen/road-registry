namespace RoadRegistry.Editor.Schema.RoadNodes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNodeConfiguration : IEntityTypeConfiguration<RoadNodeRecord>
    {
        private const string TableName = "RoadNode";

        public void Configure(EntityTypeBuilder<RoadNodeRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.EditorSchema)
                .HasKey(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
            b.Property(p => p.ShapeRecordContent).IsRequired();
            b.Property(p => p.ShapeRecordContentLength).IsRequired();
            b.Property(p => p.DbaseRecord).IsRequired();
            //b.Property(p => p.Geometry).HasColumnType("Geometry");

            b.OwnsOne(p => p.BoundingBox);
        }

    }
}
