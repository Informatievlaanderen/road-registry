﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.RoadNetwork.Schema;

#nullable disable

namespace RoadRegistry.RoadNetwork.Schema.Migrations
{
    [DbContext(typeof(RoadNetworkDbContext))]
    [Migration("20231122144138_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.HasSequence<int>("EuropeanRoadAttributeId", "RoadNetwork");

            modelBuilder.HasSequence<int>("GradeSeparatedJunctionId", "RoadNetwork");

            modelBuilder.HasSequence<int>("NationalRoadAttributeId", "RoadNetwork");

            modelBuilder.HasSequence<int>("NumberedRoadAttributeId", "RoadNetwork");

            modelBuilder.HasSequence<int>("RoadNodeId", "RoadNetwork");

            modelBuilder.HasSequence<int>("RoadSegmentId", "RoadNetwork");

            modelBuilder.HasSequence<int>("RoadSegmentLaneAttributeId", "RoadNetwork");

            modelBuilder.HasSequence<int>("RoadSegmentSurfaceAttributeId", "RoadNetwork");

            modelBuilder.HasSequence<int>("RoadSegmentWidthAttributeId", "RoadNetwork");

            modelBuilder.HasSequence<int>("TransactionId", "RoadNetwork");
#pragma warning restore 612, 618
        }
    }
}
