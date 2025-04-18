namespace RoadRegistry.Tests;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Dbase.V2;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.ShapeFile.V2;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;

public static class Customizations
{
    public static T CreateWith<T>(this IFixture fixture, Action<T> modify)
    {
        var value = fixture.Create<T>();
        modify(value);
        return value;
    }

    public static MemoryStream CreateDbfFile<T>(this IFixture fixture, DbaseSchema schema, ICollection<T> records, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        foreach (var record in records)
        {
            updateRecord?.Invoke(record);
        }

        var writer = new DbaseRecordWriter(Encoding.UTF8);
        var dbaseChangeStream = writer.WriteToDbfStream(schema, records);

        return dbaseChangeStream;
    }

    public static MemoryStream CreateDbfFileWithOneRecord<T>(this IFixture fixture, DbaseSchema schema, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        return CreateDbfFileWithOneRecord(fixture, schema, fixture.Create<T>(), updateRecord);
    }

    public static MemoryStream CreateDbfFileWithOneRecord<T>(this IFixture fixture, DbaseSchema schema, T record, Action<T> updateRecord = null)
        where T : DbaseRecord
    {
        return CreateDbfFile(fixture, schema, [record], updateRecord);
    }

    public static MemoryStream CreateEmptyDbfFile<T>(this IFixture fixture, DbaseSchema schema)
        where T : DbaseRecord
    {
        return CreateDbfFile(fixture, schema, Array.Empty<T>());
    }

    public static MemoryStream CreateEmptyProjectionFormatFile(this IFixture fixture)
    {
        var projectionFormatStream = new MemoryStream();
        using var writer = new StreamWriter(projectionFormatStream, Encoding.UTF8, leaveOpen: true);
        writer.Write(string.Empty);

        return projectionFormatStream;
    }

    public static MemoryStream CreateEmptyRoadNodeShapeFile(this IFixture fixture)
    {
        return CreateRoadNodeShapeFile(fixture, Array.Empty<PointShapeContent>());
    }

    public static MemoryStream CreateEmptyRoadSegmentShapeFile(this IFixture fixture)
    {
        return CreateRoadSegmentShapeFile(fixture, Array.Empty<PolyLineMShapeContent>());
    }

    public static MemoryStream CreateEmptyTransactionZoneShapeFile(this IFixture fixture)
    {
        return CreateTransactionZoneShapeFile(fixture, Array.Empty<PolygonShapeContent>());
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

    public static MemoryStream CreateProjectionFormatFileWithOneRecord(this IFixture fixture)
    {
        var projectionFormatStream = new MemoryStream();
        using var writer = new StreamWriter(
            projectionFormatStream,
            Encoding.UTF8,
            leaveOpen: true);
        writer.Write(ProjectionFormat.BelgeLambert1972.Content);

        return projectionFormatStream;
    }

    public static MemoryStream CreateRoadNodeShapeFile(this IFixture fixture, ICollection<PointShapeContent> shapes)
    {
        return CreateShapeFile(NetTopologySuite.IO.Esri.ShapeType.Point, shapes
            .Select(x => Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPoint(x.Shape)));
    }

    public static MemoryStream CreateRoadNodeShapeFileWithOneRecord(this IFixture fixture)
    {
        return CreateRoadNodeShapeFile(fixture, [fixture.Create<PointShapeContent>()]);
    }

    public static MemoryStream CreateRoadSegmentShapeFile(this IFixture fixture, ICollection<PolyLineMShapeContent> shapes)
    {
        return CreateShapeFile(NetTopologySuite.IO.Esri.ShapeType.PolyLineM, shapes
            .Select(x => Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryMultiLineString(x.Shape)));
    }

    public static MemoryStream CreateTransactionZoneShapeFile(this IFixture fixture, ICollection<PolygonShapeContent> shapes)
    {
        return CreateShapeFile(NetTopologySuite.IO.Esri.ShapeType.Polygon, shapes
            .Select(x => Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPolygon(x.Shape)));
    }

    public static MemoryStream CreateRoadSegmentShapeFileWithOneRecord(this IFixture fixture, PolyLineMShapeContent polyLineMShapeContent = null)
    {
        if (polyLineMShapeContent is null)
        {
            polyLineMShapeContent = fixture.Create<PolyLineMShapeContent>();
        }

        return CreateRoadSegmentShapeFile(fixture, [polyLineMShapeContent]);
    }

    private static MemoryStream CreateShapeFile(NetTopologySuite.IO.Esri.ShapeType shapeType, IEnumerable<Geometry> geometries)
    {
        ArgumentNullException.ThrowIfNull(geometries);

        var fileName = ExtractFileName.Transactiezones;
        var featureType = FeatureType.Change;

        var writer = new ShapeFileRecordWriter(Encoding.UTF8);
        var archiveStream = new MemoryStream();
        var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update);

        var features = geometries
            .Select(x => (IFeature)new Feature(x, new AttributesTable()))
            .ToList();
        writer.WriteToArchive(archive, fileName, featureType, shapeType, [], features, CancellationToken.None).GetAwaiter().GetResult();

        var entry = archive.FindEntry(fileName.ToShapeFileName(featureType));

        using var entryStream = entry.Open();
        return entryStream.CopyToNewMemoryStream();
    }

