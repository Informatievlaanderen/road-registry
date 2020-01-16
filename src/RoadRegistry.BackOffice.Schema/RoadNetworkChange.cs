namespace RoadRegistry.BackOffice.Schema
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkChange
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public string When { get; set; }
    }

    public class RoadNetworkChangeConfiguration : IEntityTypeConfiguration<RoadNetworkChange>
    {
        public const string TableName = "RoadNetworkChange";

        public void Configure(EntityTypeBuilder<RoadNetworkChange> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasIndex(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.Title);
            b.Property(p => p.Type);
            b.Property(p => p.Content);
            b.Property(p => p.When);
        }
    }
}
