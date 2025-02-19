namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Extracts;
using Extracts.Dbase.Organizations;

public class OrganizationsToZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public OrganizationsToZipArchiveWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
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

        var dbfEntry = archive.CreateEntry("eLstOrg.dbf");
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(await zipArchiveDataProvider.GetOrganizationsCount(cancellationToken) + Organization.PredefinedTranslations.All.Length),
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

            foreach (var organization in await zipArchiveDataProvider.GetOrganizations(cancellationToken))
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
