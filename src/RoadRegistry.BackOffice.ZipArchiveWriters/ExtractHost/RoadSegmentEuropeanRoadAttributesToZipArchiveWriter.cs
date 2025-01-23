namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using Extracts.Dbase.RoadSegments;
using FeatureCompare;
using Microsoft.IO;

public class RoadSegmentEuropeanRoadAttributesToZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(RecyclableMemoryStreamManager manager,
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
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (zipArchiveDataProvider == null) throw new ArgumentNullException(nameof(zipArchiveDataProvider));

        var attributes = await zipArchiveDataProvider.GetRoadSegmentEuropeanRoadAttributes(
            request.Contour,
            cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.AttEuropweg;
        FeatureType[] featureTypes = [FeatureType.Extract, FeatureType.Change];

        foreach (var featureType in featureTypes)
        {
            var dbfEntry = archive.CreateEntry(extractFilename.ToDbaseFileName(featureType));
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(attributes.Count),
                RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                   new DbaseBinaryWriter(
                       dbfHeader,
                       new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord();
                foreach (var data in attributes.OrderBy(x => x.Id).Select(x => x.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }

                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
