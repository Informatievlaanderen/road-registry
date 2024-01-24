﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentProducerSnapshot
{
    [DbContext(typeof(RoadSegmentProducerSnapshotContext))]
    partial class RoadSegmentProducerSnapshotContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

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

                    b.HasKey("Name");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Name"));

                    b.ToTable("ProjectionStates", "RoadRegistryRoadSegmentProducerSnapshotMeta");
                });

            modelBuilder.Entity("RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment.RoadSegmentRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("AccessRestrictionDutchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AccessRestrictionId")
                        .HasColumnType("int");

                    b.Property<int?>("BeginRoadNodeId")
                        .HasColumnType("int");

                    b.Property<string>("CategoryDutchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CategoryId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("EndRoadNodeId")
                        .HasColumnType("int");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("Geometry");

                    b.Property<int?>("GeometryVersion")
                        .HasColumnType("int");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastChangedTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("LeftSideMunicipalityId")
                        .HasColumnType("int");

                    b.Property<string>("LeftSideMunicipalityNisCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LeftSideStreetName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("LeftSideStreetNameId")
                        .HasColumnType("int");

                    b.Property<string>("MaintainerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MaintainerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MethodDutchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MethodId")
                        .HasColumnType("int");

                    b.Property<string>("MorphologyDutchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MorphologyId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("RecordingDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("RightSideMunicipalityId")
                        .HasColumnType("int");

                    b.Property<string>("RightSideMunicipalityNisCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RightSideStreetName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RightSideStreetNameId")
                        .HasColumnType("int");

                    b.Property<int?>("RoadSegmentVersion")
                        .HasColumnType("int");

                    b.Property<string>("StatusDutchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("StatusId")
                        .HasColumnType("int");

                    b.Property<int?>("TransactionId")
                        .HasColumnType("int");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.HasIndex("LeftSideStreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("LeftSideStreetNameId"), false);

                    b.HasIndex("RightSideStreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("RightSideStreetNameId"), false);

                    b.ToTable("RoadSegment", "RoadRegistryRoadSegmentProducerSnapshot");
                });

            modelBuilder.Entity("RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment.RoadSegmentRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Producer.Snapshot.ProjectionHost.Schema.Origin", "Origin", b1 =>
                        {
                            b1.Property<int>("RoadSegmentRecordId")
                                .HasColumnType("int");

                            b1.Property<string>("Organization")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<DateTimeOffset?>("Timestamp")
                                .HasColumnType("datetimeoffset");

                            b1.HasKey("RoadSegmentRecordId");

                            b1.ToTable("RoadSegment", "RoadRegistryRoadSegmentProducerSnapshot");

                            b1.WithOwner()
                                .HasForeignKey("RoadSegmentRecordId");
                        });

                    b.Navigation("Origin");
                });
#pragma warning restore 612, 618
        }
    }
}
