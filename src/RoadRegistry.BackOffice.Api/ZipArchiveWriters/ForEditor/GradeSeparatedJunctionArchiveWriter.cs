namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForEditor
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
    using Editor.Schema.GradeSeparatedJunctions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;

    public class GradeSeparatedJunctionArchiveWriter : IZipArchiveWriter<EditorContext>
    {
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly Encoding _encoding;

        public GradeSeparatedJunctionArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public async Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var count = await context.GradeSeparatedJunctions.CountAsync(cancellationToken);
            var dbfEntry = archive.CreateEntry("RltOgkruising.dbf");
            var dbfHeader = new DbaseFileHeader(
                DateTime.Now,
                DbaseCodePage.Western_European_ANSI,
                new DbaseRecordCount(count),
                GradeSeparatedJunctionDbaseRecord.Schema
            );
            using (var dbfEntryStream = dbfEntry.Open())
            using (var dbfWriter =
                new DbaseBinaryWriter(
                    dbfHeader,
                    new BinaryWriter(dbfEntryStream, _encoding, true)))
            {
                var dbfRecord = new GradeSeparatedJunctionDbaseRecord();
                foreach (var data in context.GradeSeparatedJunctions.OrderBy(_ => _.Id).Select(_ => _.DbaseRecord))
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
