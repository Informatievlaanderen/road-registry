namespace RoadRegistry.BackOffice.Schema
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkActivity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        //public DateTime When { get; set; }
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
            b.Property(p => p.Content);
        }
    }
}
