namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Extracts;
using Extracts.Dbase.GradeSeparatedJuntions;
using Microsoft.IO;

public class GradeSeparatedJunctionNetTopologySuiteZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public GradeSeparatedJunctionNetTopologySuiteZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
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
                .Select(x =>
                {
                    var dbfRecord = new GradeSeparatedJunctionDbaseRecord();
                    dbfRecord.FromBytes(x.DbaseRecord, _manager, _encoding);
                    return dbfRecord;
                });

            var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
            await dbaseRecordWriter.WriteToArchive(archive, extractFilename, featureType, GradeSeparatedJunctionDbaseRecord.Schema, records, cancellationToken);
        }
    }
}
