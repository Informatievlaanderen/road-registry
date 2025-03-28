﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RoadRegistry.Sync.MunicipalityRegistry;

#nullable disable

namespace RoadRegistry.Sync.MunicipalityRegistry.Migrations
{
    [DbContext(typeof(MunicipalityEventConsumerContext))]
    [Migration("20241205115203_AddOffsetOverride")]
    partial class AddOffsetOverride
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.OffsetOverride", b =>
                {
                    b.Property<string>("ConsumerGroupId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Configured")
                        .HasColumnType("bit");

                    b.Property<long>("Offset")
                        .HasColumnType("bigint");

                    b.HasKey("ConsumerGroupId");

                    b.ToTable("OffsetOverrides", "RoadRegistryMunicipalityEventConsumer");
                });

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.ProcessedMessage", b =>
                {
                    b.Property<string>("IdempotenceKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset>("DateProcessed")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("IdempotenceKey");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("IdempotenceKey"));

                    b.HasIndex("DateProcessed");

                    b.ToTable("ProcessedMessages", "RoadRegistryMunicipalityEventConsumer");
                });

            modelBuilder.Entity("RoadRegistry.Sync.MunicipalityRegistry.Models.Municipality", b =>
                {
                    b.Property<string>("MunicipalityId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("Geometry");

                    b.Property<string>("NisCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("MunicipalityId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("MunicipalityId"), false);

                    b.HasIndex("NisCode");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("NisCode"));

                    b.ToTable("Municipalities", "RoadRegistryMunicipalityEventConsumer");
                });
#pragma warning restore 612, 618
        }
    }
}
