namespace RoadRegistry.Wms.Schema.RoadSegmentDenorm
{
    using System.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    // public class RoadSegmentDenormConfiguration : IEntityTypeConfiguration<RoadSegmentDenormRecord>
    // {
    //     public const string TableName = "wegsegmentDenorm";
    //
    //     public void Configure(EntityTypeBuilder<RoadSegmentDenormRecord> b)
    //     {
    //         b.ToTable(TableName, "dbo")
    //             .HasIndex(p => p.Id)
    //             .IsClustered(false);
    //
    //         b.Property(p => p.Id).ValueGeneratedOnAdd().IsRequired();
    //         b.Property(p => p.GeometryAsByte).HasColumnName("geometrie");
    //     }
    // }

    public class RoadSegmentDenormTestConfiguration : IEntityTypeConfiguration<RoadSegmentDenormTestRecord>
    {
        public const string TableName = "wegsegmentDenorm";

        public void Configure(EntityTypeBuilder<RoadSegmentDenormTestRecord> b)
        {
            b.ToTable(TableName, "dbo")
                .HasIndex(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).HasColumnName("wegsegmentID").ValueGeneratedOnAdd().IsRequired();
            b.Property(p => p.Geometrie).HasColumnName("geometrie");
        }
    }
}