    public static ZipArchive CreateUploadZipArchive(this Fixture fixture,
        ExtractsZipArchiveTestData testData,
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
        MemoryStream transactionZoneStream = null,
        MemoryStream roadSegmentShapeIntegrationStream = null,
        MemoryStream roadSegmentDbaseIntegrationStream = null,
        MemoryStream roadNodeShapeIntegrationStream = null,
        MemoryStream roadNodeDbaseIntegrationStream = null,
        MemoryStream archiveStream = null,
        ICollection<string> excludeFileNames = null
    )
    {
        var files = new Dictionary<string, Stream>
        {
            { "IWEGSEGMENT.SHP", roadSegmentShapeIntegrationStream ?? fixture.CreateEmptyRoadSegmentShapeFile() },
            { "IWEGSEGMENT.DBF", roadSegmentDbaseIntegrationStream ?? fixture.CreateEmptyDbfFile<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema) },
            { "IWEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
            { "EWEGSEGMENT.SHP", roadSegmentShapeExtractStream },
            { "EWEGSEGMENT.DBF", roadSegmentDbaseExtractStream },
            { "EWEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
            { "WEGSEGMENT.SHP", roadSegmentShapeChangeStream },
            { "WEGSEGMENT.DBF", roadSegmentDbaseChangeStream },
            { "WEGSEGMENT.PRJ", roadSegmentProjectionFormatStream },
            { "IWEGKNOOP.SHP", roadNodeShapeIntegrationStream ?? fixture.CreateEmptyRoadNodeShapeFile() },
            { "IWEGKNOOP.DBF", roadNodeDbaseIntegrationStream ?? fixture.CreateEmptyDbfFile<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema) },
            { "IWEGKNOOP.PRJ", roadNodeProjectionFormatStream },
            { "EWEGKNOOP.SHP", roadNodeShapeExtractStream },
            { "EWEGKNOOP.DBF", roadNodeDbaseExtractStream },
            { "EWEGKNOOP.PRJ", roadNodeProjectionFormatStream },
            { "WEGKNOOP.SHP", roadNodeShapeChangeStream },
            { "WEGKNOOP.DBF", roadNodeDbaseChangeStream },
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
            { "TRANSACTIEZONES.DBF", transactionZoneStream ?? fixture.CreateDbfFileWithOneRecord<TransactionZoneDbaseRecord>(TransactionZoneDbaseRecord.Schema) }
        };

        var random = new Random(fixture.Create<int>());
        var fileNames = files.Keys.ToList();
        if (excludeFileNames is not null && excludeFileNames.Any())
        {
            fileNames = fileNames.Where(fileName => !excludeFileNames.Contains(fileName, StringComparer.InvariantCultureIgnoreCase)).ToList();
        }
        var writeOrder = fileNames.OrderBy(_ => random.Next()).ToArray();

        var leaveArchiveStreamOpen = archiveStream is not null;
        archiveStream ??= new MemoryStream();

        var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, leaveArchiveStreamOpen, Encoding.UTF8);
        foreach (var file in writeOrder)
        {
            var stream = files[file];
            if (stream is not null)
            {
                stream.Position = 0;
                using var entryStream = archive.CreateEntry(file).Open();
                stream.CopyTo(entryStream);
            }
            else
            {
                var extractFileEntry = testData.ZipArchiveWithEmptyFiles.Entries.SingleOrDefault(x => string.Equals(x.Name, file, StringComparison.InvariantCultureIgnoreCase));
                if (extractFileEntry is null)
                {
                    throw new Exception($"No file found in {nameof(testData.ZipArchiveWithEmptyFiles)} with name {file}");
                }

                using var extractFileEntryStream = extractFileEntry.Open();
                using var entryStream = archive.CreateEntry(file).Open();
                extractFileEntryStream.CopyTo(entryStream);
            }
        }

