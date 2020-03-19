namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNodeBoundingBox2DConfiguration : IEntityTypeConfiguration<RoadNodeBoundingBox2D>
    {
        public void Configure(EntityTypeBuilder<RoadNodeBoundingBox2D> builder)
        {
            builder
                .HasNoKey();
        }
    }
}
