﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.Sync.StreetNameRegistry;

#nullable disable

namespace RoadRegistry.Sync.StreetNameRegistry.Migrations
{
    [DbContext(typeof(StreetNameConsumerContext))]
    partial class StreetNameConsumerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

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

                    b.ToTable("ProcessedMessages", "RoadRegistryStreetNameConsumer");
                });
#pragma warning restore 612, 618
        }
    }
}
