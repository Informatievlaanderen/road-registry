namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentBoundingBox3DConfiguration : IEntityTypeConfiguration<RoadSegmentBoundingBox3D>
    {
        public void Configure(EntityTypeBuilder<RoadSegmentBoundingBox3D> builder)
        {
            builder
                .HasNoKey();
        }
    }
}
