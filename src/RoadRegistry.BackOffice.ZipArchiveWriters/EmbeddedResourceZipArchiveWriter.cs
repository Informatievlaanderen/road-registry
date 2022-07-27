namespace RoadRegistry.BackOffice.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class EmbeddedResourceZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
    {
        private readonly Assembly _assembly;
        private readonly string _resourceName;
        private readonly string _filename;

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

            using (var embeddedResourceStream = _assembly.GetManifestResourceStream(_resourceName))
            {
                if (embeddedResourceStream != null)
                {
                    var entry = archive.CreateEntry(_filename);
                    using (var entryStream = entry.Open())
                    {
                        await embeddedResourceStream.CopyToAsync(entryStream, cancellationToken);
                    }
                }
            }
        }
    }
}
