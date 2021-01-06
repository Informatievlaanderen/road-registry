﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RoadRegistry.Editor.Schema;

namespace RoadRegistry.Editor.Schema.Migrations
{
    [DbContext(typeof(EditorContext))]
    partial class EditorContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.ToTable("ProjectionStates", "RoadRegistryEditorMeta");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.GradeSeparatedJunctions.GradeSeparatedJunctionRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<byte[]>("DbaseRecord")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.ToTable("GradeSeparatedJunction", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.MunicipalityGeometry", b =>
                {
                    b.Property<string>("NisCode")
                        .HasMaxLength(5)
                        .HasColumnType("nchar(5)")
                        .IsFixedLength(true);

                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("Geometry");

                    b.HasKey("NisCode")
                        .IsClustered();

                    b.ToTable("MunicipalityGeometry", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.Organizations.OrganizationRecord", b =>
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

                    b.ToTable("Organization", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadNetworkChange", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("When")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsClustered(false);

                    b.ToTable("RoadNetworkChange", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadNetworkChangeRequestBasedOnArchive", b =>
                {
                    b.Property<byte[]>("ChangeRequestId")
                        .HasMaxLength(32)
                        .HasColumnType("varbinary(32)");

                    b.Property<string>("ArchiveId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ChangeRequestId");

                    b.HasIndex("ChangeRequestId")
                        .IsClustered(false);

                    b.ToTable("RoadNetworkChangeRequestBasedOnArchive", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadNetworkInfo", b =>
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

                    b.ToTable("RoadNetworkInfo", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadNodeBoundingBox2D", b =>
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

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadNodes.RoadNodeRecord", b =>
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

                    b.ToTable("RoadNode", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegmentBoundingBox3D", b =>
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

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentEuropeanRoadAttributeRecord", b =>
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

                    b.ToTable("RoadSegmentEuropeanRoadAttribute", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentLaneAttributeRecord", b =>
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

                    b.ToTable("RoadSegmentLaneAttribute", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentNationalRoadAttributeRecord", b =>
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

                    b.ToTable("RoadSegmentNationalRoadAttribute", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentNumberedRoadAttributeRecord", b =>
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

                    b.ToTable("RoadSegmentNumberedRoadAttribute", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentRecord", b =>
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

                    b.ToTable("RoadSegment", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentSurfaceAttributeRecord", b =>
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

                    b.ToTable("RoadSegmentSurfaceAttribute", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentWidthAttributeRecord", b =>
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

                    b.ToTable("RoadSegmentWidthAttribute", "RoadRegistryEditor");
                });

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadNodes.RoadNodeRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Editor.Schema.RoadNodes.RoadNodeBoundingBox", "BoundingBox", b1 =>
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

            modelBuilder.Entity("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Editor.Schema.RoadSegments.RoadSegmentBoundingBox", "BoundingBox", b1 =>
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
