namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments;

public class RoadSegmentNationalRoadAttributesZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public RoadSegmentNationalRoadAttributesZipArchiveWriter(Encoding encoding)
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

        var roadSegments = await zipArchiveDataProvider.GetRoadSegments(
            request.Contour,
            cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.AttNationweg;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var attributeId = new NextAttributeIdProvider(AttributeId.Initial);

        foreach (var featureType in featureTypes)
        {
            var records = roadSegments
                .SelectMany(x => x.NationalRoadNumbers.Select(number => (RoadSegment: x, number)))
                .OrderBy(x => x.RoadSegment.Id)
                .ThenBy(x => x.number)
                .Select(x =>
                {
                    var dbfRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                    {
                        NW_OIDN = { Value = attributeId.Next() },
                        WS_TEMPID = { Value = x.RoadSegment.RoadSegmentId }, //TODO-pr implement WS_TEMPID
                        NWNUMMER = { Value = x.number },
                        CREATIE = { Value = x.RoadSegment.Origin.Timestamp.ToBrusselsDateTime() }
                    };
                    return dbfRecord;
                });

            var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
            await dbaseRecordWriter.WriteToArchive(archive, extractFilename, featureType, RoadSegmentNationalRoadAttributeDbaseRecord.Schema, records, cancellationToken);
        }
    }
}
