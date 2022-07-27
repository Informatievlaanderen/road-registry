namespace RoadRegistry.BackOffice.ZipArchiveWriters
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class ProjectionFormatFileZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
    {
        private readonly string _filename;
        private readonly Encoding _encoding;

        public ProjectionFormatFileZipArchiveWriter(string filename, Encoding encoding)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }
        public async Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken)
        {
            if (archive == null)
            {
                throw new ArgumentNullException(nameof(archive));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var prjEntry = archive.CreateEntry(_filename);
            using (var prjEntryStream = prjEntry.Open())
            using (var prjEntryStreamWriter = new StreamWriter(prjEntryStream, _encoding))
            {
                await prjEntryStreamWriter.WriteAsync(ProjectionFormat.BelgeLambert1972.Content);
                await prjEntryStreamWriter.FlushAsync();
            }
        }
    }
}
