// namespace RoadRegistry.Wfs.Schema
// {
//     using Hosts;
//     using Microsoft.EntityFrameworkCore;
//     using Microsoft.EntityFrameworkCore.Metadata.Builders;
//
//     public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
//     {
//         public const string TableName = "OngelijkgrondseKruising";
//
//         public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> b)
//         {
//             b.ToTable(TableName, WellknownSchemas.WfsSchema)
//                 .HasKey(p => p.Id)
//                 .IsClustered(false);
//
//             b.Property(p => p.Id)
//                 .ValueGeneratedNever()
//                 .IsRequired()
//                 .HasColumnName("objectId");
//
//             b.Property(p => p.BeginTime)
//                 .HasColumnName("versieId")
//                 .HasColumnType("varchar(100)");
//
//             b.Property(p => p.Type)
//                 .HasColumnName("type")
//                 .HasColumnType("varchar(255)");
//
//             b.Property(p => p.LowerRoadSegmentId)
//                 .HasColumnName("onderliggendWegsegment");
//
//             b.Property(p => p.UpperRoadSegmentId)
//                 .HasColumnName("bovenliggendWegsegment");
//
//             b.Property(p => p.IntersectGeometry)
//                 .HasColumnName("puntGeometrie")
//                 .HasColumnType("Geometry");
//         }
//     }
// }

