namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Dbase;
using ExtractHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Product.Schema;
using RoadRegistry.BackOffice.Extracts.Dbase.Organizations.V2;

public class OrganizationsToZipArchiveWriter : ZipArchiveWriters.IZipArchiveWriter<ProductContext>
{
    private readonly Encoding _encoding;
    private readonly string _entryFormat;
    private readonly IVersionedDbaseRecordReader<Organization> _recordReader;

    public OrganizationsToZipArchiveWriter(string entryFormat, RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _entryFormat = entryFormat.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
        _recordReader = new OrganizationDbaseRecordConverter(manager.ThrowIfNull(), encoding);
    }

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

            foreach (var record in context.Organizations.OrderBy(_ => _.SortableCode))
            {
                var organization = _recordReader.Read(record.DbaseRecord, record.DbaseSchemaVersion);

                dbfRecord.ORG.Value = organization.Translation.Identifier;
                dbfRecord.LBLORG.Value = organization.Translation.Name;
                dbfRecord.OVOCODE.Value = organization.OvoCode;

                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
