namespace RoadRegistry.Extracts.ZipArchiveWriters;

using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;

public class CompositeZipArchiveWriter : IZipArchiveWriter
{
    private readonly ILogger _logger;
    private readonly IZipArchiveWriter[] _writers;

    public CompositeZipArchiveWriter(ILogger logger, params IZipArchiveWriter[] writers)
    {
        _logger = logger;
        _writers = writers.ThrowIfNull();
    }

    public async Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataSession zipArchiveData,
        ZipArchiveWriteContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveData);

        foreach (var writer in _writers)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("{Type} started...", writer.GetType().Name);

            await writer.WriteAsync(archive, request, zipArchiveData, context, cancellationToken);

            _logger.LogInformation("{Type} completed in {Elapsed}", writer.GetType().Name, sw.Elapsed);
        }
    }
}
