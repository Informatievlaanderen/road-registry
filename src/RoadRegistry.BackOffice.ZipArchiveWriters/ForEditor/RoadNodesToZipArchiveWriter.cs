namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForEditor;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Extracts.Dbase.RoadNodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;

public class RoadNodesToZipArchiveWriter : IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadNodesToZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var count = await context.RoadNodes.CountAsync(cancellationToken);
        var dbfEntry = archive.CreateEntry("Wegknoop.dbf");
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(count),
            RoadNodeDbaseRecord.Schema
        );
        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter =
               new DbaseBinaryWriter(
                   dbfHeader,
                   new BinaryWriter(dbfEntryStream, _encoding, true)))
        {
            var dbfRecord = new RoadNodeDbaseRecord();
            foreach (var data in context.RoadNodes.OrderBy(_ => _.Id).Select(_ => _.DbaseRecord))
            {
                dbfRecord.FromBytes(data, _manager, _encoding);
                dbfWriter.Write(dbfRecord);
            }

            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }

        var shpBoundingBox = count > 0
            ? (await context.RoadNodeBoundingBox.SingleAsync(cancellationToken)).ToBoundingBox3D()
            : BoundingBox3D.Empty;

        var info = await context.RoadNetworkInfo.SingleAsync(cancellationToken);

        var shpEntry = archive.CreateEntry("Wegknoop.shp");
        var shpHeader = new ShapeFileHeader(
            new WordLength(info.TotalRoadNodeShapeLength),
            ShapeType.Point,
            shpBoundingBox);
        await using (var shpEntryStream = shpEntry.Open())
        using (var shpWriter =
               new ShapeBinaryWriter(
                   shpHeader,
                   new BinaryWriter(shpEntryStream, _encoding, true)))
        {
            var number = RecordNumber.Initial;
            foreach (var data in context.RoadNodes.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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

        var shxEntry = archive.CreateEntry("Wegknoop.shx");
        var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(count));
        await using (var shxEntryStream = shxEntry.Open())
        using (var shxWriter =
               new ShapeIndexBinaryWriter(
                   shxHeader,
                   new BinaryWriter(shxEntryStream, _encoding, true)))
        {
            var offset = ShapeIndexRecord.InitialOffset;
            var number = RecordNumber.Initial;
            foreach (var data in context.RoadNodes.OrderBy(_ => _.Id).Select(_ => _.ShapeRecordContent))
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