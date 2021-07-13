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
    using Editor.Schema.RoadSegments;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;

    public class RoadSegmentNumberedRoadAttributesToZipArchiveWriter : IZipArchiveWriter<EditorContext>
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

        public RoadSegmentNumberedRoadAttributesToZipArchiveWriter(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, MultiPolygon contour, EditorContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (contour == null) throw new ArgumentNullException(nameof(contour));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var attributes = await context.RoadSegmentNumberedRoadAttributes
                .Where(attribute => context.RoadSegments.Any(segment =>
                    segment.Geometry.Intersects(contour) && attribute.RoadSegmentId == segment.Id))
                .ToListAsync(cancellationToken);
            var dbfEntry = archive.CreateEntry("AttGenumweg.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(attributes.Count),
                RoadSegmentNumberedRoadAttributeDbaseRecord.Schema
            );
            await using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord();
                foreach (var data in attributes.OrderBy(_ => _.Id).Select(_ => _.DbaseRecord))
                {
                    dbfRecord.FromBytes(data, _manager, _encoding);
                    dbfWriter.Write(dbfRecord);
                }

                dbfWriter.Writer.Flush();
                await dbfEntryStream.FlushAsync(cancellationToken);
            }
        }
    }
}
