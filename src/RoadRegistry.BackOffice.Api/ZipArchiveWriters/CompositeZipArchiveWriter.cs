namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Microsoft.EntityFrameworkCore;

    public class CompositeZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
    {
        private readonly IZipArchiveWriter<TContext>[] _writers;

        public CompositeZipArchiveWriter(params IZipArchiveWriter<TContext>[] writers)
        {
            _writers = writers ?? throw new ArgumentNullException(nameof(writers));
        }

        public async Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (context == null) throw new ArgumentNullException(nameof(context));
            foreach (var writer in _writers)
            {
                await writer.WriteAsync(archive, context, cancellationToken);
            }
        }
    }
}
