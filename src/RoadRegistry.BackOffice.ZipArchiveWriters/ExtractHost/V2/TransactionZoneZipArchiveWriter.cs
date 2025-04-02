namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;

using System.IO.Compression;
using System.Text;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase;

public class TransactionZoneZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private const ExtractFileName FileName = ExtractFileName.Transactiezones;

    public TransactionZoneZipArchiveWriter(Encoding encoding)
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

        //TODO-pr hoe wegschrijven gebruik makende van WellKnownGeometryFactories.WithoutMAndZ? -> op voorhand cleanup doen

        var dbaseRecordWriter = new DbaseRecordWriter(_encoding);
        await dbaseRecordWriter.WriteToArchive(archive, FileName, FeatureType.Change, TransactionZoneDbaseRecord.Schema, [
            (dbaseRecord, (Geometry)request.Contour)
        ], cancellationToken);
    }
}
