namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
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
    using Editor.Schema.RoadSegments;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;

    public class RoadSegmentsToZipArchiveWriter : IZipArchiveWriter
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

        public RoadSegmentsToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var count = await context.RoadSegments.CountAsync(cancellationToken);
            var dbfEntry = archive.CreateEntry("Wegsegment.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(count),
                RoadSegmentDbaseRecord.Schema
            );
            using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadSegmentDbaseRecord();
                foreach (var data in context.RoadSegments.OrderBy(_ => _.Id).Select(_ => _.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }
                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }

            var shpBoundingBox =
                (await context.RoadSegmentBoundingBox.SingleOrDefaultAsync(cancellationToken))?.ToBoundingBox3D()
                ?? new BoundingBox3D(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            var info = await context.RoadNetworkInfo.SingleAsync(cancellationToken);

            var shpEntry = archive.CreateEntry("Wegsegment.shp");
            var shpHeader = new ShapeFileHeader(
                new WordLength(info.TotalRoadSegmentShapeLength),
                ShapeType.PolyLineM,
                shpBoundingBox);
            using (var shpEntryStream = shpEntry.Open())
            using (var shpWriter =
                new ShapeBinaryWriter(
                    shpHeader,
                    new BinaryWriter(shpEntryStream, _encoding, true)))
            {
                var number = RecordNumber.Initial;
                foreach (var data in context.RoadSegments.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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

            var shxEntry = archive.CreateEntry("Wegsegment.shx");
            var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(count));
            using (var shxEntryStream = shxEntry.Open())
            using (var shxWriter =
                new ShapeIndexBinaryWriter(
                    shxHeader,
                    new BinaryWriter(shxEntryStream, _encoding, true)))
            {
                var offset = ShapeIndexRecord.InitialOffset;
                var number = RecordNumber.Initial;
                foreach (var data in context.RoadSegments.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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
