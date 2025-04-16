namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;

using System.IO.Compression;
using System.Text;
using Dbase.V2;
using Microsoft.IO;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;

public class RoadSegmentLaneAttributesZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadSegmentLaneAttributesZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _manager = manager.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveDataProvider);

        var attributes = await zipArchiveDataProvider.GetRoadSegmentLaneAttributes(
            request.Contour,
            cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.AttRijstroken;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        foreach (var featureType in featureTypes)
        {
            var records = attributes
                .OrderBy(x => x.Id)
                .Select(x =>
                {
                    var dbfRecord = new RoadSegmentLaneAttributeDbaseRecord();
                    dbfRecord.FromBytes(x.DbaseRecord, _manager, _encoding);
                    return dbfRecord;
                });

            var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
            await dbaseRecordWriter.WriteToArchive(archive, extractFilename, featureType, RoadSegmentLaneAttributeDbaseRecord.Schema, records, cancellationToken);
        }
    }
}
