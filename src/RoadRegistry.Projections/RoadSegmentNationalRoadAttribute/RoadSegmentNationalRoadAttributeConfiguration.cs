namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentNationalRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNationalRoadAttributeRecord>
    {
        private const string TableName = "RoadSegmentNationalRoadAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentNationalRoadAttributeRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasKey(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.RoadSegmentId);
            b.Property(p => p.DbaseRecord);
        }
    }
}