        archiveStream.Position = 0;

        return archive;
    }

    public static T CreateWhichIsDifferentThan<T>(this IFixture fixture, params T[] illegalValues)
    {
        if (!illegalValues.Any())
        {
            throw new ArgumentException(nameof(illegalValues));
        }

        return CreateUntil<T>(fixture, value => illegalValues.All(illegalValue => !Equals(value, illegalValue)));
    }

    public static T CreateWhichIsDifferentThan<T>(this IFixture fixture, Func<T, T, bool> comparer, params T[] illegalValues)
    {
        return CreateUntil<T>(fixture, value => illegalValues.All(illegalValue => !comparer(value, illegalValue)));
    }

    public static T CreateUntil<T>(this IFixture fixture, Predicate<T> predicate)
    {
        var value = fixture.Create<T>();

        while (!predicate(value))
        {
            value = fixture.Create<T>();
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
                        Lanes = fixture.CreateMany<ImportedRoadSegmentLaneAttribute>(generator.Next(1, 10)).ToArray(),
                        Widths = fixture.CreateMany<ImportedRoadSegmentWidthAttribute>(generator.Next(1, 10)).ToArray(),
                        Surfaces = fixture.CreateMany<ImportedRoadSegmentSurfaceAttribute>(generator.Next(1, 10)).ToArray(),
                        PartOfEuropeanRoads = fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>(generator.Next(1, 10)).ToArray(),
                        PartOfNationalRoads = fixture.CreateMany<ImportedRoadSegmentNationalRoadAttribute>(generator.Next(1, 10)).ToArray(),
                        PartOfNumberedRoads = fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttribute>(generator.Next(1, 10)).ToArray(),
                        RecordingDate = fixture.Create<DateTime>(),
                        Origin = fixture.Create<ImportedOriginProperties>(),
                        When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
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

    public static void CustomizeRoadSegmentSideAttributes(this IFixture fixture)
    {
        fixture.Customize<RoadSegmentSideAttributes>(customization =>
            customization
                .FromFactory(generator =>
                    new RoadSegmentSideAttributes
                    {
                        StreetNameId = fixture.Create<bool>() ?
                            fixture.Create<StreetNameLocalId>()
                            : null
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeImportedOrganization(this IFixture fixture)
    {
        fixture.Customize<ImportedOrganization>(composer =>
            composer.FromFactory(generator =>
                new ImportedOrganization
                {
                    Code = fixture.Create<OrganizationId>(),
                    Name = fixture.Create<OrganizationName>(),
                    When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                }
            ).OmitAutoProperties()
        );
    }

    public static void CustomizeRoadSegmentAdded(this IFixture fixture)
    {
        fixture.CustomizeRoadSegmentSideAttributes();

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
                        SegmentVersion = fixture.Create<RoadSegmentVersion>(),
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
                        SegmentVersion = fixture.Create<RoadSegmentVersion>(),
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
                        SegmentVersion = fixture.Create<RoadSegmentVersion>(),
                        Number = fixture.Create<NumberedRoadNumber>(),
                        Direction = fixture.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = fixture.Create<RoadSegmentNumberedRoadOrdinal>()
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
                        AccessRestriction = fixture.Create<RoadSegmentAccessRestriction>(),
                        LeftSide = new RoadSegmentSideAttributes
                        {
                            StreetNameId = fixture.Create<StreetNameLocalId>()
                        },
                        RightSide = new RoadSegmentSideAttributes
                        {
                            StreetNameId = fixture.Create<StreetNameLocalId>()
                        },
                        Lanes = fixture.CreateMany<RoadSegmentLaneAttributes>().ToArray(),
                        Surfaces = fixture.CreateMany<RoadSegmentSurfaceAttributes>().ToArray(),
                        Widths = fixture.CreateMany<RoadSegmentWidthAttributes>().ToArray()
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

        fixture.Customize<RequestedRoadSegmentLaneAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(fixture);
                instance.AttributeId = fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Count = fixture.Create<RoadSegmentLaneCount>();
                instance.Direction = fixture.Create<RoadSegmentLaneDirection>();
            }).OmitAutoProperties());
    }
    public static RoadSegmentLaneAttributes CreateRoadSegmentLaneAttribute(this IFixture fixture, double roadSegmentGeometryLength)
    {
        var lane = fixture.Create<RoadSegmentLaneAttributes>();
        lane.FromPosition = 0;
        lane.ToPosition = RoadSegmentPosition.FromDouble(roadSegmentGeometryLength);
        return lane;
    }
    public static RequestedRoadSegmentLaneAttribute CreateRequestedRoadSegmentLaneAttribute(this IFixture fixture, double roadSegmentGeometryLength, int? attributeId = null)
    {
        var lane = fixture.Create<RequestedRoadSegmentLaneAttribute>();
        lane.FromPosition = 0;
        lane.ToPosition = RoadSegmentPosition.FromDouble(roadSegmentGeometryLength);
        if (attributeId is not null)
        {
            lane.AttributeId = attributeId.Value;
        }
        return lane;
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
                        SegmentVersion = fixture.Create<RoadSegmentVersion>(),
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
                        SegmentVersion = fixture.Create<RoadSegmentVersion>(),
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
                        SegmentVersion = fixture.Create<RoadSegmentVersion>(),
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

        fixture.Customize<RequestedRoadSegmentSurfaceAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(fixture);
                instance.AttributeId = fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Type = fixture.Create<RoadSegmentSurfaceType>();
            }).OmitAutoProperties());
    }
    public static RoadSegmentSurfaceAttributes CreateRoadSegmentSurfaceAttribute(this IFixture fixture, double roadSegmentGeometryLength)
    {
        var surface = fixture.Create<RoadSegmentSurfaceAttributes>();
        surface.FromPosition = 0;
        surface.ToPosition = RoadSegmentPosition.FromDouble(roadSegmentGeometryLength);
        return surface;
    }
    public static RequestedRoadSegmentSurfaceAttribute CreateRequestedRoadSegmentSurfaceAttribute(this IFixture fixture, double roadSegmentGeometryLength, int? attributeId = null)
    {
        var surface = fixture.Create<RequestedRoadSegmentSurfaceAttribute>();
        surface.FromPosition = 0;
        surface.ToPosition = RoadSegmentPosition.FromDouble(roadSegmentGeometryLength);
        if (attributeId is not null)
        {
            surface.AttributeId = attributeId.Value;
        }
        return surface;
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

        fixture.Customize<RequestedRoadSegmentWidthAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(fixture);
                instance.AttributeId = fixture.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Width = fixture.Create<RoadSegmentWidth>();
            }).OmitAutoProperties());
    }

    public static void CustomizeStreetNameModified(this IFixture fixture)
    {
        fixture.Customize<StreetNameModified>(customization =>
            customization
                .FromFactory(generator =>
                    new StreetNameModified
                    {
                        NameModified = fixture.Create<bool>(),
                        StatusModified = fixture.Create<bool>(),
                        HomonymAdditionModified = fixture.Create<bool>(),
                        Restored = fixture.Create<bool>(),
                        Record = fixture.Create<StreetNameRecord>(),
                        When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeStreetNameRecord(this IFixture fixture)
    {
        var streetNameStatuses = new[] { "voorgesteld", "inGebruik", "gehistoreerd", "afgekeurd" };

        fixture.Customize<StreetNameRecord>(customization =>
            customization
                .FromFactory(generator =>
                    {
                        var streetNameLocalId = fixture.Create<StreetNameLocalId>();
                        var name = fixture.Create<string>();
                        var homonymAddition = fixture.Create<string>();

                        return new StreetNameRecord
                        {
                            NisCode = generator.Next(10000, 100000).ToString(),
                            StreetNameId = new StreetNameId(streetNameLocalId),
                            PersistentLocalId = streetNameLocalId,
                            StreetNameStatus = streetNameStatuses[generator.Next(0, streetNameStatuses.Length)],
                            DutchName = name,
                            DutchHomonymAddition = homonymAddition,
                            DutchNameWithHomonymAddition = !string.IsNullOrEmpty(homonymAddition)
                                ? $"{name}_{homonymAddition}"
                                : name
                        };
                    }
                )
                .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNetworkExtractGotRequested(this IFixture fixture)
    {
        fixture.Customize<RoadNetworkExtractGotRequested>(
            customization =>
                customization
                    .FromFactory(_ =>
                        {
                            var externalRequestId = fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractGotRequested
                            {
                                Description = fixture.Create<ExtractDescription>(),
                                DownloadId = fixture.Create<Guid>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                Contour = new RoadNetworkExtractGeometry
                                {
                                    SpatialReferenceSystemIdentifier =
                                        SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                    MultiPolygon = [],
                                    Polygon = null
                                },
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant()),
                                IsInformative = false
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNetworkExtractGotRequestedV2(this IFixture fixture)
    {
        fixture.Customize<RoadNetworkExtractGotRequestedV2>(
            customization =>
                customization
                    .FromFactory(_ =>
                        {
                            var externalRequestId = fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractGotRequestedV2
                            {
                                Description = fixture.Create<ExtractDescription>(),
                                DownloadId = fixture.Create<Guid>(),
                                ExternalRequestId = externalRequestId,
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                Contour = new RoadNetworkExtractGeometry
                                {
                                    SpatialReferenceSystemIdentifier =
                                        SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                    MultiPolygon = [],
                                    Polygon = null
                                },
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant()),
                                IsInformative = false
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
    }

    public static void CustomizeRoadNetworkExtractClosed(this IFixture fixture)
    {
        fixture.Customize<RoadNetworkExtractClosed>(
            customization =>
                customization
                    .FromFactory(_ =>
                        {
                            var externalRequestId = fixture.Create<ExternalExtractRequestId>();
                            return new RoadNetworkExtractClosed
                            {
                                RequestId = ExtractRequestId.FromExternalRequestId(externalRequestId),
                                ExternalRequestId = externalRequestId,
                                DownloadIds = fixture.CreateMany<string>(Random.Shared.Next(1, 5)).ToArray(),
                                DateRequested = DateTime.UtcNow,
                                Reason = RoadNetworkExtractCloseReason.NoDownloadReceived,
                                When = InstantPattern.ExtendedIso.Format(SystemClock.Instance.GetCurrentInstant())
                            };
                        }
                    )
                    .OmitAutoProperties()
        );
    }

    public static RoadSegmentWidthAttributes CreateRoadSegmentWidthAttribute(this IFixture fixture, double roadSegmentGeometryLength)
    {
        var width = fixture.Create<RoadSegmentWidthAttributes>();
        width.FromPosition = 0;
        width.ToPosition = RoadSegmentPosition.FromDouble(roadSegmentGeometryLength);
        return width;
    }
    public static RequestedRoadSegmentWidthAttribute CreateRequestedRoadSegmentWidthAttribute(this IFixture fixture, double roadSegmentGeometryLength, int? attributeId = null)
    {
        var width = fixture.Create<RequestedRoadSegmentWidthAttribute>();
        width.FromPosition = 0;
        width.ToPosition = RoadSegmentPosition.FromDouble(roadSegmentGeometryLength);
        if (attributeId is not null)
        {
            width.AttributeId = attributeId.Value;
        }
        return width;
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

    public static bool EqualsCollection<T1, T2>(this IEnumerable<T1> enumerable1, IEnumerable<T2> enumerable2, Func<T1, T2, bool> comparer)
    {
        var collection1 = enumerable1.ToArray();
        var collection2 = enumerable2.ToArray();

        if (collection1.Length != collection2.Length)
        {
            return false;
        }

        for (var i = 0; i < collection1.Length; i++)
        {
            if (!comparer(collection1[i], collection2[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static PointShapeContent ToShapeContent(this Point point)
    {
        return new PointShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(point));
    }
    public static PolyLineMShapeContent ToShapeContent(this LineString lineString)
    {
        return lineString.ToMultiLineString().ToShapeContent();
    }
    public static PolyLineMShapeContent ToShapeContent(this MultiLineString lineString)
    {
        return new PolyLineMShapeContent(
            Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(lineString)
        );
    }
    public static PolygonShapeContent ToShapeContent(this Polygon polygon)
    {
        return new PolygonShapeContent(
            Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPolygon(polygon)
        );
    }

    public static IEnumerable<RequestedChange> CreateAllRequestedChanges(this Fixture fixture)
    {
        var changeProperties = typeof(RequestedChange).GetProperties();
        Assert.NotEmpty(changeProperties);

        var data = fixture
            .Create<RequestedChange>();

        foreach (var changeProperty in changeProperties)
        {
            var change = new RequestedChange();

            var changeValue = changeProperty.GetValue(data);
            Assert.NotNull(changeValue);
            changeProperty.SetValue(change, changeValue);

            yield return change;
        }
    }

    public static IEnumerable<AcceptedChange> CreateAllAcceptedChanges(this Fixture fixture)
    {
        var changeProperties = typeof(AcceptedChange)
            .GetProperties()
            .Where(x => x.Name != nameof(AcceptedChange.Problems))
            .ToArray();
        Assert.NotEmpty(changeProperties);

        var data = fixture
            .Create<AcceptedChange>();

        foreach (var changeProperty in changeProperties)
        {
            var change = new AcceptedChange();

            var changeValue = changeProperty.GetValue(data);
            Assert.NotNull(changeValue);
            changeProperty.SetValue(change, changeValue);

            yield return change;
        }
    }
}
