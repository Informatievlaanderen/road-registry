﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadRegistry.Sync.StreetNameRegistry;

#nullable disable

namespace RoadRegistry.Sync.StreetNameRegistry.Migrations.StreetNameProjection
{
    [DbContext(typeof(StreetNameProjectionContext))]
    partial class StreetNameProjectionContextModelSnapshot : ModelSnapshot
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

                    b.ToTable("ProjectionStates", "RoadRegistryStreetName");
                });

            modelBuilder.Entity("RoadRegistry.Sync.StreetNameRegistry.StreetNameRecord", b =>
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

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("bit");

                    b.Property<string>("NisCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PersistentLocalId")
                        .HasColumnType("int");

                    b.Property<string>("StreetNameStatus")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("StreetNameId");

                    b.HasIndex("PersistentLocalId");

                    b.HasIndex("StreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("StreetNameId"), false);

                    b.HasIndex("StreetNameStatus");

                    b.ToTable("StreetName", "RoadRegistryStreetName");
                });
#pragma warning restore 612, 618
        }
    }
}
