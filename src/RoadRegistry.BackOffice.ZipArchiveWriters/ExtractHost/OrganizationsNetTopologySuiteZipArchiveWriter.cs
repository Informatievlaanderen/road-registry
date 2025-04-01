namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Core;
using Extracts;
using Extracts.Dbase.Organizations;

public class OrganizationsNetTopologySuiteZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public OrganizationsNetTopologySuiteZipArchiveWriter(Encoding encoding)
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

        var dbaseRecords = new List<OrganizationDbaseRecord>();

        foreach (var predefined in Organization.PredefinedTranslations.All)
        {
            var dbfRecord = new OrganizationDbaseRecord();
            dbfRecord.ORG.Value = predefined.Identifier;
            dbfRecord.LBLORG.Value = predefined.Name;
            dbaseRecords.Add(dbfRecord);
        }

        foreach (var organization in await zipArchiveDataProvider.GetOrganizations(cancellationToken))
        {
            var dbfRecord = new OrganizationDbaseRecord();
            dbfRecord.ORG.Value = organization.Code;
            dbfRecord.LBLORG.Value = organization.Name;
            dbfRecord.OVOCODE.Value = organization.OvoCode;
            dbaseRecords.Add(dbfRecord);
        }

        var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
        await dbaseRecordWriter.WriteToArchive(archive, "eLstOrg.dbf", OrganizationDbaseRecord.Schema, dbaseRecords, cancellationToken);
    }
}
