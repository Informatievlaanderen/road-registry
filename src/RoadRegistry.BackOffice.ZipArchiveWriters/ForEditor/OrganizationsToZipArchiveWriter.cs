namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForEditor;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Editor.Schema;
using Extracts.Dbase.Organizations.V2;
using Microsoft.EntityFrameworkCore;

public class OrganizationsToZipArchiveWriter : IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;

    public OrganizationsToZipArchiveWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var dbfEntry = archive.CreateEntry("LstOrg.dbf");
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(await context.OrganizationsV2.Where(x => x.IsMaintainer).CountAsync(cancellationToken) + Organization.PredefinedTranslations.All.Length),
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

            foreach (var record in context.OrganizationsV2.Where(x => x.IsMaintainer).OrderBy(x => x.Code))
            {
                dbfRecord.ORG.Value = record.Code;
                dbfRecord.LBLORG.Value = record.Name;
                dbfRecord.OVOCODE.Value = record.OvoCode;

                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }
    }
}
