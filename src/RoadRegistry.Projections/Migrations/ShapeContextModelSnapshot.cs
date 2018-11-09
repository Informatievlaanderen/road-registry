﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RoadRegistry.Projections.Migrations
{
    [DbContext(typeof(ShapeContext))]
    partial class ShapeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Aiv.Vbr.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Position");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","RoadRegistryProjectionMetaData");
                });

            modelBuilder.Entity("RoadRegistry.Projections.GradeSeparatedJunctionRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("GradeSeparatedJunction","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.OrganizationRecord", b =>
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

            modelBuilder.Entity("RoadRegistry.Projections.RoadNetworkInfo", b =>
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

                    b.Property<int>("ReferencePointCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadNodeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentDynamicHardeningAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentDynamicLaneAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentDynamicWidthAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentEuropeanRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentNationalRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentNumberedRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(0);

                    b.Property<int>("TotalReferencePointShapeLength");

                    b.Property<int>("TotalRoadNodeShapeLength");

                    b.Property<int>("TotalRoadSegmentShapeLength");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadNetworkInfo","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadNodeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<byte[]>("ShapeRecordContent");

                    b.Property<int>("ShapeRecordContentLength");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadNode","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadReferencePointRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<byte[]>("ShapeRecordContent");

                    b.Property<int>("ShapeRecordContentLength");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadReferencePoint","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentSurfaceAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentSurfaceAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentLaneAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentLaneAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentWidthAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentWidthAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentEuropeanRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentEuropeanRoadAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentNationalRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentNationalRoadAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentNumberedRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<int>("RoadSegmentId");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegmentNumberedRoadAttribute","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentRecord", b =>
                {
                    b.Property<int>("Id");

                    b.Property<byte[]>("DbaseRecord");

                    b.Property<byte[]>("ShapeRecordContent");

                    b.Property<int>("ShapeRecordContentLength");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("RoadSegment","RoadRegistryShape");
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadNodeRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Projections.BoundingBox2D", "Envelope", b1 =>
                        {
                            b1.Property<int>("RoadNodeRecordId");

                            b1.Property<double>("MaximumX");

                            b1.Property<double>("MaximumY");

                            b1.Property<double>("MinimumX");

                            b1.Property<double>("MinimumY");

                            b1.ToTable("RoadNode","RoadRegistryShape");

                            b1.HasOne("RoadRegistry.Projections.RoadNodeRecord")
                                .WithOne("Envelope")
                                .HasForeignKey("RoadRegistry.Projections.BoundingBox2D", "RoadNodeRecordId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadReferencePointRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Projections.BoundingBox2D", "Envelope", b1 =>
                        {
                            b1.Property<int>("RoadReferencePointRecordId");

                            b1.Property<double>("MaximumX");

                            b1.Property<double>("MaximumY");

                            b1.Property<double>("MinimumX");

                            b1.Property<double>("MinimumY");

                            b1.ToTable("RoadReferencePoint","RoadRegistryShape");

                            b1.HasOne("RoadRegistry.Projections.RoadReferencePointRecord")
                                .WithOne("Envelope")
                                .HasForeignKey("RoadRegistry.Projections.BoundingBox2D", "RoadReferencePointRecordId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("RoadRegistry.Projections.RoadSegmentRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Projections.BoundingBox2D", "Envelope", b1 =>
                        {
                            b1.Property<int>("RoadSegmentRecordId");

                            b1.Property<double>("MaximumX");

                            b1.Property<double>("MaximumY");

                            b1.Property<double>("MinimumX");

                            b1.Property<double>("MinimumY");

                            b1.ToTable("RoadSegment","RoadRegistryShape");

                            b1.HasOne("RoadRegistry.Projections.RoadSegmentRecord")
                                .WithOne("Envelope")
                                .HasForeignKey("RoadRegistry.Projections.BoundingBox2D", "RoadSegmentRecordId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
