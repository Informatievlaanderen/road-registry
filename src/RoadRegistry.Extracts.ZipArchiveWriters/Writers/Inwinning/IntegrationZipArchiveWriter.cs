namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.Inwinning;

using System.IO.Compression;
using System.Text;
using Extensions;
using Infrastructure.ShapeFile;
using NetTopologySuite.Geometries;
using Projections;
using Schemas.Inwinning.RoadNodes;
using Schemas.Inwinning.RoadSegments;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class IntegrationZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private const int IntegrationBufferInMeters = 350;

    public IntegrationZipArchiveWriter(Encoding encoding)
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

        const FeatureType featureType = FeatureType.Integration;

        var segmentsInContour = await zipArchiveDataProvider.GetRoadSegments(request.Contour, cancellationToken);
        var nodesInContour = await zipArchiveDataProvider.GetRoadNodes(request.Contour, cancellationToken);

        // segments integration
        var integrationBufferedSegmentsGeometries = segmentsInContour.Select(x => x.Geometry.Value.Buffer(IntegrationBufferInMeters)).ToList();

        var integrationSegments = new List<RoadSegmentExtractItem>();
        var integrationNodes = new List<RoadNodeExtractItem>();

        if (integrationBufferedSegmentsGeometries.Any())
        {
            var integrationBufferedContourGeometry = (IPolygonal)WellKnownGeometryFactories.Lambert08
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            var segmentsInIntegrationBuffer = await zipArchiveDataProvider.GetRoadSegments(
                integrationBufferedContourGeometry,
                cancellationToken);

            integrationSegments = segmentsInIntegrationBuffer.Except(segmentsInContour, new RoadSegmentEqualityComparerById()).ToList();
            integrationSegments = integrationSegments
                .Where(integrationSegment => integrationBufferedSegmentsGeometries.Any(segmentBufferedGeometry => segmentBufferedGeometry.Intersects(integrationSegment.Geometry.Value)))
                .ToList();

            // nodes integration
            var nodesInIntegrationBuffer = await zipArchiveDataProvider.GetRoadNodes(
                integrationBufferedContourGeometry,
                cancellationToken);

            var integrationNodeIds = integrationSegments.SelectMany(segment => new[] { segment.StartNodeId, segment.EndNodeId }).Distinct().ToList();
            integrationNodes = nodesInIntegrationBuffer.Where(integrationNode => integrationNodeIds.Contains(integrationNode.RoadNodeId))
                .ToList();
            integrationNodes = integrationNodes.Except(nodesInContour, new RoadNodeEqualityComparerById()).ToList();
        }

        await WriteRoadSegments();
        await WriteRoadNodes();

        async Task WriteRoadSegments()
        {
            var records = RoadSegmentsZipArchiveWriter.ConvertToDbaseRecords(integrationSegments, context);

            var writer = new Lambert08ShapeFileRecordWriter(_encoding);
            await writer.WriteToArchive(archive, ExtractFileName.Wegsegment, featureType, ShapeType.PolyLine, RoadSegmentDbaseRecord.Schema, records, cancellationToken);
        }

        async Task WriteRoadNodes()
        {
            var records = RoadNodesZipArchiveWriter.ConvertToDbaseRecords(integrationNodes)
                .Concat(RoadNodesZipArchiveWriter.BuildSchijnknoopDbaseRecords(integrationSegments, context));

            var writer = new Lambert08ShapeFileRecordWriter(_encoding);
            await writer.WriteToArchive(archive, ExtractFileName.Wegknoop, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
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
}
