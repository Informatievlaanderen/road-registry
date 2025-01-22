namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Editor.Schema.RoadNodes;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadNodes;
using FeatureCompare;
using Microsoft.IO;
using NetTopologySuite.Geometries;

public class RoadNodesToZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadNodesToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
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
        FeatureType[] featureTypes = [FeatureType.Extract, FeatureType.Change];

        foreach (var featureType in featureTypes)
        {
            var dbfEntry = archive.CreateEntry(featureType.ToDbaseFileName(extractFilename));
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(nodes.Count),
                RoadNodeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                   new DbaseBinaryWriter(
                       dbfHeader,
                       new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadNodeDbaseRecord();
                foreach (var data in nodes.OrderBy(record => record.Id).Select(record => record.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }

                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }

            var shpBoundingBox =
                nodes.Aggregate(
                    BoundingBox3D.Empty,
                    (box, record) => box.ExpandWith(record.GetBoundingBox().ToBoundingBox3D()));

            var shpEntry = archive.CreateEntry(featureType.ToShapeFileName(extractFilename));
            var shpHeader = new ShapeFileHeader(
                new WordLength(
                    nodes.Aggregate(0, (length, record) => length + record.ShapeRecordContentLength)),
                ShapeType.Point,
                shpBoundingBox);
            await using (var shpEntryStream = shpEntry.Open())
            using (var shpWriter =
                   new ShapeBinaryWriter(
                       shpHeader,
                       new BinaryWriter(shpEntryStream, _encoding, true)))
            {
                var number = RecordNumber.Initial;
                foreach (var data in nodes.OrderBy(x => x.Id).Select(x => x.ShapeRecordContent))
                {
                    shpWriter.Write(
                        ShapeContentFactory
                            .FromBytes(data, _manager, _encoding)
                            .RecordAs(number)
                    );
                    number = number.Next();
                }

                shpWriter.Writer.Flush();
                await shpEntryStream.FlushAsync(cancellationToken);
            }

            var shxEntry = archive.CreateEntry(featureType.ToShapeIndexFileName(extractFilename));
            var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(nodes.Count));
            await using (var shxEntryStream = shxEntry.Open())
            using (var shxWriter =
                   new ShapeIndexBinaryWriter(
                       shxHeader,
                       new BinaryWriter(shxEntryStream, _encoding, true)))
            {
                var offset = ShapeIndexRecord.InitialOffset;
                var number = RecordNumber.Initial;
                foreach (var data in nodes.OrderBy(x => x.Id).Select(x => x.ShapeRecordContent))
                {
                    var shpRecord = ShapeContentFactory
                        .FromBytes(data, _manager, _encoding)
                        .RecordAs(number);
                    shxWriter.Write(shpRecord.IndexAt(offset));
                    number = number.Next();
                    offset = offset.Plus(shpRecord.Length);
                }

                shxWriter.Writer.Flush();
                await shxEntryStream.FlushAsync(cancellationToken);
            }

            await archive.CreateCpgEntry(featureType.ToCpgFileName(extractFilename), _encoding, cancellationToken);
            await archive.CreateProjectionEntry(featureType.ToProjectionFileName(extractFilename), _encoding, cancellationToken);
        }
    }
}
