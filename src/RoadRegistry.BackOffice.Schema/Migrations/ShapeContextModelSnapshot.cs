﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.BackOffice.Schema;

namespace RoadRegistry.BackOffice.Schema.Migrations
{
    [DbContext(typeof(ShapeContext))]
    partial class ShapeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DesiredState");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt");

                    b.Property<long>("Position");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","RoadRegistryProjectionMetaData");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.GradeSeparatedJunctions.GradeSeparatedJunctionRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("GradeSeparatedJunction","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.Organizations.OrganizationRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<string>("SortableCode");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("Organization","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadNetworkChange", b =>
                {
                    b.Property<long>("Id");

                    b.Property<string>("Content");

                    b.Property<string>("Title");

                    b.Property<string>("Type");

                    b.Property<string>("When");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadNetworkChange","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadNetworkInfo", b =>
                {
                    b.Property<int>("Id")
                        .HasDefaultValue(0);

                    b.Property<bool>("CompletedImport")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<int>("GradeSeparatedJunctionCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("OrganizationCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadNodeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentEuropeanRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentLaneAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentNationalRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentNumberedRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentSurfaceAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentWidthAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("TotalRoadNodeShapeLength");

                    b.Property<int>("TotalRoadSegmentShapeLength");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadNetworkInfo","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadNodes.RoadNodeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<byte[]>("ShapeRecordContent");

                    b.Property<int>("ShapeRecordContentLength");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadNode","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegmentEuropeanRoadAttributes.RoadSegmentEuropeanRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentEuropeanRoadAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegmentLaneAttributes.RoadSegmentLaneAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentLaneAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegmentNationalRoadAttributes.RoadSegmentNationalRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentNationalRoadAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegmentNumberedRoadAttributes.RoadSegmentNumberedRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentNumberedRoadAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegmentSurfaceAttributes.RoadSegmentSurfaceAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentSurfaceAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegmentWidthAttributes.RoadSegmentWidthAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentWidthAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegments.RoadSegmentRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<byte[]>("ShapeRecordContent");

                    b.Property<int>("ShapeRecordContentLength");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegment","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadNodes.RoadNodeRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.BackOffice.Schema.RoadNodes.RoadNodeBoundingBox", "BoundingBox", b1 =>
                        {
                            b1.Property<int>("RoadNodeRecordId");

                            b1.Property<double>("MaximumX");

                            b1.Property<double>("MaximumY");

                            b1.Property<double>("MinimumX");

                            b1.Property<double>("MinimumY");

                            b1.HasKey("RoadNodeRecordId");

                            b1.ToTable("RoadNode","RoadRegistryShape");

                            b1.HasOne("RoadRegistry.BackOffice.Schema.RoadNodes.RoadNodeRecord")
                                .WithOne("BoundingBox")
                                .HasForeignKey("RoadRegistry.BackOffice.Schema.RoadNodes.RoadNodeBoundingBox", "RoadNodeRecordId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("RoadRegistry.BackOffice.Schema.RoadSegments.RoadSegmentRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.BackOffice.Schema.RoadSegments.RoadSegmentBoundingBox", "BoundingBox", b1 =>
                        {
                            b1.Property<int>("RoadSegmentRecordId");

                            b1.Property<double>("MaximumM");

                            b1.Property<double>("MaximumX");

                            b1.Property<double>("MaximumY");

                            b1.Property<double>("MinimumM");

                            b1.Property<double>("MinimumX");

                            b1.Property<double>("MinimumY");

                            b1.HasKey("RoadSegmentRecordId");

                            b1.ToTable("RoadSegment","RoadRegistryShape");

                            b1.HasOne("RoadRegistry.BackOffice.Schema.RoadSegments.RoadSegmentRecord")
                                .WithOne("BoundingBox")
                                .HasForeignKey("RoadRegistry.BackOffice.Schema.RoadSegments.RoadSegmentBoundingBox", "RoadSegmentRecordId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
