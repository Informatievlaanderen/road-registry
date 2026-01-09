namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2.Writers;

using System.IO.Compression;
using System.Text;
using Core;
using Microsoft.IO;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Schemas.ExtractV2.RoadSegments;

public class RoadSegmentEuropeanRoadAttributesZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public RoadSegmentEuropeanRoadAttributesZipArchiveWriter(Encoding encoding)
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

        var roadSegments = await zipArchiveDataProvider.GetRoadSegments(
            request.Contour,
            cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.AttEuropweg;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var attributeId = new NextAttributeIdProvider(AttributeId.Initial);

        foreach (var featureType in featureTypes)
        {
            var records = roadSegments
                .SelectMany(x => x.EuropeanRoadNumbers.Select(number => (RoadSegment: x, number)))
                .OrderBy(x => x.RoadSegment.Id)
                .ThenBy(x => x.number)
                .Select(x =>
                {
                    var dbfRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                    {
                        EU_OIDN = { Value = attributeId.Next() },
                        WS_OIDN = { Value = x.RoadSegment.RoadSegmentId },
                        EUNUMMER = { Value = x.number },
                        BEGINTIJD = { Value = x.RoadSegment.Origin.Timestamp.ToBrusselsDateTime() },
                        BEGINORG = { Value = x.RoadSegment.Origin.OrganizationId },
                        //LBLBGNORG = {  }
                    };
                    return dbfRecord;
                });

            var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
            await dbaseRecordWriter.WriteToArchive(archive, extractFilename, featureType, RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema, records, cancellationToken);
        }
    }
}
