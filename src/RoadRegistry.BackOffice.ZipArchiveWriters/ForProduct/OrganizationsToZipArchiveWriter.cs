namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Extracts.Dbase.Organizations;
using Microsoft.EntityFrameworkCore;
using Product.Schema;
using RoadRegistry.Extensions;

public class OrganizationsToZipArchiveWriter : IZipArchiveWriter<ProductContext>
{
    private readonly Encoding _encoding;
    private readonly string _entryFormat;

    public OrganizationsToZipArchiveWriter(string entryFormat, Encoding encoding)
    {
        _entryFormat = entryFormat.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(context);

        var organizationsQuery = context.OrganizationsV2.Where(x => x.IsMaintainer);

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

            foreach (var organization in organizationsQuery.OrderBy(x => x.Code))
            {
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
