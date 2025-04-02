namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V1;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase;
using RoadRegistry.BackOffice.ZipArchiveWriters.Extensions;
using DbaseFileHeader = Be.Vlaanderen.Basisregisters.Shaperon.DbaseFileHeader;

public class TransactionZoneToZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private const ExtractFileName FileName = ExtractFileName.Transactiezones;

    public TransactionZoneToZipArchiveWriter(Encoding encoding)
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

        await CreateDbaseEntry(archive, FileName, [
            new TransactionZoneDbaseRecord
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
            }
        ], cancellationToken);

        var features = new List<IFeature>
        {
            new Feature((Geometry)request.Contour, new AttributesTable())
        };

        await archive.CreateShapeEntry(FileName, _encoding, features, WellKnownGeometryFactories.WithoutMAndZ, cancellationToken);
        await archive.CreateCpgEntry(FileName, _encoding, cancellationToken);
        await archive.CreateProjectionEntry(FeatureType.Change.ToProjectionFileName(FileName), _encoding, cancellationToken);
    }

    private async Task CreateDbaseEntry(ZipArchive archive, ExtractFileName fileName, ICollection<TransactionZoneDbaseRecord> dbfRecords, CancellationToken cancellationToken)
    {
        var dbfEntry = archive.CreateEntry(fileName.ToDbaseFileName());
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(dbfRecords.Count),
            TransactionZoneDbaseRecord.Schema
        );

        await using var dbfEntryStream = dbfEntry.Open();

        using var dbfWriter = new DbaseBinaryWriter(dbfHeader, new BinaryWriter(dbfEntryStream, _encoding, true));
        dbfWriter.Write(dbfRecords);
        dbfWriter.Writer.Flush();

        await dbfEntryStream.FlushAsync(cancellationToken);
    }
}
