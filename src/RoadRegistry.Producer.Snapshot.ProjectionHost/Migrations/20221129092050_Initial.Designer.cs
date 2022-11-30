﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RoadRegistry.Producer.Snapshot.ProjectionHost.Schema;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations
{
    [DbContext(typeof(ProducerSnapshotContext))]
    [Migration("20221129092050_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.ToTable("ProjectionStates", "RoadRegistryProducerSnapshotMeta");
                });

            modelBuilder.Entity("RoadRegistry.Producer.Snapshot.ProjectionHost.Schema.RoadNodeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<Geometry>("Geometry")
                        .IsRequired()
                        .HasColumnType("Geometry");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastChangedTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.ToTable("RoadNode", "RoadRegistryProducerSnapshot");
                });

            modelBuilder.Entity("RoadRegistry.Producer.Snapshot.ProjectionHost.Schema.RoadNodeRecord", b =>
                {
                    b.OwnsOne("RoadRegistry.Producer.Snapshot.ProjectionHost.Schema.Origin", "Origin", b1 =>
                        {
                            b1.Property<int>("RoadNodeRecordId")
                                .HasColumnType("int");

                            b1.Property<DateTimeOffset?>("BeginTime")
                                .HasColumnType("datetimeoffset");

                            b1.Property<string>("Organization")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("RoadNodeRecordId");

                            b1.ToTable("RoadNode", "RoadRegistryProducerSnapshot");

                            b1.WithOwner()
                                .HasForeignKey("RoadNodeRecordId");
                        });

                    b.Navigation("Origin");
                });
#pragma warning restore 612, 618
        }
    }
}
