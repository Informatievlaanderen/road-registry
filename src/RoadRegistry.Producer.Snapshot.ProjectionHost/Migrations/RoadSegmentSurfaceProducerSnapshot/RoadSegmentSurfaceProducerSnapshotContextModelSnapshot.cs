﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations
{
    [DbContext(typeof(RoadSegmentSurfaceProducerSnapshotContext))]
    partial class RoadSegmentSurfaceProducerSnapshotContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
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

                    b.ToTable("ProjectionStates", "RoadRegistryRoadSegmentSurfaceProducerSnapshotMetaSchema");
                });

            modelBuilder.Entity("RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface.RoadSegmentSurfaceRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<double>("FromPosition")
                        .HasColumnType("float");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastChangedTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("RoadSegmentGeometryVersion")
                        .HasColumnType("int");

                    b.Property<int>("RoadSegmentId")
                        .HasColumnType("int");

                    b.Property<double>("ToPosition")
                        .HasColumnType("float");

                    b.Property<string>("TypeDutchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.ToTable("RoadSegmentSurface", "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema");
                });

            modelBuilder.Entity("RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegmentSurface.RoadSegmentSurfaceRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Producer.Snapshot.ProjectionHost.Schema.Origin", "Origin", b1 =>
                        {
                            b1.Property<int>("RoadSegmentSurfaceRecordId")
                                .HasColumnType("int");

                            b1.Property<DateTimeOffset?>("BeginTime")
                                .HasColumnType("datetimeoffset");

                            b1.Property<string>("OrganizationId")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("OrganizationName")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("RoadSegmentSurfaceRecordId");

                            b1.ToTable("RoadSegmentSurface", "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema");

                            b1.WithOwner()
                                .HasForeignKey("RoadSegmentSurfaceRecordId");
                        });

                    b.Navigation("Origin");
                });
#pragma warning restore 612, 618
        }
    }
}
