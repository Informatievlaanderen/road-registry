using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadRegistry.Infrastructure;

namespace RoadRegistry.Projections
{
    public class RoadNodeRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public class RoadNodeConfiguration : IEntityTypeConfiguration<RoadNodeRecord>
    {
        private const string TableName = "RoadNode";

        public void Configure(EntityTypeBuilder<RoadNodeRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasKey(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.ShapeRecordContent);
            b.Property(p => p.DbaseRecord);
        }
    }
}
