﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.Product.Schema;

namespace RoadRegistry.Product.Schema.Migrations
{
    [DbContext(typeof(ProductContext))]
    [Migration("20210106145031_AddProjectionStatesErrorMessage")]
    partial class AddProjectionStatesErrorMessage
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name")
                        .IsClustered();

                    b.ToTable("ProjectionStates", "RoadRegistryProductMeta");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.GradeSeparatedJunctions.GradeSeparatedJunctionRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.ToTable("GradeSeparatedJunction", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.Organizations.OrganizationRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("SortableCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsClustered(false);

                    b.ToTable("Organization", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadNetworkInfo", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<bool>("CompletedImport")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<int>("GradeSeparatedJunctionCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("OrganizationCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadNodeCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentEuropeanRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentLaneAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentNationalRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentNumberedRoadAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentSurfaceAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("RoadSegmentWidthAttributeCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("TotalRoadNodeShapeLength")
                        .HasColumnType("int");

                    b.Property<int>("TotalRoadSegmentShapeLength")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsClustered(false);

                    b.ToTable("RoadNetworkInfo", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadNodeBoundingBox2D", b =>
                {
                    b.Property<double>("MaximumX")
                        .HasColumnType("float");

                    b.Property<double>("MaximumY")
                        .HasColumnType("float");

                    b.Property<double>("MinimumX")
                        .HasColumnType("float");

                    b.Property<double>("MinimumY")
                        .HasColumnType("float");

                    b.ToTable("RoadNodeBoundingBox");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadNodes.RoadNodeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("ShapeRecordContent")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.ToTable("RoadNode", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegmentBoundingBox3D", b =>
                {
                    b.Property<double>("MaximumM")
                        .HasColumnType("float");

                    b.Property<double>("MaximumX")
                        .HasColumnType("float");

                    b.Property<double>("MaximumY")
                        .HasColumnType("float");

                    b.Property<double>("MinimumM")
                        .HasColumnType("float");

                    b.Property<double>("MinimumX")
                        .HasColumnType("float");

                    b.Property<double>("MinimumY")
                        .HasColumnType("float");

                    b.ToTable("RoadSegmentBoundingBox");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentEuropeanRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("RoadSegmentId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("RoadSegmentId");

                    b.ToTable("RoadSegmentEuropeanRoadAttribute", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentLaneAttributeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("RoadSegmentId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("RoadSegmentId");

                    b.ToTable("RoadSegmentLaneAttribute", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentNationalRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("RoadSegmentId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("RoadSegmentId");

                    b.ToTable("RoadSegmentNationalRoadAttribute", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentNumberedRoadAttributeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("RoadSegmentId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("RoadSegmentId");

                    b.ToTable("RoadSegmentNumberedRoadAttribute", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("ShapeRecordContent")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("ShapeRecordContentLength")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.ToTable("RoadSegment", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentSurfaceAttributeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("RoadSegmentId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("RoadSegmentId");

                    b.ToTable("RoadSegmentSurfaceAttribute", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentWidthAttributeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("RoadSegmentId")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("RoadSegmentId");

                    b.ToTable("RoadSegmentWidthAttribute", "RoadRegistryProduct");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadNodes.RoadNodeRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Product.Schema.RoadNodes.RoadNodeBoundingBox", "BoundingBox", b1 =>
                        {
                            b1.Property<int>("RoadNodeRecordId")
                                .HasColumnType("int");

                            b1.Property<double>("MaximumX")
                                .HasColumnType("float");

                            b1.Property<double>("MaximumY")
                                .HasColumnType("float");

                            b1.Property<double>("MinimumX")
                                .HasColumnType("float");

                            b1.Property<double>("MinimumY")
                                .HasColumnType("float");

                            b1.HasKey("RoadNodeRecordId");

                            b1.ToTable("RoadNode");

                            b1.WithOwner()
                                .HasForeignKey("RoadNodeRecordId");
                        });

                    b.Navigation("BoundingBox");
                });

            modelBuilder.Entity("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Product.Schema.RoadSegments.RoadSegmentBoundingBox", "BoundingBox", b1 =>
                        {
                            b1.Property<int>("RoadSegmentRecordId")
                                .HasColumnType("int");

                            b1.Property<double>("MaximumM")
                                .HasColumnType("float");

                            b1.Property<double>("MaximumX")
                                .HasColumnType("float");

                            b1.Property<double>("MaximumY")
                                .HasColumnType("float");

                            b1.Property<double>("MinimumM")
                                .HasColumnType("float");

                            b1.Property<double>("MinimumX")
                                .HasColumnType("float");

                            b1.Property<double>("MinimumY")
                                .HasColumnType("float");

                            b1.HasKey("RoadSegmentRecordId");

                            b1.ToTable("RoadSegment");

                            b1.WithOwner()
                                .HasForeignKey("RoadSegmentRecordId");
                        });

                    b.Navigation("BoundingBox");
                });
#pragma warning restore 612, 618
        }
    }
}
