namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadSegments;
using Microsoft.IO;

public class RoadSegmentWidthAttributesToZipArchiveWriter : IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadSegmentWidthAttributesToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request,
        EditorContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var dbfRecords = (await context.RoadSegmentWidthAttributes
            .ToListWithPolygonials(request.Contour,
                (dbSet, polygon) => dbSet.InsideContour(polygon),
                x => x.Id,
                cancellationToken))
            .Select(attribute =>
            {
                var dbfRecord = new RoadSegmentWidthAttributeDbaseRecord();
                dbfRecord.FromBytes(attribute.DbaseRecord, _manager, _encoding);
                return dbfRecord;
            })
            .ToList();

        await WriteAsync(archive, "eAttWegbreedte.dbf", dbfRecords, cancellationToken);
    }

    public async Task WriteAsync(ZipArchive archive, string fileName,
        ICollection<RoadSegmentWidthAttributeDbaseRecord> records,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(records);
        
        var dbfEntry = archive.CreateEntry(fileName);
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(records.Count),
            RoadSegmentWidthAttributeDbaseRecord.Schema
        );
        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter =
               new DbaseBinaryWriter(
                   dbfHeader,
                   new BinaryWriter(dbfEntryStream, _encoding, true)))
        {
            foreach (var dbfRecord in records.OrderBy(_ => _.WB_OIDN.Value))
            {
                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
