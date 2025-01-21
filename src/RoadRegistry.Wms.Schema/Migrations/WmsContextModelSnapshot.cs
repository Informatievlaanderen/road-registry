﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using RoadRegistry.Wms.Schema;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    [DbContext(typeof(WmsContext))]
    partial class WmsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

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

                    b.ToTable("ProjectionStates", "RoadRegistryWmsMeta");
                });

            modelBuilder.Entity("RoadRegistry.Wms.Schema.OverlappingTransactionZonesRecord", b =>
                {
                    b.Property<Guid>("DownloadId1")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("DownloadId2")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Geometry>("Contour")
                        .IsRequired()
                        .HasColumnType("Geometry");

                    b.Property<string>("Description1")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Omschrijving1");

                    b.Property<string>("Description2")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Omschrijving2");

                    b.HasKey("DownloadId1", "DownloadId2");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("DownloadId1", "DownloadId2"));

                    b.HasIndex("DownloadId1");

                    b.HasIndex("DownloadId2");

                    b.ToTable("OverlappendeBijwerkingszones", "RoadRegistryWms");
                });

            modelBuilder.Entity("RoadRegistry.Wms.Schema.RoadSegmentEuropeanRoadAttributeRecord", b =>
                {
                    b.Property<int>("EU_OIDN")
                        .HasColumnType("int")
                        .HasColumnName("EU_OIDN");

                    b.Property<string>("BEGINORG")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("BEGINORG");

                    b.Property<DateTime>("BEGINTIJD")
                        .HasColumnType("datetime2")
                        .HasColumnName("BEGINTIJD");

                    b.Property<string>("EUNUMMER")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("EUNUMMER");

                    b.Property<string>("LBLBGNORG")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("LBLBGNORG");

                    b.Property<int>("WS_OIDN")
                        .HasColumnType("int")
                        .HasColumnName("WS_OIDN");

                    b.HasKey("EU_OIDN");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("EU_OIDN"), false);

                    b.ToTable("EuropeseWeg", "RoadRegistryWms");
                });

            modelBuilder.Entity("RoadRegistry.Wms.Schema.RoadSegmentNationalRoadAttributeRecord", b =>
                {
                    b.Property<int>("NW_OIDN")
                        .HasColumnType("int")
                        .HasColumnName("NW_OIDN");

                    b.Property<string>("BEGINORG")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("BEGINORG");

                    b.Property<DateTime>("BEGINTIJD")
                        .HasColumnType("datetime2")
                        .HasColumnName("BEGINTIJD");

                    b.Property<string>("IDENT2")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("IDENT2");

                    b.Property<string>("LBLBGNORG")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("LBLBGNORG");

                    b.Property<int>("WS_OIDN")
                        .HasColumnType("int")
                        .HasColumnName("WS_OIDN");

                    b.HasKey("NW_OIDN");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("NW_OIDN"), false);

                    b.ToTable("NationaleWeg", "RoadRegistryWms");
                });

            modelBuilder.Entity("RoadRegistry.Wms.Schema.RoadSegmentRecord", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int")
                        .HasColumnName("wegsegmentID");

                    b.Property<string>("AccessRestrictionDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("lblToegangsbeperking");

                    b.Property<int?>("AccessRestrictionId")
                        .HasColumnType("int")
                        .HasColumnName("toegangsbeperking");

                    b.Property<string>("BeginApplication")
                        .HasColumnType("varchar(100)")
                        .HasColumnName("beginapplicatie");

                    b.Property<string>("BeginOrganizationId")
                        .HasColumnType("varchar(18)")
                        .HasColumnName("beginorganisatie");

                    b.Property<string>("BeginOrganizationName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("lblOrganisatie");

                    b.Property<int?>("BeginRoadNodeId")
                        .HasColumnType("int")
                        .HasColumnName("beginWegknoopID");

                    b.Property<string>("BeginTime")
                        .HasColumnType("varchar(100)")
                        .HasColumnName("begintijd");

                    b.Property<string>("CategoryDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("lblCategorie");

                    b.Property<string>("CategoryId")
                        .HasColumnType("varchar(10)")
                        .HasColumnName("categorie");

                    b.Property<int?>("EndRoadNodeId")
                        .HasColumnType("int")
                        .HasColumnName("eindWegknoopID");

                    b.Property<Geometry>("Geometry2D")
                        .HasColumnType("Geometry")
                        .HasColumnName("geometrie2D");

                    b.Property<int?>("GeometryVersion")
                        .HasColumnType("int")
                        .HasColumnName("geometrieversie");

                    b.Property<bool>("IsRemoved")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false)
                        .HasColumnName("verwijderd");

                    b.Property<int?>("LeftSideMunicipalityId")
                        .HasColumnType("int")
                        .HasColumnName("linksGemeente");

                    b.Property<string>("LeftSideMunicipalityNisCode")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("linksGemeenteNisCode");

                    b.Property<string>("LeftSideStreetName")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("linksStraatnaam");

                    b.Property<int?>("LeftSideStreetNameId")
                        .HasColumnType("int")
                        .HasColumnName("linksStraatnaamID");

                    b.Property<string>("MaintainerId")
                        .HasColumnType("varchar(18)")
                        .HasColumnName("beheerder");

                    b.Property<string>("MaintainerName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("lblBeheerder");

                    b.Property<string>("MethodDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("lblMethode");

                    b.Property<int?>("MethodId")
                        .HasColumnType("int")
                        .HasColumnName("methode");

                    b.Property<string>("MorphologyDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("lblMorfologie");

                    b.Property<int?>("MorphologyId")
                        .HasColumnType("int")
                        .HasColumnName("morfologie");

                    b.Property<DateTime?>("RecordingDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("opnamedatum");

                    b.Property<int?>("RightSideMunicipalityId")
                        .HasColumnType("int")
                        .HasColumnName("rechtsGemeente");

                    b.Property<string>("RightSideMunicipalityNisCode")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("rechtsGemeenteNisCode");

                    b.Property<string>("RightSideStreetName")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("rechtsStraatnaam");

                    b.Property<int?>("RightSideStreetNameId")
                        .HasColumnType("int")
                        .HasColumnName("rechtsStraatnaamID");

                    b.Property<int?>("RoadSegmentVersion")
                        .HasColumnType("int")
                        .HasColumnName("wegsegmentversie");

                    b.Property<string>("StatusDutchName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("lblStatus");

                    b.Property<int?>("StatusId")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<int?>("TransactionId")
                        .HasColumnType("int")
                        .HasColumnName("transactieID");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"));

                    b.HasIndex("IsRemoved");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("IsRemoved"), false);

                    b.HasIndex("LeftSideStreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("LeftSideStreetNameId"), false);

                    b.HasIndex("MorphologyId")
                        .HasDatabaseName("wegsegmentmorfologie");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("MorphologyId"), false);

                    b.HasIndex("RightSideStreetNameId");

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("RightSideStreetNameId"), false);

                    b.ToTable("wegsegmentDenorm", "RoadRegistryWms");
                });

            modelBuilder.Entity("RoadRegistry.Wms.Schema.TransactionZoneRecord", b =>
                {
                    b.Property<Guid>("DownloadId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Geometry>("Contour")
                        .IsRequired()
                        .HasColumnType("Geometry");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Omschrijving");

                    b.HasKey("DownloadId");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("DownloadId"));

                    b.ToTable("Bijwerkingszones", "RoadRegistryWms");
                });
#pragma warning restore 612, 618
        }
    }
}
