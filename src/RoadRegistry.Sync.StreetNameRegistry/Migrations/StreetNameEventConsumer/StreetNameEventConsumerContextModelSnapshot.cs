﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.Sync.StreetNameRegistry;

#nullable disable

namespace RoadRegistry.Sync.StreetNameRegistry.Migrations.StreetNameEventConsumer
{
    [DbContext(typeof(StreetNameEventConsumerContext))]
    partial class StreetNameEventConsumerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.ToTable("OffsetOverrides", "RoadRegistryStreetNameEventConsumer");
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

                    b.ToTable("ProcessedMessages", "RoadRegistryStreetNameEventConsumer");
                });
#pragma warning restore 612, 618
        }
    }
}
