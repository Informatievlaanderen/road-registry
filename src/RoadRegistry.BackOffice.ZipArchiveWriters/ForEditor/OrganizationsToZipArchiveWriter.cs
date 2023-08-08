namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForEditor;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Dbase;
using Editor.Schema;
using ExtractHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using RoadRegistry.BackOffice.Abstractions.Organizations;
using RoadRegistry.BackOffice.Extracts.Dbase.Organizations.V2;

public class OrganizationsToZipArchiveWriter : ZipArchiveWriters.IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;
    private readonly IVersionedDbaseRecordReader<OrganizationDetail> _recordReader;

    public OrganizationsToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
        _recordReader = new OrganizationDbaseRecordReader(manager.ThrowIfNull(), encoding);
    }

    public async Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var dbfEntry = archive.CreateEntry("LstOrg.dbf");
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
