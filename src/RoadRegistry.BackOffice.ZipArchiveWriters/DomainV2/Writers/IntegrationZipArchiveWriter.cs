namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2.Writers;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Utilities;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using RoadNode;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.DbaseV2.RoadNodes;
using RoadRegistry.BackOffice.Extracts.DbaseV2.RoadSegments;
using RoadRegistry.BackOffice.ShapeFile.V2;
using RoadRegistry.Extensions;
using RoadSegment;
using RoadSegment.ValueObjects;
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
        var integrationBufferedSegmentsGeometries = segmentsInContour.Select(x => x.Geometry.Buffer(IntegrationBufferInMeters)).ToList();

        var integrationSegments = new List<RoadSegment>();
        var integrationNodes = new List<RoadNode>();

        if (integrationBufferedSegmentsGeometries.Any())
        {
            var integrationBufferedContourGeometry =  (IPolygonal)WellKnownGeometryFactories.Default
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            var segmentsInIntegrationBuffer = await zipArchiveDataProvider.GetRoadSegments(
                integrationBufferedContourGeometry,
                cancellationToken);

            integrationSegments = segmentsInIntegrationBuffer.Except(segmentsInContour, new RoadSegmentEqualityComparerById()).ToList();
            integrationSegments = integrationSegments.Where(integrationSegment => { return integrationBufferedSegmentsGeometries.Any(segmentBufferedGeometry => segmentBufferedGeometry.Intersects(integrationSegment.Geometry)); })
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
                .SelectMany(record => record.Attributes.StreetNameId.Values.Select(x => x.Value))
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
                    var leftStreetNameId = GetValue(x.Attributes.StreetNameId, RoadSegmentAttributeSide.Left);
                    var rightStreetNameId = GetValue(x.Attributes.StreetNameId, RoadSegmentAttributeSide.Right);

                    var dbfRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = x.RoadSegmentId },
                        WS_UIDN = { Value = $"{x.RoadSegmentId}_{new Rfc3339SerializableDateTimeOffset(x.LastModified.Timestamp.ToBelgianDateTimeOffset()).ToString()}"},
                        //WS_GIDN = { Value = "", }, //TODO-pr obsolete?

                        B_WK_OIDN = { Value = x.StartNodeId },
                        E_WK_OIDN = { Value = x.EndNodeId },
                        STATUS = { Value = GetValue(x.Attributes.Status) },
                        //LBLSTATUS = { Value = xxx },
                        MORF = { Value = GetValue(x.Attributes.Morphology) },
                        //LBLMORF = { Value = xxx },
                        WEGCAT = { Value = GetValue(x.Attributes.Category) },
                        //LBLWEGCAT = { Value = xxx },
                        LSTRNMID = { Value = leftStreetNameId },
                        LSTRNM = { Value = cachedStreetNames.GetValueOrDefault(leftStreetNameId) },
                        RSTRNMID = { Value = rightStreetNameId },
                        RSTRNM = { Value = cachedStreetNames.GetValueOrDefault(rightStreetNameId) },
                        BEHEER = { Value = GetValue(x.Attributes.MaintenanceAuthorityId) },
                        //LBLBEHEER = { Value = xxx },
                        METHODE = { Value = x.Attributes.GeometryDrawMethod },
                        //LBLMETHOD = { Value = xxx },
                        TGBEP = { Value = GetValue(x.Attributes.AccessRestriction) },
                        //LBLTGBEP = { Value = xxx },

                        //OPNDATUM = { Value = xxx },
                        BEGINTIJD = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                        BEGINORG = { Value = x.Origin.OrganizationId }
                    };

                    return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry);
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

                    return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry);
                });

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }

    private static T GetValue<T>(RoadSegmentDynamicAttributeValues<T> attributes)
    {
        return attributes.Values.Single().Value;
    }
    private static T GetValue<T>(RoadSegmentDynamicAttributeValues<T> attributes, RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Left).Value,
            RoadSegmentAttributeSide.Right => attributes.Values.Single(x => x.Side == RoadSegmentAttributeSide.Both || x.Side == RoadSegmentAttributeSide.Right).Value,
            _ => throw new InvalidOperationException("Only left or right side is allowed.")
        };
    }

    private sealed class RoadSegmentEqualityComparerById : IEqualityComparer<RoadSegment>
    {
        public bool Equals(RoadSegment x, RoadSegment y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(RoadSegment obj)
        {
            return obj.RoadSegmentId;
        }
    }

    private sealed class RoadNodeEqualityComparerById : IEqualityComparer<RoadNode>
    {
        public bool Equals(RoadNode x, RoadNode y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(RoadNode obj)
        {
            return obj.RoadNodeId;
        }
    }
}
