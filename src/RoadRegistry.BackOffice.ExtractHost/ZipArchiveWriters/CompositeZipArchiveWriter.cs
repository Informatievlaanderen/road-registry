namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Extracts;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;

    public class CompositeZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
    {
        private readonly IZipArchiveWriter<TContext>[] _writers;

        public CompositeZipArchiveWriter(params IZipArchiveWriter<TContext>[] writers)
        {
            _writers = writers ?? throw new ArgumentNullException(nameof(writers));
        }

        public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, TContext context,
            CancellationToken cancellationToken)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (context == null) throw new ArgumentNullException(nameof(context));

            foreach (var writer in _writers)
            {
                await writer.WriteAsync(archive, request, context, cancellationToken);
            }
        }
    }
}
