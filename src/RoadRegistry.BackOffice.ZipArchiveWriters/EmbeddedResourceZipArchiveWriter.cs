namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.IO.Compression;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

public class EmbeddedResourceZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    private readonly Assembly _assembly;
    private readonly string _filename;
    private readonly string _resourceName;

    public EmbeddedResourceZipArchiveWriter(Assembly assembly, string resourceName, string filename)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
    }

    public async Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        await using (var embeddedResourceStream = _assembly.GetManifestResourceStream(_resourceName))
        {
            if (embeddedResourceStream != null)
            {
                var entry = archive.CreateEntry(_filename);
                await using (var entryStream = entry.Open())
                {
                    await embeddedResourceStream.CopyToAsync(entryStream, cancellationToken);
                }
            }
        }
    }
}