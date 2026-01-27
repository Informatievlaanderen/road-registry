namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.Inwinning;

using System.IO.Compression;
using System.Text;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Infrastructure.ShapeFile;
using NetTopologySuite.Geometries;
using Projections;
using Schemas.DomainV2.RoadNodes;
using Point = NetTopologySuite.Geometries.Point;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class RoadNodesZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public RoadNodesZipArchiveWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        ZipArchiveWriteContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveDataProvider);

        var nodes = (await zipArchiveDataProvider.GetRoadNodes(request.Contour, cancellationToken)).ToList();

        const ExtractFileName extractFilename = ExtractFileName.Wegknoop;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var writer = new Lambert08ShapeFileRecordWriter(_encoding);

        var records = new List<(DbaseRecord, Geometry)>();
        records.AddRange(ConvertToDbaseRecords(nodes));

        if (nodes.Any())
        {
            var segments = await zipArchiveDataProvider.GetRoadSegments(request.Contour, cancellationToken);
            records.AddRange(BuildSchijnknoopDbaseRecords(segments));
        }

        foreach (var featureType in featureTypes)
        {
            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }

    internal static IEnumerable<(DbaseRecord, Geometry)> ConvertToDbaseRecords(IEnumerable<RoadNodeExtractItem> nodes)
    {
        return nodes
            .OrderBy(record => record.Id)
            .Select(x =>
            {
                var dbfRecord = new RoadNodeDbaseRecord
                {
                    WK_OIDN = { Value = x.RoadNodeId },
                    TYPE = { Value = x.IsV2 ? RoadNodeTypeV2.Parse(x.Type).Translation.Identifier : MigrateRoadNodeType(RoadNodeType.Parse(x.Type)) },
                    GRENSKNOOP = { Value = ToGrensknoopDbfValue(x.Grensknoop, x.Geometry, x.IsV2) },
                    CREATIE = { Value = x.Origin.Timestamp.ToBrusselsDateTime() }
                };

                return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.Value);
            });
    }

    internal static IEnumerable<(DbaseRecord, Geometry)> BuildSchijnknoopDbaseRecords(IEnumerable<RoadSegmentExtractItem> segments)
    {
        var schijnknoopIdProvider = new NextAttributeIdProvider(new AttributeId(5000000));

        return segments
            .SelectMany(x =>
                x.Flatten().Skip(1)
                    .Select(segment =>
                    {
                        var coordinate = segment.Geometry.Value.Coordinate;
                        var nodeGeometry = RoadNodeGeometry.Create(new Point(coordinate.X, coordinate.Y) { SRID = x.Geometry.SRID });

                        var dbfRecord = new RoadNodeDbaseRecord
                        {
                            WK_OIDN = { Value = schijnknoopIdProvider.Next() },
                            TYPE = { Value = RoadNodeTypeV2.Schijnknoop },
                            GRENSKNOOP = { Value = ToGrensknoopDbfValue(IsWithin10MeterOfGrens(nodeGeometry)) },
                            CREATIE = { Value = x.Origin.Timestamp.ToBrusselsDateTime() }
                        };

                        return ((DbaseRecord)dbfRecord, (Geometry)nodeGeometry.Value);
                    })
            );
    }

    private static int MigrateRoadNodeType(RoadNodeType v1)
    {
        var mapping = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 1 },
            { 5, 4 }
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }

    private static short ToGrensknoopDbfValue(bool grensknoop, RoadNodeGeometry geometry, bool isV2)
    {
        return ToGrensknoopDbfValue(isV2
            ? grensknoop
            : IsWithin10MeterOfGrens(geometry));
    }
    private static short ToGrensknoopDbfValue(bool grensknoop)
    {
        return grensknoop
            ? (short)0
            : (short)-8;
    }

    private static bool IsWithin10MeterOfGrens(RoadNodeGeometry geometry)
    {
        //TODO-pr indien x.Grensknoop null, dan:
        //‘-8’ indien >10m van de gewestgrens)
        //'0' indien ≤10m van de gewestgrens
        throw new NotImplementedException();
    }
}
