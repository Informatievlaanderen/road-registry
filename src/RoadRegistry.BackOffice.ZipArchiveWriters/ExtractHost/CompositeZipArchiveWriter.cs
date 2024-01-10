namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.Diagnostics;
using System.IO.Compression;
using Extracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class CompositeZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    private readonly ILogger _logger;
    private readonly IZipArchiveWriter<TContext>[] _writers;

    public CompositeZipArchiveWriter(ILogger logger, params IZipArchiveWriter<TContext>[] writers)
    {
        _logger = logger;
        _writers = writers.ThrowIfNull();
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, TContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var writer in _writers)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("{Type} started...", writer.GetType().Name);

            await writer.WriteAsync(archive, request, context, cancellationToken);

            _logger.LogInformation("{Type} completed in {Elapsed}", writer.GetType().Name, sw.Elapsed);
        }
    }
}
