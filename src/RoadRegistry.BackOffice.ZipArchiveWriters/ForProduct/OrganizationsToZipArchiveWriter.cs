namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Product.Schema;
using Product.Schema.Organizations;

public class OrganizationsToZipArchiveWriter : IZipArchiveWriter<ProductContext>
{
    public OrganizationsToZipArchiveWriter(string entryFormat, RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _entryFormat = entryFormat ?? throw new ArgumentNullException(nameof(entryFormat));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    private readonly Encoding _encoding;
    private readonly string _entryFormat;
    private readonly RecyclableMemoryStreamManager _manager;

    public async Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var dbfEntry = archive.CreateEntry(string.Format(_entryFormat, "LstOrg.dbf"));
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(await context.Organizations.CountAsync(cancellationToken) + Organization.PredefinedTranslations.All.Length),
            OrganizationDbaseRecord.Schema
        );
        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter =
               new DbaseBinaryWriter(
                   dbfHeader,
                   new BinaryWriter(dbfEntryStream, _encoding, true)))
        {
            var dbfRecord = new OrganizationDbaseRecord();
            foreach (var predefined in Organization.PredefinedTranslations.All)
            {
                dbfRecord.ORG.Value = predefined.Identifier;
                dbfRecord.LBLORG.Value = predefined.Name;
                dbfWriter.Write(dbfRecord);
            }

            foreach (var data in context.Organizations.OrderBy(_ => _.SortableCode).Select(_ => _.DbaseRecord))
            {
                dbfRecord.FromBytes(data, _manager, _encoding);
                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
