﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RoadRegistry.Wms.Schema;

namespace RoadRegistry.Wms.Schema.Migrations
{
    [DbContext(typeof(WmsContext))]
    [Migration("20200929120358_AddStreetNameCachePosition")]
    partial class AddStreetNameCachePosition
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DesiredState")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DesiredStateChangedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("Name")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.ToTable("ProjectionStates","RoadRegistryWmsMeta");
                });

            modelBuilder.Entity("RoadRegistry.Wms.Schema.RoadSegmentRecord", b =>
                {
                    b.Property<int?>("Id")
                        .HasColumnName("wegsegmentID")
                        .HasColumnType("int");

                    b.Property<string>("AccessRestrictionDutchName")
                        .HasColumnName("lblToegangsbeperking")
                        .HasColumnType("varchar(64)");

                    b.Property<int?>("AccessRestrictionId")
                        .HasColumnName("toegangsbeperking")
                        .HasColumnType("int");

                    b.Property<string>("BeginApplication")
                        .HasColumnName("beginapplicatie")
                        .HasColumnType("varchar(100)");

                    b.Property<string>("BeginOperator")
                        .HasColumnName("beginoperator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BeginOrganizationId")
                        .HasColumnName("beginorganisatie")
                        .HasColumnType("varchar(18)");

                    b.Property<string>("BeginOrganizationName")
                        .HasColumnName("lblOrganisatie")
                        .HasColumnType("varchar(64)");

                    b.Property<int?>("BeginRoadNodeId")
                        .HasColumnName("beginWegknoopID")
                        .HasColumnType("int");

                    b.Property<string>("BeginTime")
                        .HasColumnName("begintijd")
                        .HasColumnType("varchar(100)");

                    b.Property<string>("CategoryDutchName")
                        .HasColumnName("lblCategorie")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("CategoryId")
                        .HasColumnName("categorie")
                        .HasColumnType("varchar(10)");

                    b.Property<int?>("EndRoadNodeId")
                        .HasColumnName("eindWegknoopID")
                        .HasColumnType("int");

                    b.Property<Geometry>("Geometry2D")
                        .HasColumnName("geometrie2D")
                        .HasColumnType("Geometry");

                    b.Property<int?>("GeometryVersion")
                        .HasColumnName("geometrieversie")
                        .HasColumnType("int");

                    b.Property<int?>("LeftSideMunicipalityId")
                        .HasColumnName("linksGemeente")
                        .HasColumnType("int");

                    b.Property<string>("LeftSideMunicipalityNisCode")
                        .HasColumnName("linksGemeenteNisCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LeftSideStreetName")
                        .HasColumnName("linksStraatnaam")
                        .HasColumnType("varchar(128)");

                    b.Property<int?>("LeftSideStreetNameId")
                        .HasColumnName("linksStraatnaamID")
                        .HasColumnType("int");

                    b.Property<string>("MaintainerId")
                        .HasColumnName("beheerder")
                        .HasColumnType("varchar(18)");

                    b.Property<string>("MaintainerName")
                        .HasColumnName("lblBeheerder")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("MethodDutchName")
                        .HasColumnName("lblMethode")
                        .HasColumnType("varchar(64)");

                    b.Property<int?>("MethodId")
                        .HasColumnName("methode")
                        .HasColumnType("int");

                    b.Property<string>("MorphologyDutchName")
                        .HasColumnName("lblMorfologie")
                        .HasColumnType("varchar(64)");

                    b.Property<int?>("MorphologyId")
                        .HasColumnName("morfologie")
                        .HasColumnType("int");

                    b.Property<DateTime?>("RecordingDate")
                        .HasColumnName("opnamedatum")
                        .HasColumnType("datetime2");

                    b.Property<int?>("RightSideMunicipalityId")
                        .HasColumnName("rechtsGemeente")
                        .HasColumnType("int");

                    b.Property<string>("RightSideMunicipalityNisCode")
                        .HasColumnName("rechtsGemeenteNisCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RightSideStreetName")
                        .HasColumnName("rechtsStraatnaam")
                        .HasColumnType("varchar(128)");

                    b.Property<int?>("RightSideStreetNameId")
                        .HasColumnName("rechtsStraatnaamID")
                        .HasColumnType("int");

                    b.Property<int?>("RoadSegmentVersion")
                        .HasColumnName("wegsegmentversie")
                        .HasColumnType("int");

                    b.Property<string>("StatusDutchName")
                        .HasColumnName("lblStatus")
                        .HasColumnType("varchar(64)");

                    b.Property<int?>("StatusId")
                        .HasColumnName("status")
                        .HasColumnType("int");

                    b.Property<long>("StreetNameCachePosition")
                        .HasColumnName("straatnaamCachePositie")
                        .HasColumnType("bigint");

                    b.Property<int?>("TransactionId")
                        .HasColumnName("transactieID")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("wegsegmentDenorm","RoadRegistryWms");
                });
#pragma warning restore 612, 618
        }
    }
}
