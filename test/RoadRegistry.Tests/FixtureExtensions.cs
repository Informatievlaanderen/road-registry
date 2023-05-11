namespace RoadRegistry.Tests;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extracts.Dbase;
using RoadRegistry.BackOffice.Messages;
using Point = NetTopologySuite.Geometries.Point;

public static class Customizations
{
    public static T CreateWhichIsDifferentThan<T>(this IFixture fixture, params T[] illegalValues)
    {
        var value = fixture.Create<T>();

        while (illegalValues.Any(illegalValue => Equals(value, illegalValue)))
        {
            value = fixture.Create<T>();
        }

        return value;
    }

    public static bool EqualsCollection<T>(this IEnumerable<T> enumerable1, IEnumerable<T> enumerable2)
    {
        var collection1 = enumerable1.ToArray();
        var collection2 = enumerable2.ToArray();

        if (collection1.Length != collection2.Length)
        {
            return false;
        }

        for (var i = 0; i < collection1.Length; i++)
        {
            if (!Equals(collection1[i], collection2[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static IEnumerable<T> CreateManyWhichIsDifferentThan<T>(this IFixture fixture, IEnumerable<T> illegalValue)
    {
        var value = fixture.CreateMany<T>();

        while (value.EqualsCollection(illegalValue))
        {
            value = fixture.CreateMany<T>();
        }

        return value;
    }

    public static void CustomizeGradeSeparatedJunctionAdded(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionAdded>(customization =>
            customization
                .FromFactory(generator =>
                    new GradeSeparatedJunctionAdded
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>(),
                        TemporaryId = fixture.Create<GradeSeparatedJunctionId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>(),
                        LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        UpperRoadSegmentId = fixture.Create<RoadSegmentId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeGradeSeparatedJunctionModified(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionModified>(customization =>
            customization
                .FromFactory(generator =>
                    new GradeSeparatedJunctionModified
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>(),
                        LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        UpperRoadSegmentId = fixture.Create<RoadSegmentId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeGradeSeparatedJunctionRemoved(this IFixture fixture)
    {
        fixture.Customize<GradeSeparatedJunctionRemoved>(customization =>
            customization
                .FromFactory(generator =>
                    new GradeSeparatedJunctionRemoved
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedGradeSeparatedJunction(this IFixture fixture)
    {
        fixture.Customize<ImportedGradeSeparatedJunction>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedGradeSeparatedJunction
                    {
                        Id = fixture.Create<GradeSeparatedJunctionId>(),
                        Type = fixture.Create<GradeSeparatedJunctionType>(),
                        LowerRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        UpperRoadSegmentId = fixture.Create<RoadSegmentId>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadNode(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadNode>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadNode
                    {
                        Id = fixture.Create<RoadNodeId>(),
                        Type = fixture.Create<RoadNodeType>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<Point>()),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegment(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegment>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegment
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        Version = fixture.Create<int>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        GeometryVersion = fixture.Create<GeometryVersion>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = fixture.Create<OrganizationId>(),
                            Name = fixture.Create<OrganizationName>()
                        },
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        LeftSide = fixture.Create<ImportedRoadSegmentSideAttribute>(),
                        RightSide = fixture.Create<ImportedRoadSegmentSideAttribute>(),
                        Lanes = fixture.CreateMany<ImportedRoadSegmentLaneAttribute>(generator.Next(0, 10)).ToArray(),
                        Widths = fixture.CreateMany<ImportedRoadSegmentWidthAttribute>(generator.Next(0, 10)).ToArray(),
                        Surfaces = fixture.CreateMany<ImportedRoadSegmentSurfaceAttribute>(generator.Next(0, 10)).ToArray(),
                        PartOfEuropeanRoads = fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>(generator.Next(0, 10)).ToArray(),
                        PartOfNationalRoads = fixture.CreateMany<ImportedRoadSegmentNationalRoadAttribute>(generator.Next(0, 10)).ToArray(),
                        PartOfNumberedRoads = fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttribute>(generator.Next(0, 10)).ToArray(),
                        RecordingDate = fixture.Create<DateTime>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentEuropeanRoadAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentEuropeanRoadAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentEuropeanRoadAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Number = fixture.Create<EuropeanRoadNumber>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentLaneAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentLaneAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentLaneAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Count = fixture.Create<RoadSegmentLaneCount>(),
                        Direction = fixture.Create<RoadSegmentLaneDirection>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentNationalRoadAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentNationalRoadAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentNationalRoadAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Number = fixture.Create<NationalRoadNumber>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentNumberedRoadAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentNumberedRoadAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentNumberedRoadAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentSideAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentSideAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentSideAttribute
                    {
                        StreetNameId = fixture.Create<int?>(),
                        StreetName = fixture.Create<string>(),
                        MunicipalityNISCode = fixture.Create<string>(),
                        Municipality = fixture.Create<string>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentSurfaceAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentSurfaceAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentSurfaceAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Type = fixture.Create<RoadSegmentSurfaceType>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedRoadSegmentWidthAttributes(this IFixture fixture)
    {
        fixture.Customize<ImportedRoadSegmentWidthAttribute>(customization =>
            customization
                .FromFactory(generator =>
                    new ImportedRoadSegmentWidthAttribute
                    {
                        AttributeId = fixture.Create<int>(),
                        Width = fixture.Create<RoadSegmentWidth>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>(),
                        Origin = fixture.Create<ImportedOriginProperties>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNetworkChangesAccepted(this IFixture fixture)
    {
        fixture.Customize<RoadNetworkChangesAccepted>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNetworkChangesAccepted
                    {
                        RequestId = fixture.Create<ArchiveId>(),
                        Reason = fixture.Create<Reason>(),
                        Operator = fixture.Create<OperatorName>(),
                        OrganizationId = fixture.Create<OrganizationId>(),
                        Organization = fixture.Create<OrganizationName>(),
                        TransactionId = fixture.Create<TransactionId>(),
                        Changes = fixture.CreateMany<AcceptedChange>(generator.Next(1, 5)).ToArray(),
                        When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeAdded(this IFixture fixture)
    {
        fixture.Customize<RoadNodeAdded>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNodeAdded
                    {
                        Id = fixture.Create<RoadNodeId>(),
                        TemporaryId = fixture.Create<RoadNodeId>(),
                        Type = fixture.Create<RoadNodeType>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<Point>()),
                        Version = 1
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeModified(this IFixture fixture)
    {
        fixture.Customize<RoadNodeModified>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNodeModified
                    {
                        Id = fixture.Create<RoadNodeId>(),
                        Type = fixture.Create<RoadNodeType>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<Point>()),
                        Version = fixture.Create<RoadNodeVersion>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNodeRemoved(this IFixture fixture)
    {
        fixture.Customize<RoadNodeRemoved>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadNodeRemoved
                    {
                        Id = fixture.Create<RoadNodeId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAdded(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAdded>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAdded
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        TemporaryId = fixture.Create<RoadSegmentId>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        Lanes = fixture.CreateMany<RoadSegmentLaneAttributes>(generator.Next(1, 5)).ToArray(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Surfaces = fixture.CreateMany<RoadSegmentSurfaceAttributes>(generator.Next(1, 5)).ToArray(),
                        Version = fixture.Create<int>(),
                        Widths = fixture.CreateMany<RoadSegmentWidthAttributes>(generator.Next(1, 5)).ToArray(),
                        LeftSide = fixture.Create<RoadSegmentSideAttributes>(),
                        RightSide = fixture.Create<RoadSegmentSideAttributes>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = fixture.Create<OrganizationId>(),
                            Name = fixture.Create<OrganizationName>()
                        },
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        GeometryVersion = fixture.Create<GeometryVersion>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAddedToEuropeanRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAddedToEuropeanRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAddedToEuropeanRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<EuropeanRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAddedToNationalRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAddedToNationalRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAddedToNationalRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAddedToNumberedRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAddedToNumberedRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        TemporaryAttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentLaneAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentLaneAttributes>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentLaneAttributes
                    {
                        AttributeId = fixture.Create<int>(),
                        Count = fixture.Create<RoadSegmentLaneCount>(),
                        Direction = fixture.Create<RoadSegmentLaneDirection>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentModified>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentModified
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        Lanes = fixture.CreateMany<RoadSegmentLaneAttributes>(generator.Next(1, 5)).ToArray(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Surfaces = fixture.CreateMany<RoadSegmentSurfaceAttributes>(generator.Next(1, 5)).ToArray(),
                        Version = fixture.Create<int>(),
                        Widths = fixture.CreateMany<RoadSegmentWidthAttributes>(generator.Next(1, 5)).ToArray(),
                        LeftSide = fixture.Create<RoadSegmentSideAttributes>(),
                        RightSide = fixture.Create<RoadSegmentSideAttributes>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = fixture.Create<OrganizationId>(),
                            Name = fixture.Create<OrganizationName>()
                        },
                        GeometryDrawMethod = fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        GeometryVersion = fixture.Create<GeometryVersion>(),
                        Status = fixture.Create<RoadSegmentStatus>(),
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        StartNodeId = fixture.Create<RoadNodeId>(),
                        EndNodeId = fixture.Create<RoadNodeId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAttributesModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentAttributesModified>(customization =>
            customization
                .FromFactory(_ =>
                    new RoadSegmentAttributesModified
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        Category = fixture.Create<RoadSegmentCategory>(),
                        Morphology = fixture.Create<RoadSegmentMorphology>(),
                        Version = fixture.Create<int>(),
                        MaintenanceAuthority = new MaintenanceAuthority
                        {
                            Code = fixture.Create<OrganizationId>(),
                            Name = fixture.Create<OrganizationName>()
                        },
                        Status = fixture.Create<RoadSegmentStatus>(),
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentGeometryModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentGeometryModified>(customization =>
            customization
                .FromFactory(_ =>
                    new RoadSegmentGeometryModified
                    {
                        Id = fixture.Create<RoadSegmentId>(),
                        Version = fixture.Create<int>(),
                        Geometry = GeometryTranslator.Translate(fixture.Create<MultiLineString>()),
                        GeometryVersion = fixture.Create<GeometryVersion>(),
                        Lanes = fixture.CreateMany<RoadSegmentLaneAttributes>(10).ToArray(),
                        Surfaces = fixture.CreateMany<RoadSegmentSurfaceAttributes>(10).ToArray(),
                        Widths = fixture.CreateMany<RoadSegmentWidthAttributes>(10).ToArray()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentOnNumberedRoadModified(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentOnNumberedRoadModified>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentOnNumberedRoadModified
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemoved(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemoved>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemoved
                    {
                        Id = fixture.Create<RoadSegmentId>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemovedFromEuropeanRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemovedFromEuropeanRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemovedFromEuropeanRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<EuropeanRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemovedFromNationalRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemovedFromNationalRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemovedFromNationalRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentRemovedFromNumberedRoad(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentRemovedFromNumberedRoad>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentRemovedFromNumberedRoad
                    {
                        AttributeId = fixture.Create<AttributeId>(),
                        SegmentId = fixture.Create<RoadSegmentId>(),
                        Number = fixture.Create<NationalRoadNumber>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentSurfaceAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentSurfaceAttributes>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentSurfaceAttributes
                    {
                        AttributeId = fixture.Create<int>(),
                        Type = fixture.Create<RoadSegmentSurfaceType>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>()
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentWidthAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentWidthAttributes>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentWidthAttributes
                    {
                        AttributeId = fixture.Create<int>(),
                        Width = fixture.Create<RoadSegmentWidth>(),
                        FromPosition = fixture.Create<RoadSegmentPosition>(),
                        ToPosition = fixture.Create<RoadSegmentPosition>(),
                        AsOfGeometryVersion = fixture.Create<int>()
                    }
                )
                .OmitAutoProperties()
        );
    }


    public static MemoryStream CreateDbfFileWithOneRecord<T>(this IFixture fixture, DbaseSchema schema, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        return CreateDbfFileWithOneRecord(fixture, schema, fixture.Create<T>(), updateRecord);
    }

    public static MemoryStream CreateDbfFileWithOneRecord<T>(this IFixture fixture, DbaseSchema schema, T record, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        return CreateDbfFile(fixture, schema, new[] { record }, updateRecord);
    }

    public static MemoryStream CreateEmptyDbfFile<T>(this IFixture fixture, DbaseSchema schema)
        where T : DbaseRecord
    {
        return CreateDbfFile(fixture, schema, Array.Empty<T>());
    }

    public static MemoryStream CreateDbfFile<T>(this IFixture fixture, DbaseSchema schema, ICollection<T> records, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        var dbaseChangeStream = new MemoryStream();

        using (var writer = new DbaseBinaryWriter(
           new DbaseFileHeader(
               fixture.Create<DateTime>(),
               DbaseCodePage.Western_European_ANSI,
               new DbaseRecordCount(records.Count),
               schema),
           new BinaryWriter(
               dbaseChangeStream,
               Encoding.UTF8,
               true)))
        {
            if (records.Any())
            {
                foreach (var record in records)
                {
                    updateRecord?.Invoke(record);
                    writer.Write(record);
                }
            }
            else
            {
                writer.Write(Array.Empty<T>());
            }
        }

        return dbaseChangeStream;
    }

    public static MemoryStream CreateProjectionFormatFileWithOneRecord(this IFixture fixture)
    {
        var projectionFormatStream = new MemoryStream();
        using (var writer = new StreamWriter(
                   projectionFormatStream,
                   Encoding.UTF8,
                   leaveOpen: true))
        {
            writer.Write(ProjectionFormat.BelgeLambert1972.Content);
        }

        return projectionFormatStream;
    }

    public static MemoryStream CreateEmptyProjectionFormatFile(this IFixture fixture)
    {
        var projectionFormatStream = new MemoryStream();
        using (var writer = new StreamWriter(
                   projectionFormatStream,
                   Encoding.UTF8,
                   leaveOpen: true))
        {
            writer.Write(string.Empty);
        }

        return projectionFormatStream;
    }

    public static MemoryStream CreateRoadSegmentShapeFileWithOneRecord(this IFixture fixture, PolyLineMShapeContent polyLineMShapeContent = null)
    {
        if (polyLineMShapeContent is null)
        {
            polyLineMShapeContent = fixture.Create<PolyLineMShapeContent>();
        }

        return CreateRoadSegmentShapeFile(fixture, new[] { polyLineMShapeContent });
    }

    public static MemoryStream CreateEmptyRoadSegmentShapeFile(this IFixture fixture)
    {
        return CreateRoadSegmentShapeFile(fixture, Array.Empty<PolyLineMShapeContent>());
    }

    public static MemoryStream CreateRoadSegmentShapeFile(this IFixture fixture, ICollection<PolyLineMShapeContent> shapes)
    {
        return CreateShapeFile(fixture, ShapeType.PolyLineM, shapes, shape => shape.Shape.NumberOfPoints > 0 ? BoundingBox3D.FromGeometry(shape.Shape) : BoundingBox3D.Empty);
    }

    public static MemoryStream CreateRoadNodeShapeFileWithOneRecord(this IFixture fixture)
    {
        return CreateRoadNodeShapeFile(fixture, new[] { fixture.Create<PointShapeContent>() });
    }

    public static MemoryStream CreateEmptyRoadNodeShapeFile(this IFixture fixture)
    {
        return CreateRoadNodeShapeFile(fixture, Array.Empty<PointShapeContent>());
    }

    public static MemoryStream CreateRoadNodeShapeFile(this IFixture fixture, ICollection<PointShapeContent> shapes)
    {
        return CreateShapeFile(fixture, ShapeType.Point, shapes, shape => BoundingBox3D.FromGeometry(shape.Shape));
    }

    public static MemoryStream CreateShapeFile<TShapeContent>(this IFixture fixture, ShapeType shapeType, ICollection<TShapeContent> shapes, Func<TShapeContent, BoundingBox3D> getBoundingBox3D)
        where TShapeContent : ShapeContent
    {
        ArgumentNullException.ThrowIfNull(shapes);

        var roadNodeShapeChangeStream = new MemoryStream();

        var shapeRecords = new List<ShapeRecord>();
        var fileWordLength = ShapeFileHeader.Length;
        var boundingBox3D = BoundingBox3D.Empty;
        var recordNumber = RecordNumber.Initial;
        foreach (var shape in shapes)
        {
            boundingBox3D = boundingBox3D.ExpandWith(getBoundingBox3D(shape));

            var shapeRecord = shape.RecordAs(recordNumber);
            fileWordLength = fileWordLength.Plus(shapeRecord.Length);
            shapeRecords.Add(shapeRecord);

            recordNumber = recordNumber.Next();
        }

        using (var writer = new ShapeBinaryWriter(
           new ShapeFileHeader(
               fileWordLength,
               shapeType,
               boundingBox3D),
           new BinaryWriter(
               roadNodeShapeChangeStream,
               Encoding.UTF8,
               true)))
        {
            if (shapeRecords.Any())
            {
                foreach (var shapeRecord in shapeRecords)
                {
                    writer.Write(shapeRecord);
                }
            }
            else
            {
                writer.Write(Array.Empty<ShapeRecord>());
            }
        }

        return roadNodeShapeChangeStream;
    }

    public static ZipArchive CreateUploadZipArchive(this Fixture fixture, ExtractsZipArchiveTestData testData,
        MemoryStream roadSegmentShapeChangeStream = null,
        MemoryStream roadSegmentProjectionFormatStream = null,
        MemoryStream roadSegmentDbaseChangeStream = null,
        MemoryStream roadNodeShapeChangeStream = null,
        MemoryStream roadNodeProjectionFormatStream = null,
        MemoryStream roadNodeDbaseChangeStream = null,
        MemoryStream europeanRoadChangeStream = null,
        MemoryStream numberedRoadChangeStream = null,
        MemoryStream nationalRoadChangeStream = null,
        MemoryStream laneChangeStream = null,
        MemoryStream widthChangeStream = null,
        MemoryStream surfaceChangeStream = null,
        MemoryStream gradeSeparatedJunctionChangeStream = null,
        MemoryStream roadSegmentShapeExtractStream = null,
        MemoryStream roadSegmentDbaseExtractStream = null,
        MemoryStream roadNodeShapeExtractStream = null,
        MemoryStream roadNodeDbaseExtractStream = null,
        MemoryStream europeanRoadExtractStream = null,
        MemoryStream numberedRoadExtractStream = null,
        MemoryStream nationalRoadExtractStream = null,
        MemoryStream laneExtractStream = null,
        MemoryStream widthExtractStream = null,
        MemoryStream surfaceExtractStream = null,
        MemoryStream gradeSeparatedJunctionExtractStream = null,
        MemoryStream transactionZoneStream = null
    )
    {
        if (transactionZoneStream is null)
        {
            transactionZoneStream = fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema);
        }

        var files = new Dictionary<string, Stream>
            {
                { "IWEGSEGMENT.DBF", fixture.CreateEmptyDbfFile<RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments.RoadSegmentDbaseRecord>(RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments.RoadSegmentDbaseRecord.Schema) },
                { "IWEGSEGMENT.SHP", fixture.CreateEmptyRoadSegmentShapeFile() },
                { "WEGSEGMENT.SHP", roadSegmentShapeChangeStream },
                { "EWEGSEGMENT.SHP", roadSegmentShapeExtractStream },
                { "WEGSEGMENT.DBF", roadSegmentDbaseChangeStream },
                { "EWEGSEGMENT.DBF", roadSegmentDbaseExtractStream },
                { "WEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
                { "IWEGKNOOP.DBF", fixture.CreateEmptyDbfFile<RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes.RoadNodeDbaseRecord>(RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes.RoadNodeDbaseRecord.Schema) },
                { "IWEGKNOOP.SHP", fixture.CreateEmptyRoadNodeShapeFile() },
                { "WEGKNOOP.SHP", roadNodeShapeChangeStream },
                { "EWEGKNOOP.SHP", roadNodeShapeExtractStream },
                { "WEGKNOOP.DBF", roadNodeDbaseChangeStream },
                { "EWEGKNOOP.DBF", roadNodeDbaseExtractStream },
                { "WEGKNOOP.PRJ", roadNodeProjectionFormatStream },
                { "ATTEUROPWEG.DBF", europeanRoadChangeStream },
                { "EATTEUROPWEG.DBF", europeanRoadExtractStream },
                { "ATTGENUMWEG.DBF", numberedRoadChangeStream },
                { "EATTGENUMWEG.DBF", numberedRoadExtractStream },
                { "ATTNATIONWEG.DBF", nationalRoadChangeStream },
                { "EATTNATIONWEG.DBF", nationalRoadExtractStream },
                { "ATTRIJSTROKEN.DBF", laneChangeStream },
                { "EATTRIJSTROKEN.DBF", laneExtractStream },
                { "ATTWEGBREEDTE.DBF", widthChangeStream },
                { "EATTWEGBREEDTE.DBF", widthExtractStream },
                { "ATTWEGVERHARDING.DBF", surfaceChangeStream },
                { "EATTWEGVERHARDING.DBF", surfaceExtractStream },
                { "RLTOGKRUISING.DBF", gradeSeparatedJunctionChangeStream },
                { "ERLTOGKRUISING.DBF", gradeSeparatedJunctionExtractStream },
                { "TRANSACTIEZONES.DBF", transactionZoneStream }
            };

        var random = new Random(fixture.Create<int>());
        var writeOrder = files.Keys.OrderBy(_ => random.Next()).ToArray();

        var archiveStream = new MemoryStream();
        using (var createArchive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            foreach (var file in writeOrder)
            {
                var stream = files[file];
                if (stream is not null)
                {
                    stream.Position = 0;
                    using (var entryStream = createArchive.CreateEntry(file).Open())
                    {
                        stream.CopyTo(entryStream);
                    }
                }
                else
                {
                    var extractFileEntry = testData.ZipArchiveWithEmptyFiles.Entries.Single(x => x.Name == file);
                    using (var extractFileEntryStream = extractFileEntry.Open())
                    using (var entryStream = createArchive.CreateEntry(file).Open())
                    {
                        extractFileEntryStream.CopyTo(entryStream);
                    }
                }
            }
        }
        archiveStream.Position = 0;

        return new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8);
    }
}
