namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extracts;

public class ProjectionFormatFileZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    private readonly Encoding _encoding;
    private readonly string _filename;

    public ProjectionFormatFileZipArchiveWriter(string filename, Encoding encoding)
    {
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));

        if (context == null) throw new ArgumentNullException(nameof(context));

        var prjEntry = archive.CreateEntry(_filename);
        await using (var prjEntryStream = prjEntry.Open())
        await using (var prjEntryStreamWriter = new StreamWriter(prjEntryStream, _encoding))
        {
            await prjEntryStreamWriter.WriteAsync(ProjectionFormat.BelgeLambert1972.Content);
            await prjEntryStreamWriter.FlushAsync();
        }
    }
}