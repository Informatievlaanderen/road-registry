namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
    {
        private const string TableName = "GradeSeparatedJunction";

        public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasKey(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.DbaseRecord);
        }
    }
}
