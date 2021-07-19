namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Editor.Schema;
    using Editor.Schema.Extracts;
    using Editor.Schema.RoadNodes;
    using Extracts;
    using Microsoft.IO;

    public class TransactionZoneToZipArchiveWriter : IZipArchiveWriter<EditorContext>
    {
        private readonly Encoding _encoding;

        public TransactionZoneToZipArchiveWriter(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request,
            EditorContext context,
            CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var dbfEntry = archive.CreateEntry("eTransactiezones.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(1),
                RoadNodeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new TransactionZoneDbaseRecord
                {
                    SOURCEID = {Value = 1},
                    TYPE = {Value = 2},
                    BESCHRIJV =
                    {
                        Value =
                            $"Extract[DownloadId={request.DownloadId.ToGuid():N};RequestId={request.ExternalRequestId.ToString()}]"
                    },
                    OPERATOR = {Value = ""},
                    ORG = {Value = "AGIV"},
                    APPLICATIE = {Value = "Wegenregister"}
                };
                dbfWriter.Write(dbfRecord);
                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }

            var polygon = PolygonalGeometryTranslator.FromGeometry(request.Contour);
            var shapeContent = new PolygonShapeContent(polygon);
            var shpBoundingBox = BoundingBox3D.FromGeometry(polygon);

            var shpEntry = archive.CreateEntry("eTransactiezones.shp");
            var shpHeader = new ShapeFileHeader(
                shapeContent.ContentLength,
                ShapeType.Polygon,
                shpBoundingBox);
            await using (var shpEntryStream = shpEntry.Open())
            using (var shpWriter =
                new ShapeBinaryWriter(
                    shpHeader,
                    new BinaryWriter(shpEntryStream, _encoding, true)))
            {
                shpWriter.Write(shapeContent.RecordAs(RecordNumber.Initial));
                shpWriter.Writer.Flush();
                await shpEntryStream.FlushAsync(cancellationToken);
            }

            var shxEntry = archive.CreateEntry("eTransactiezones.shx");
            var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(1));
            await using (var shxEntryStream = shxEntry.Open())
            using (var shxWriter =
                new ShapeIndexBinaryWriter(
                    shxHeader,
                    new BinaryWriter(shxEntryStream, _encoding, true)))
            {
                shxWriter.Write(shapeContent.RecordAs(RecordNumber.Initial).IndexAt(ShapeIndexRecord.InitialOffset));
                shxWriter.Writer.Flush();
                await shxEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
