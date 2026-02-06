namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.Inwinning;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Infrastructure.ShapeFile;
using NetTopologySuite.Geometries;
using Projections;
using RoadSegment.ValueObjects;
using Schemas.Inwinning.RoadSegments;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class RoadSegmentsZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public RoadSegmentsZipArchiveWriter(Encoding encoding)
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

        var segments = await zipArchiveDataProvider.GetRoadSegments(request.Contour, cancellationToken);
        var records = ConvertToDbaseRecords(segments, context);

        const ExtractFileName extractFilename = ExtractFileName.Wegsegment;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var writer = new Lambert08ShapeFileRecordWriter(_encoding);

        foreach (var featureType in featureTypes)
        {
            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.PolyLine, RoadSegmentDbaseRecord.Schema, records, cancellationToken);
        }
    }

    internal static IReadOnlyCollection<(DbaseRecord, Geometry)> ConvertToDbaseRecords(IEnumerable<RoadSegmentExtractItem> segments, ZipArchiveWriteContext context)
    {
        return segments
            .OrderBy(x => x.Id)
            .SelectMany(roadSegment =>
            {
                return roadSegment.Flatten()
                    .Select(x =>
                    {
                        var status = x.IsV2 ? RoadSegmentStatusV2.Parse(x.Status).Translation.Identifier : MigrateToV2(RoadSegmentStatus.Parse(x.Status));
                        var morphology = x.IsV2 ? RoadSegmentMorphologyV2.Parse(x.Morphology).Translation.Identifier : MigrateToV2(RoadSegmentMorphology.Parse(x.Morphology));
                        var accessRestriction = x.IsV2 ? RoadSegmentAccessRestrictionV2.Parse(x.AccessRestriction).Translation.Identifier : MigrateToV2(RoadSegmentAccessRestriction.Parse(x.AccessRestriction));
                        var category = x.IsV2 ? RoadSegmentCategoryV2.Parse(x.Category).Translation.Identifier : MigrateToV2(RoadSegmentCategory.Parse(x.Category));
                        var surfaceType = x.IsV2 ? RoadSegmentSurfaceTypeV2.Parse(x.SurfaceType).Translation.Identifier : MigrateToV2(RoadSegmentSurfaceType.Parse(x.SurfaceType));

                        var dbfRecord = new RoadSegmentDbaseRecord
                        {
                            WS_TEMPID = { Value = context.NewTempId(x.RoadSegmentId, x.Geometry) },
                            WS_OIDN = { Value = x.RoadSegmentId },
                            STATUS = { Value = status },
                            MORF = { Value = morphology },
                            WEGCAT = { Value = category },
                            LSTRNMID = { Value = x.LeftStreetNameId },
                            RSTRNMID = { Value = x.RightStreetNameId },
                            LBEHEER = { Value = x.LeftMaintenanceAuthorityId },
                            RBEHEER = { Value = x.RightMaintenanceAuthorityId },
                            TOEGANG = { Value = accessRestriction },
                            VERHARDING = { Value = surfaceType },
                            AUTOHEEN = { Value = x.CarAccess is not null ? (x.CarAccess == VehicleAccess.Forward || x.CarAccess == VehicleAccess.BiDirectional).ToDbaseShortValue() : null },
                            AUTOTERUG = { Value = x.CarAccess is not null ? (x.CarAccess == VehicleAccess.Backward || x.CarAccess == VehicleAccess.BiDirectional).ToDbaseShortValue() : null },
                            FIETSHEEN = { Value = x.BikeAccess is not null ? (x.BikeAccess == VehicleAccess.Forward || x.BikeAccess == VehicleAccess.BiDirectional).ToDbaseShortValue() : null },
                            FIETSTERUG = { Value = x.BikeAccess is not null ? (x.BikeAccess == VehicleAccess.Backward || x.BikeAccess == VehicleAccess.BiDirectional).ToDbaseShortValue() : null },
                            VOETGANGER = { Value = x.PedestrianAccess?.ToDbaseShortValue() },

                            CREATIE = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                            VERSIE = { Value = x.LastModified.Timestamp.ToBrusselsDateTime() }
                        };

                        return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.Value);
                    });
            })
            .ToList();
    }

    private static int MigrateToV2(RoadSegmentStatus v1)
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

    private static int MigrateToV2(RoadSegmentMorphology v1)
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

    private static int MigrateToV2(RoadSegmentAccessRestriction v1)
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

    private static string MigrateToV2(RoadSegmentCategory v1)
    {
        var mapping = new Dictionary<string, string>
        {
            { "EHW", "EHW" },
            { "VHW", "VHW" },
            { "RW", "RW" },
            { "IW", "IW" },
            { "OW", "OW" },
            { "EW", "EW" },
            { "-8", "-8" },
            { "-9", "-9" },

            //obsolete values, only a problem for tst/stg
            { "L", "OW" },
            { "L1", "EW" },
            { "L2", "EW" },
            { "L3", "EW" },
            { "H", "EHW" },
            { "PI", "VHW" },
            { "PII", "VHW" },
            { "PII-1", "VHW" },
            { "PII-2", "VHW" },
            { "PII-3", "VHW" },
            { "PII-4", "VHW" },
            { "S", "RW" },
            { "S1", "IW" },
            { "S2", "IW" },
            { "S3", "IW" },
            { "S4", "IW" }
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }

    private static int MigrateToV2(RoadSegmentSurfaceType v1)
    {
        var mapping = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { -9, 3 },
            { -8, -8 },
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }
}
