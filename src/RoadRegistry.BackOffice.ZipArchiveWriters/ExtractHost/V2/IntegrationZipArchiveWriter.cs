namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.Editor.Schema.RoadNodes;
using RoadRegistry.Editor.Schema.RoadSegments;
using ShapeFile.V2;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class IntegrationZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IStreetNameCache _streetNameCache;

    private const int IntegrationBufferInMeters = 350;

    public IntegrationZipArchiveWriter(
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        _streetNameCache = streetNameCache ?? throw new ArgumentNullException(nameof(streetNameCache));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveDataProvider);

        const FeatureType featureType = FeatureType.Integration;

        var segmentsInContour = await zipArchiveDataProvider.GetRoadSegments(request.Contour, cancellationToken);
        var nodesInContour = await zipArchiveDataProvider.GetRoadNodes(request.Contour, cancellationToken);

        // segments integration
        var integrationBufferedSegmentsGeometries = segmentsInContour.Select(x => x.Geometry.Buffer(IntegrationBufferInMeters)).ToList();

        var integrationSegments = new List<RoadSegmentRecord>();
        var integrationNodes = new List<RoadNodeRecord>();

        if (integrationBufferedSegmentsGeometries.Any())
        {
            var integrationBufferedContourGeometry =  (IPolygonal)WellKnownGeometryFactories.Default
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            var segmentsInIntegrationBuffer = await zipArchiveDataProvider.GetRoadSegments(
                integrationBufferedContourGeometry,
                cancellationToken);

            integrationSegments = segmentsInIntegrationBuffer.Except(segmentsInContour, new RoadSegmentRecordEqualityComparerById()).ToList();
            integrationSegments = integrationSegments.Where(integrationSegment => { return integrationBufferedSegmentsGeometries.Any(segmentBufferedGeometry => segmentBufferedGeometry.Intersects(integrationSegment.Geometry)); })
                .ToList();

            // nodes integration
            var nodesInIntegrationBuffer = await zipArchiveDataProvider.GetRoadNodes(
                integrationBufferedContourGeometry,
                cancellationToken);

            var integrationNodeIds = integrationSegments.SelectMany(segment => new[] { segment.StartNodeId, segment.EndNodeId }).Distinct().ToList();
            integrationNodes = nodesInIntegrationBuffer.Where(integrationNode => integrationNodeIds.Contains(integrationNode.Id))
                .ToList();
            integrationNodes = integrationNodes.Except(nodesInContour, new RoadNodeRecordEqualityComparerById()).ToList();
        }

        await WriteRoadSegments();
        await WriteRoadNodes();

        async Task WriteRoadSegments()
        {
            const ExtractFileName extractFilename = ExtractFileName.Wegsegment;

            var cachedStreetNameIds = integrationSegments
                .Select(record => record.LeftSideStreetNameId)
                .Union(integrationSegments.Select(record => record.RightSideStreetNameId))
                .Where(streetNameId => streetNameId > 0)
                .Select(streetNameId => streetNameId.Value)
                .Distinct()
                .ToList();
            var cachedStreetNames = await _streetNameCache.GetStreetNamesById(cachedStreetNameIds, cancellationToken);

            var writer = new ShapeFileRecordWriter(_encoding);

            var records = integrationSegments
                .OrderBy(x => x.Id)
                .Select(x =>
                {
                    var dbfRecord = new RoadSegmentDbaseRecord();
                    dbfRecord.FromBytes(x.DbaseRecord, _manager, _encoding);

                    if (dbfRecord.LSTRNMID.Value.HasValue && cachedStreetNames.ContainsKey(dbfRecord.LSTRNMID.Value.Value))
                        dbfRecord.LSTRNM.Value = cachedStreetNames[dbfRecord.LSTRNMID.Value.Value];

                    if (dbfRecord.RSTRNMID.Value.HasValue && cachedStreetNames.ContainsKey(dbfRecord.RSTRNMID.Value.Value))
                        dbfRecord.RSTRNM.Value = cachedStreetNames[dbfRecord.RSTRNMID.Value.Value];

                    return ((DbaseRecord)dbfRecord, x.Geometry);
                })
                .ToList();

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.PolyLine, RoadSegmentDbaseRecord.Schema, records, cancellationToken);
        }

        async Task WriteRoadNodes()
        {
            const ExtractFileName extractFilename = ExtractFileName.Wegknoop;

            var writer = new ShapeFileRecordWriter(_encoding);

            var records = integrationNodes
                .OrderBy(record => record.Id)
                .Select(node =>
                {
                    var dbfRecord = new RoadNodeDbaseRecord();
                    dbfRecord.FromBytes(node.DbaseRecord, _manager, _encoding);

                    return ((DbaseRecord)dbfRecord, node.Geometry);
                });

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }
}
