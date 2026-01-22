namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Extracts.Schemas.DomainV2;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

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
        ZipArchiveWriteContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveDataProvider);

        var dbaseRecord = new TransactionZoneDbaseRecord
        {
            TYPE = { Value = 2 },
            BESCHRIJV = { Value = request.ExtractDescription },
            DOWNLOADID = { Value = request.DownloadId.ToGuid().ToString("N") }
        };

        var writer = new ShapeFileRecordWriter(_encoding);
        await writer.WriteToArchive(archive, FileName, FeatureType.Change, ShapeType.Polygon, TransactionZoneDbaseRecord.Schema, [
            (dbaseRecord, (Geometry)request.Contour)
        ], cancellationToken);
    }
}
