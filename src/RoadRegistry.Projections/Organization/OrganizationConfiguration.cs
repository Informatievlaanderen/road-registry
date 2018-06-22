namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class OrganizationConfiguration : IEntityTypeConfiguration<OrganizationRecord>
    {
        public const string TableName = "Organization";

        public void Configure(EntityTypeBuilder<OrganizationRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasIndex(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.Property(p => p.Code);
            b.Property(p => p.DbaseRecord);
        }
    }
}
