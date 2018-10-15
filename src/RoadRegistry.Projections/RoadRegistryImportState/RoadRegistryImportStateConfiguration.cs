namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadRegistryStateConfiguration : IEntityTypeConfiguration<RoadRegistryImportStateRecord>
    {
        public const string TableName = "RoadRegistryState";

        public void Configure(EntityTypeBuilder<RoadRegistryImportStateRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasIndex(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.Property(p => p.ImportComplete).HasDefaultValue(false);
        }
    }
}
