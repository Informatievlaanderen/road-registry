namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public abstract class RoadNetworkActivity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }

    public class RoadNetworkActivityConfiguration : IEntityTypeConfiguration<RoadNetworkActivity>
    {
        public const string TableName = "RoadNetworkActivity";

        public void Configure(EntityTypeBuilder<RoadNetworkActivity> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasIndex(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.HasDiscriminator<string>(nameof(RoadNetworkActivity.Type));

            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.Property(p => p.Title);
            b.Property(p => p.Type);
        }
    }
}
