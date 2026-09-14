namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Product.Schema;
using RoadRegistry.Extracts.Schemas.ExtractV1.GradeSeparatedJuntions;

public class GradeSeparatedJunctionArchiveWriter : IZipArchiveWriter<ProductContext>
{
    private readonly Encoding _encoding;
    private readonly string _entryFormat;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly ProductInwinningFilter _inwinningFilter;

    public GradeSeparatedJunctionArchiveWriter(string entryFormat, RecyclableMemoryStreamManager manager, Encoding encoding, ProductInwinningFilter? inwinningFilter = null)
    {
        _entryFormat = entryFormat ?? throw new ArgumentNullException(nameof(entryFormat));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _inwinningFilter = inwinningFilter ?? ProductInwinningFilter.None;
    }

    public async Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        // A junction names the road segments it crosses at only in its dbase record.
        var excludedIds = new HashSet<int>();
        if (_inwinningFilter.ExcludesAnything)
        {
            var junctionRecord = new GradeSeparatedJunctionDbaseRecord();
            foreach (var junction in context.GradeSeparatedJunctions.Select(_ => new { _.Id, _.DbaseRecord }))
            {
                junctionRecord.FromBytes(junction.DbaseRecord, _manager, _encoding);
                if (_inwinningFilter.ExcludesGradeSeparatedJunction(junctionRecord.BO_WS_OIDN.Value, junctionRecord.ON_WS_OIDN.Value))
                {
                    excludedIds.Add(junction.Id);
                }
            }
        }

        var count = await context.GradeSeparatedJunctions.CountAsync(cancellationToken) - excludedIds.Count;
        var dbfEntry = archive.CreateEntry(string.Format(_entryFormat, "RltOgkruising.dbf"));
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(count),
            GradeSeparatedJunctionDbaseRecord.Schema
        );
        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter =
               new DbaseBinaryWriter(
                   dbfHeader,
                   new BinaryWriter(dbfEntryStream, _encoding, true)))
        {
            var dbfRecord = new GradeSeparatedJunctionDbaseRecord();
            foreach (var data in context.GradeSeparatedJunctions
                         .OrderBy(_ => _.Id)
                         .Select(_ => new { _.Id, _.DbaseRecord })
                         .AsEnumerable()
                         .Where(_ => !excludedIds.Contains(_.Id))
                         .Select(_ => _.DbaseRecord))
            {
                dbfRecord.FromBytes(data, _manager, _encoding);
                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
