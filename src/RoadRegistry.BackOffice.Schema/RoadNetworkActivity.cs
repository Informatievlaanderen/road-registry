namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkActivity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string RelatesToArchiveId { get; set; }
        public string[] Errors { get; set; }
        public string[] Warnings { get; set; }

    }

    public class RoadNetworkActivityConfiguration : IEntityTypeConfiguration<RoadNetworkActivity>
    {
        public const string TableName = "RoadNetworkActivity";

        public void Configure(EntityTypeBuilder<RoadNetworkActivity> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasIndex(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.Property(p => p.Title);
            b.Property(p => p.Type);
            b.Property(p => p.RelatesToArchiveId);
            b.Property(p => p.Errors)
                .HasConversion(
                    v => v == null ? null : string.Join("|", v),
                    v => v == null ? null : v.Split('|'));
            b.Property(p => p.Warnings)
                .HasConversion(
                    v => v == null ? null : string.Join("|", v),
                    v => v == null ? null : v.Split('|'));
        }
    }
}
