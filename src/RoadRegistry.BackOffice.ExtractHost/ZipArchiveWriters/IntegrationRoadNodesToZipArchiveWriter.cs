namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Editor.Schema;
    using Editor.Schema.RoadNodes;
    using Extracts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using NetTopologySuite.Operation.Buffer;

    public class IntegrationRoadNodesToZipArchiveWriter : IZipArchiveWriter<EditorContext>
    {
        private readonly RecyclableMemoryStreamManager _manager;

        private readonly Encoding _encoding;

        public IntegrationRoadNodesToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request,
            EditorContext context,
            CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (context == null) throw new ArgumentNullException(nameof(context));

            const int integrationBufferInMeters = 350;
            var requestedContour = (NetTopologySuite.Geometries.Geometry) request.Contour;
            var contourWithIntegrationBuffer = requestedContour.Buffer(integrationBufferInMeters);
            var integrationBuffer = contourWithIntegrationBuffer.Difference(requestedContour);

            var nodesInContour =
                await context.RoadNodes.InsideContour(request.Contour).ToListAsync(cancellationToken);
            var nodesInIntegrationBuffer =
                await context.RoadNodes.InsideContour(integrationBuffer as NetTopologySuite.Geometries.IPolygonal).ToListAsync(cancellationToken);
            var integrationNodes = nodesInIntegrationBuffer.Except(nodesInContour, new RoadNodeRecordEqualityComparerById()).ToList();


            var dbfEntry = archive.CreateEntry("iWegknoop.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(integrationNodes.Count),
                RoadNodeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadNodeDbaseRecord();
                foreach (var data in integrationNodes.OrderBy(_ => _.Id).Select(_ => _.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }
                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }

            var shpBoundingBox =
                integrationNodes.Aggregate(
                    BoundingBox3D.Empty,
                    (box, record) => box.ExpandWith(record.BoundingBox.ToBoundingBox3D()));

            var shpEntry = archive.CreateEntry("iWegknoop.shp");
            var shpHeader = new ShapeFileHeader(
                new WordLength(
                    integrationNodes.Aggregate(0, (length, record) => length + record.ShapeRecordContentLength)),
                ShapeType.Point,
                shpBoundingBox);
            await using (var shpEntryStream = shpEntry.Open())
            using (var shpWriter =
                new ShapeBinaryWriter(
                    shpHeader,
                    new BinaryWriter(shpEntryStream, _encoding, true)))
            {
                var number = RecordNumber.Initial;
                foreach (var data in integrationNodes.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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

            var shxEntry = archive.CreateEntry("iWegknoop.shx");
            var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(integrationNodes.Count));
            await using (var shxEntryStream = shxEntry.Open())
            using (var shxWriter =
                new ShapeIndexBinaryWriter(
                    shxHeader,
                    new BinaryWriter(shxEntryStream, _encoding, true)))
            {
                var offset = ShapeIndexRecord.InitialOffset;
                var number = RecordNumber.Initial;
                foreach (var data in integrationNodes.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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
        }
    }
}


