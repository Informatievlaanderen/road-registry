namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
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
        IZipArchiveDataSession zipArchiveData,
        ZipArchiveWriteContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveData);

        var editableSegments = await GetEditableRoadSegments(request, zipArchiveData, cancellationToken);
        var nodes = await GetEditableRoadNodes(request, zipArchiveData, editableSegments, cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.Wegknoop;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var writer = new Lambert08ShapeFileRecordWriter(_encoding);

        var records = new List<(DbaseRecord, Geometry)>();
        records.AddRange(ConvertToDbaseRecords(nodes));
        records.AddRange(BuildSchijnknoopDbaseRecords(editableSegments, context));

        await WriteIntegration(writer, archive, request, zipArchiveData, context, editableSegments, nodes, cancellationToken);

        foreach (var featureType in featureTypes)
        {
            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }

    private async Task WriteIntegration(
        ShapeFileRecordWriter writer,
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataSession zipArchiveData,
        ZipArchiveWriteContext context,
        IReadOnlyCollection<RoadSegmentExtractItem> editableSegments,
        IReadOnlyCollection<RoadNodeExtractItem> editableNodes,
        CancellationToken cancellationToken)
    {
        const int IntegrationBufferInMeters = 350;

        var segmentsInContour = await zipArchiveData.GetRoadSegments(request.Contour, cancellationToken);

        var integrationBufferedSegmentsGeometries = segmentsInContour.Select(x => x.Geometry.Value.Buffer(IntegrationBufferInMeters)).ToList();

        var integrationSegments = new List<RoadSegmentExtractItem>();
        var integrationNodes = new List<RoadNodeExtractItem>();

        if (integrationBufferedSegmentsGeometries.Any())
        {
            var integrationBufferedContourGeometry = (IPolygonal)WellKnownGeometryFactories.Lambert08
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            var segmentsInIntegrationBuffer = await zipArchiveData.GetRoadSegments(
                integrationBufferedContourGeometry,
                cancellationToken);

            // Only the bewerkbare data is taken out of the buffer: what else lies inside the contour joins the
            // integratiedata.
            integrationSegments = segmentsInIntegrationBuffer
                .Except(editableSegments, new RoadSegmentEqualityComparerById())
                .Where(integrationSegment => integrationBufferedSegmentsGeometries.Any(segmentBufferedGeometry => segmentBufferedGeometry.Intersects(integrationSegment.Geometry.Value)))
                .ToList();

            // nodes integration
            var nodesInIntegrationBuffer = await zipArchiveData.GetRoadNodes(
                integrationBufferedContourGeometry,
                cancellationToken);

            var integrationNodeIds = integrationSegments
                .SelectMany(segment => new[] { segment.StartNodeId, segment.EndNodeId })
                .Distinct()
                .ToHashSet();
            integrationNodes = nodesInIntegrationBuffer
                .Where(integrationNode => integrationNodeIds.Contains(integrationNode.RoadNodeId))
                .Except(editableNodes, new RoadNodeEqualityComparerById())
                .ToList();
        }

        context.IntegrationSegments = integrationSegments;

        var records = ConvertToDbaseRecords(integrationNodes)
            .Concat(BuildSchijnknoopDbaseRecords(integrationSegments, context));

        await writer.WriteToArchive(archive, ExtractFileName.Wegknoop, FeatureType.Integration, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
    }

    private static async Task<IReadOnlyCollection<RoadSegmentExtractItem>> GetEditableRoadSegments(
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataSession zipArchiveData,
        CancellationToken cancellationToken)
    {
        var editableRoadSegmentIds = await zipArchiveData.GetEditableRoadSegmentIds(request.Contour, cancellationToken);

        return (await zipArchiveData.GetRoadSegments(request.Contour, cancellationToken))
            .Where(x => editableRoadSegmentIds.Contains(x.RoadSegmentId))
            .ToList();
    }

    // The begin- and eindknopen of the bewerkbare road segments. A road node of the contour that only connects road
    // segments in the integratiedata goes into the integratiedata with them.
    private static async Task<IReadOnlyCollection<RoadNodeExtractItem>> GetEditableRoadNodes(
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataSession zipArchiveData,
        IReadOnlyCollection<RoadSegmentExtractItem> editableSegments,
        CancellationToken cancellationToken)
    {
        var editableRoadNodeIds = editableSegments
            .SelectMany(EditableRoadSegments.RoadNodeIdsOf)
            .ToHashSet();

        return (await zipArchiveData.GetRoadNodes(request.Contour, cancellationToken))
            .Where(x => editableRoadNodeIds.Contains(x.RoadNodeId))
            .ToList();
    }

    private static IEnumerable<(DbaseRecord, Geometry)> ConvertToDbaseRecords(IEnumerable<RoadNodeExtractItem> nodes)
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

    private static IEnumerable<(DbaseRecord, Geometry)> BuildSchijnknoopDbaseRecords(IEnumerable<RoadSegmentExtractItem> segments, ZipArchiveWriteContext context)
    {
        return segments
            .SelectMany(x =>
                x.Flatten().Skip(1)
                    .Select(segment =>
                    {
                        var coordinate = segment.Geometry.Value.Coordinate;
                        var nodeGeometry = RoadNodeGeometry.Create(new Point(coordinate.X, coordinate.Y) { SRID = x.Geometry.SRID });

                        var dbfRecord = new RoadNodeDbaseRecord
                        {
                            WK_OIDN = { Value = context.NewSchijnknoopId() },
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
            { 1, 10 },
            { 2, 11 },
            { 3, 12 },
            { 4, 10 },
            { 5, 13 }
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
            ? (short)-8
            : (short)0;
    }

    private static bool IsWithin10MeterOfGrens(RoadNodeGeometry geometry)
    {
        return GewestGrens.IsCloseToBorder(geometry.Value, 10);
    }

    private sealed class RoadNodeEqualityComparerById : IEqualityComparer<RoadNodeExtractItem>
    {
        public bool Equals(RoadNodeExtractItem? x, RoadNodeExtractItem? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(RoadNodeExtractItem obj)
        {
            return obj.RoadNodeId;
        }
    }

    private sealed class RoadSegmentEqualityComparerById : IEqualityComparer<RoadSegmentExtractItem>
    {
        public bool Equals(RoadSegmentExtractItem? x, RoadSegmentExtractItem? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(RoadSegmentExtractItem obj)
        {
            return obj.RoadSegmentId;
        }
    }
}
