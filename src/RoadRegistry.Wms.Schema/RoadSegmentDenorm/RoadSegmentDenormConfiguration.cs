namespace RoadRegistry.Wms.Schema.RoadSegmentDenorm
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentDenormConfiguration : IEntityTypeConfiguration<RoadSegmentDenormRecord>
    {
        public const string TableName = "Organization";

        public void Configure(EntityTypeBuilder<RoadSegmentDenormRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.WmsSchema)
                .HasIndex(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedOnAdd().IsRequired();
        }
    }
}
