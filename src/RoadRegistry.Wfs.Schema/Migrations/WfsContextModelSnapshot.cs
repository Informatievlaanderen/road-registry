﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RoadRegistry.Wfs.Schema;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    [DbContext(typeof(WfsContext))]
    partial class WfsContextModelSnapshot : ModelSnapshot
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

                    b.ToTable("ProjectionStates", "RoadRegistryWfsMeta");
                });

            modelBuilder.Entity("RoadRegistry.Wfs.Schema.RoadNodeRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("objectId");

                    b.Property<string>("BeginTime")
                        .HasColumnType("varchar(100)")
                        .HasColumnName("versieId");

                    b.Property<Geometry>("Geometry")
                        .HasColumnType("Geometry")
                        .HasColumnName("puntGeometrie");

                    b.Property<string>("Type")
                        .HasColumnType("varchar(255)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.ToTable("Wegknoop", "RoadRegistryWfs");
                });

            modelBuilder.Entity("RoadRegistry.Wfs.Schema.RoadSegmentRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("objectId");

                    b.Property<string>("AccessRestriction")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("toegangsbeperking");

                    b.Property<int?>("BeginRoadNodeId")
                        .HasColumnType("int")
                        .HasColumnName("beginknoopObjectId");

                    b.Property<string>("BeginTime")
                        .HasColumnType("varchar(100)")
                        .HasColumnName("versieId");

                    b.Property<string>("CategoryDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("wegcategorie");

                    b.Property<int?>("EndRoadNodeId")
                        .HasColumnType("int")
                        .HasColumnName("eindknoopObjectId");

                    b.Property<Geometry>("Geometry2D")
                        .HasColumnType("Geometry")
                        .HasColumnName("middellijnGeometrie");

                    b.Property<bool>("IsRemoved")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false)
                        .HasColumnName("verwijderd");

                    b.Property<string>("LeftSideStreetName")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("linkerstraatnaam");

                    b.Property<int?>("LeftSideStreetNameId")
                        .HasColumnType("int")
                        .HasColumnName("linkerstraatnaamObjectId");

                    b.Property<string>("MaintainerId")
                        .HasColumnType("varchar(18)")
                        .HasColumnName("wegbeheerder");

                    b.Property<string>("MaintainerName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("labelWegbeheerder");

                    b.Property<string>("MethodDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("methodeWegsegmentgeometrie");

                    b.Property<string>("MorphologyDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("morfologischeWegklasse");

                    b.Property<string>("RightSideStreetName")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("rechterstraatnaam");

                    b.Property<int?>("RightSideStreetNameId")
                        .HasColumnType("int")
                        .HasColumnName("rechterstraatnaamObjectId");

                    b.Property<string>("StatusDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("wegsegmentstatus");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.HasIndex("IsRemoved");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("IsRemoved"), false);

                    b.HasIndex("LeftSideStreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("LeftSideStreetNameId"), false);

                    b.HasIndex("RightSideStreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("RightSideStreetNameId"), false);

                    b.ToTable("Wegsegment", "RoadRegistryWfs");
                });
#pragma warning restore 612, 618
        }
    }
}
