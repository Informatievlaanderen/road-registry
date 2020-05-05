namespace RoadRegistry.Product.Schema.GradeSeparatedJunctions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
    {
        private const string TableName = "GradeSeparatedJunction";

        public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.ProductSchema)
                .HasKey(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
            b.Property(p => p.DbaseRecord).IsRequired();
        }
    }
}
