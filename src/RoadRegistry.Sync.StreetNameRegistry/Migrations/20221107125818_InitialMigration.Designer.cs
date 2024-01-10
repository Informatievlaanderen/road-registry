﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.StreetNameConsumer.Schema;

#nullable disable

namespace RoadRegistry.StreetNameConsumer.Schema.Migrations
{
    [DbContext(typeof(StreetNameConsumerContext))]
    [Migration("20221107125818_InitialMigration")]
    partial class InitialMigration
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

                    b.ToTable("ProjectionStates", "RoadRegistryStreetNameConsumer");
                });

            modelBuilder.Entity("RoadRegistry.StreetNameConsumer.Schema.StreetNameConsumerItem", b =>
                {
                    b.Property<string>("StreetNameId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DutchHomonymAddition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DutchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DutchNameWithHomonymAddition")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)")
                        .HasComputedColumnSql("COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED");

                    b.Property<string>("EnglishHomonymAddition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishNameWithHomonymAddition")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)")
                        .HasComputedColumnSql("COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED");

                    b.Property<string>("FrenchHomonymAddition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FrenchName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FrenchNameWithHomonymAddition")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)")
                        .HasComputedColumnSql("COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED");

                    b.Property<string>("GermanHomonymAddition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GermanName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GermanNameWithHomonymAddition")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)")
                        .HasComputedColumnSql("COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED");

                    b.Property<string>("HomonymAddition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MunicipalityId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NameWithHomonymAddition")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("nvarchar(max)")
                        .HasComputedColumnSql("COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED");

                    b.Property<string>("NisCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<int?>("StreetNameStatus")
                        .HasColumnType("int");

                    b.HasKey("StreetNameId");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("StreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("StreetNameId"), false);

                    b.ToTable("StreetName", "RoadRegistryStreetNameConsumer");
                });
#pragma warning restore 612, 618
        }
    }
}