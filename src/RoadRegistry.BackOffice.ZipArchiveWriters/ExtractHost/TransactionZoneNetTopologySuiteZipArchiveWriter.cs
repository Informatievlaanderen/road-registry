namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Extensions;
using Extracts;
using Extracts.Dbase;
using NetTopologySuite.Geometries;

public class TransactionZoneNetTopologySuiteZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private const ExtractFileName FileName = ExtractFileName.Transactiezones;

    public TransactionZoneNetTopologySuiteZipArchiveWriter(Encoding encoding)
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

        var dbaseRecord = new TransactionZoneDbaseRecord
        {
            SOURCEID = { Value = 1 },
            TYPE = { Value = 2 },
            BESCHRIJV =
            {
                Value = string.IsNullOrEmpty(request.ExtractDescription) ? request.ExternalRequestId : request.ExtractDescription
            },
            OPERATOR = { Value = "" },
            ORG = { Value = "AGIV" },
            APPLICATIE = { Value = "Wegenregister" },
            DOWNLOADID = { Value = request.DownloadId.ToGuid().ToString("N") }
        };

        //TODO-pr hoe wegschrijven gebruik makende van WellKnownGeometryFactories.WithoutMAndZ?

        var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
        await dbaseRecordWriter.WriteToArchive(archive, FileName, FeatureType.Change, TransactionZoneDbaseRecord.Schema, [
            (dbaseRecord, (Geometry)request.Contour)
        ], cancellationToken);
    }
}
