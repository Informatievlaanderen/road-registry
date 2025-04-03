namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using Extracts.Dbase.RoadNodes;
using Microsoft.IO;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class RoadNodesZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadNodesZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
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

        var nodes = await zipArchiveDataProvider.GetRoadNodes(request.Contour, cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.Wegknoop;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var dbaseRecordWriter = new DbaseRecordWriter(_encoding);

        foreach (var featureType in featureTypes)
        {
            var records = nodes
                .OrderBy(record => record.Id)
                .Select(node =>
                {
                    var dbfRecord = new RoadNodeDbaseRecord();
                    dbfRecord.FromBytes(node.DbaseRecord, _manager, _encoding);

                    return ((DbaseRecord)dbfRecord, node.Geometry);
                });

            await dbaseRecordWriter.WriteToArchive(archive, extractFilename, featureType, RoadNodeDbaseRecord.Schema, ShapeType.Point, records, cancellationToken);
        }
    }
}
