namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Extracts;
using Extracts.Dbase.RoadSegments;
using Microsoft.IO;

public class RoadSegmentNumberedRoadAttributesNetTopologySuiteZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadSegmentNumberedRoadAttributesNetTopologySuiteZipArchiveWriter(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
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

        var attributes = await zipArchiveDataProvider.GetRoadSegmentNumberedRoadAttributes(
            request.Contour,
            cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.AttGenumweg;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        foreach (var featureType in featureTypes)
        {
            var records = attributes
                .OrderBy(x => x.Id)
                .Select(x =>
                {
                    var dbfRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord();
                    dbfRecord.FromBytes(x.DbaseRecord, _manager, _encoding);
                    return dbfRecord;
                });

            var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
            await dbaseRecordWriter.WriteToArchive(archive, extractFilename, featureType, RoadSegmentNumberedRoadAttributeDbaseRecord.Schema, records, cancellationToken);
        }
    }
}
