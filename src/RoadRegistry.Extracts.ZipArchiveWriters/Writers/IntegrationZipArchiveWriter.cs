namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Utilities;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Schemas.ExtractV2.RoadNodes;
using RoadRegistry.Extracts.Schemas.ExtractV2.RoadSegments;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadSegment.ValueObjects;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class IntegrationZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly IStreetNameCache _streetNameCache;

    private const int IntegrationBufferInMeters = 350;

    public IntegrationZipArchiveWriter(
        IStreetNameCache streetNameCache,
        Encoding encoding)
    {
        _streetNameCache = streetNameCache.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
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
        var integrationBufferedSegmentsGeometries = segmentsInContour.Select(x => x.Geometry.ToGeometry().Buffer(IntegrationBufferInMeters)).ToList();

        var integrationSegments = new List<RoadSegmentExtractItem>();
        var integrationNodes = new List<RoadNodeExtractItem>();

        if (integrationBufferedSegmentsGeometries.Any())
        {
            var integrationBufferedContourGeometry =  (IPolygonal)WellKnownGeometryFactories.Default
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            var segmentsInIntegrationBuffer = await zipArchiveDataProvider.GetRoadSegments(
                integrationBufferedContourGeometry,
                cancellationToken);

            integrationSegments = segmentsInIntegrationBuffer.Except(segmentsInContour, new RoadSegmentEqualityComparerById()).ToList();
            integrationSegments = integrationSegments.Where(integrationSegment => { return integrationBufferedSegmentsGeometries.Any(segmentBufferedGeometry => segmentBufferedGeometry.Intersects(integrationSegment.Geometry.ToGeometry())); })
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
            const ExtractFileName extractFilename = ExtractFileName.Wegsegment;

            var cachedStreetNameIds = integrationSegments
                .SelectMany(record => record.StreetNameId.Values.Select(x => x.Value))
                .Where(streetNameId => streetNameId > 0)
                .Select(streetNameId => streetNameId.ToInt32())
                .Distinct()
                .ToList();
            var cachedStreetNames = await _streetNameCache.GetStreetNamesById(cachedStreetNameIds, cancellationToken);

            var writer = new ShapeFileRecordWriter(_encoding);

            var records = integrationSegments
                .OrderBy(x => x.Id)
                .Select(x =>
                {
                    var leftStreetNameId = GetValue(x.StreetNameId, RoadSegmentAttributeSide.Left);
                    var rightStreetNameId = GetValue(x.StreetNameId, RoadSegmentAttributeSide.Right);

                    var dbfRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = x.RoadSegmentId },
                        WS_UIDN = { Value = $"{x.RoadSegmentId}_{new Rfc3339SerializableDateTimeOffset(x.LastModified.Timestamp.ToBelgianDateTimeOffset()).ToString()}"},
                        //WS_GIDN = { Value = "", },

                        B_WK_OIDN = { Value = x.StartNodeId },
                        E_WK_OIDN = { Value = x.EndNodeId },
                        STATUS = { Value = GetValue(x.Status) },
                        //LBLSTATUS = { Value = xxx },
                        MORF = { Value = GetValue(x.Morphology) },
                        //LBLMORF = { Value = xxx },
                        WEGCAT = { Value = GetValue(x.Category) },
                        //LBLWEGCAT = { Value = xxx },
                        LSTRNMID = { Value = leftStreetNameId },
                        LSTRNM = { Value = cachedStreetNames.GetValueOrDefault(leftStreetNameId) },
                        RSTRNMID = { Value = rightStreetNameId },
                        RSTRNM = { Value = cachedStreetNames.GetValueOrDefault(rightStreetNameId) },
                        BEHEER = { Value = GetValue(x.MaintenanceAuthorityId) },
                        //LBLBEHEER = { Value = xxx },
                        METHODE = { Value = x.GeometryDrawMethod },
                        //LBLMETHOD = { Value = xxx },
                        TGBEP = { Value = GetValue(x.AccessRestriction) },
                        //LBLTGBEP = { Value = xxx },

                        //OPNDATUM = { Value = xxx },
                        BEGINTIJD = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                        BEGINORG = { Value = x.Origin.OrganizationId }
                    };

                    return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.ToGeometry());
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
                .Select(x =>
                {
                    var dbfRecord = new RoadNodeDbaseRecord
                    {
                        WK_OIDN = { Value = x.RoadNodeId },
                        WK_UIDN = { Value = $"{x.RoadNodeId}_{new Rfc3339SerializableDateTimeOffset(x.LastModified.Timestamp.ToBelgianDateTimeOffset()).ToString()}"},
                        TYPE = { Value = x.Type },
                        LBLTYPE = { Value = x.Type.ToDutchString() },
                        BEGINTIJD = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                        BEGINORG = { Value = x.Origin.OrganizationId }
                    };

                    return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.ToGeometry());
                });

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }

    private static T GetValue<T>(RoadRegistry.Extracts.Projections.RoadSegmentDynamicAttributeValues<T> attributes)
    {
        return attributes.Values.Single().Value;
    }
    private static T GetValue<T>(RoadRegistry.Extracts.Projections.RoadSegmentDynamicAttributeValues<T> attributes, RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Left).Value,
            RoadSegmentAttributeSide.Right => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Right).Value,
            _ => throw new InvalidOperationException("Only left or right side is allowed.")
        };
    }

    private sealed class RoadSegmentEqualityComparerById : IEqualityComparer<RoadSegmentExtractItem>
    {
        public bool Equals(RoadSegmentExtractItem x, RoadSegmentExtractItem y)
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
        public bool Equals(RoadNodeExtractItem x, RoadNodeExtractItem y)
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
