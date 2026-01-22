namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Schemas.DomainV2.GradeSeparatedJuntions;

public class GradeSeparatedJunctionZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public GradeSeparatedJunctionZipArchiveWriter(Encoding encoding)
    {
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

        var junctions = await zipArchiveDataProvider.GetGradeSeparatedJunctions(
            request.Contour, cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.RltOgkruising;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        foreach (var featureType in featureTypes)
        {
            var records = junctions
                .OrderBy(x => x.Id)
                .Select(x => new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = x.GradeSeparatedJunctionId },
                    BO_WS_OIDN = { Value = x.UpperRoadSegmentId },
                    ON_WS_OIDN = { Value = x.LowerRoadSegmentId },
                    TYPE = { Value = x.IsV2 ? GradeSeparatedJunctionTypeV2.Parse(x.Type).Translation.Identifier : MigrateToV2(GradeSeparatedJunctionType.Parse(x.Type)) },
                    BEGINTIJD = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                    BEGINORG = { Value = x.Origin.OrganizationId }
                });

            var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
            await dbaseRecordWriter.WriteToArchive(archive, extractFilename, featureType, GradeSeparatedJunctionDbaseRecord.Schema, records, cancellationToken);
        }
    }

    public static int MigrateToV2(GradeSeparatedJunctionType v1)
    {
        var mapping = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { -8, -8 }
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }
}
