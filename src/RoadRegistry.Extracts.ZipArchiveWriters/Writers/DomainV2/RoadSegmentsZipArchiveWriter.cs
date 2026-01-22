namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Utilities;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadSegment.ValueObjects;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class RoadSegmentsZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly IStreetNameCache _streetNameCache;

    public RoadSegmentsZipArchiveWriter(
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

        var segments = await zipArchiveDataProvider.GetRoadSegments(
            request.Contour, cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.Wegsegment;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var cachedStreetNameIds = segments
            .SelectMany(record => record.StreetNameId.Values.Select(x => x.Value))
            .Where(streetNameId => streetNameId > 0)
            .Select(streetNameId => streetNameId.ToInt32())
            .Distinct()
            .ToList();
        var cachedStreetNames = await _streetNameCache.GetStreetNamesById(cachedStreetNameIds, cancellationToken);

        var writer = new ShapeFileRecordWriter(_encoding);

        foreach (var featureType in featureTypes)
        {
            var records = segments
                .OrderBy(x => x.Id)
                .Select(x =>
                {
                    var leftStreetNameId = GetValue(x.StreetNameId, RoadSegmentAttributeSide.Left);
                    var rightStreetNameId = GetValue(x.StreetNameId, RoadSegmentAttributeSide.Right);
                    var status = GetValue(x.Status);
                    var morphology = GetValue(x.Morphology);
                    var accessRestriction = GetValue(x.AccessRestriction);

                    var statusValue = x.IsV2 ? RoadSegmentStatusV2.Parse(status).Translation.Identifier : MigrateToV2(RoadSegmentStatus.Parse(status));
                    var morphologyValue = x.IsV2 ? RoadSegmentMorphologyV2.Parse(morphology).Translation.Identifier : MigrateToV2(RoadSegmentMorphology.Parse(morphology));
                    var accessRestrictionValue = x.IsV2 ? RoadSegmentAccessRestrictionV2.Parse(accessRestriction).Translation.Identifier : MigrateToV2(RoadSegmentAccessRestriction.Parse(accessRestriction));

                    var dbfRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = x.RoadSegmentId },
                        WS_UIDN = { Value = $"{x.RoadSegmentId}_{new Rfc3339SerializableDateTimeOffset(x.LastModified.Timestamp.ToBelgianDateTimeOffset()).ToString()}" },
                        //WS_GIDN = { Value = "", },

                        B_WK_OIDN = { Value = x.StartNodeId },
                        E_WK_OIDN = { Value = x.EndNodeId },
                        STATUS = { Value = statusValue },
                        //LBLSTATUS = { Value = xxx },
                        MORF = { Value = morphologyValue },
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
                        TOEGANG = { Value = accessRestrictionValue },
                        //LBLTGBEP = { Value = xxx },

                        //OPNDATUM = { Value = xxx },
                        BEGINTIJD = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                        BEGINORG = { Value = x.Origin.OrganizationId }
                    };

                    return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.Value);
                })
                .ToList();

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.PolyLine, RoadSegmentDbaseRecord.Schema, records, cancellationToken);
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

    public static int MigrateToV2(RoadSegmentStatus v1)
    {
        var mapping = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, -5 },
            { -8, -5 }
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }

    public static int MigrateToV2(RoadSegmentMorphology v1)
    {
        var mapping = new Dictionary<int, int>
        {
            { 101, 1 },
            { 102, 2 },
            { 103, 3 },
            { 104, 7 },
            { 105, 3 },
            { 106, 3 },
            { 107, 5 },
            { 108, 5 },
            { 109, 4 },
            { 110, 3 },
            { 111, 6 },
            { 112, 6 },
            { 113, -113 },
            { 114, -114 },
            { 116, 11 },
            { 120, -120 },
            { 125, 8 },
            { 130, 12 },
            { -8, -8 }
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }

    public static int MigrateToV2(RoadSegmentAccessRestriction v1)
    {
        var mapping = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, -2 },
            { 3, -3 },
            { 4, 2 },
            { 5, 1 },
            { 6, 1 }
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }
}
