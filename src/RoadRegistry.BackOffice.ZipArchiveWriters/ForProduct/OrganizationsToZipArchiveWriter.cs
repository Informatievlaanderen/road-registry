namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using System.IO.Compression;
using System.Text;
using Abstractions.Organizations;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Dbase;
using Extracts.Dbase.Organizations.V2;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Product.Schema;

public class OrganizationsToZipArchiveWriter : IZipArchiveWriter<ProductContext>
{
    private readonly Encoding _encoding;
    private readonly string _entryFormat;
    private readonly IVersionedDbaseRecordReader<OrganizationDetail> _recordReader;

    public OrganizationsToZipArchiveWriter(string entryFormat, RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _entryFormat = entryFormat.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
        _recordReader = new OrganizationDbaseRecordReader(manager.ThrowIfNull(), encoding);
    }

    public async Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(context);

        //TODO-pr filter IsMaintainer when V2 is available
        var organizationsQuery = context.Organizations.AsQueryable();

        var dbfEntry = archive.CreateEntry(string.Format(_entryFormat, "LstOrg.dbf"));
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(await organizationsQuery.CountAsync(cancellationToken) + Organization.PredefinedTranslations.All.Length),
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

            foreach (var record in organizationsQuery.OrderBy(x => x.Code))
            {
                var organization = _recordReader.Read(record.DbaseRecord, record.DbaseSchemaVersion);

                dbfRecord.ORG.Value = organization.Code;
                dbfRecord.LBLORG.Value = organization.Name;
                dbfRecord.OVOCODE.Value = organization.OvoCode;

                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
