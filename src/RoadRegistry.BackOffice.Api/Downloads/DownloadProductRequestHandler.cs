namespace RoadRegistry.BackOffice.Api.Downloads;

using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using NodaTime.Text;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Downloads;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;
using RoadRegistry.Product.Schema;

public class DownloadProductRequestHandler : EndpointRequestHandler<DownloadProductRequest, DownloadProductResponse>
{
    private readonly IStreetNameCache _cache;
    private readonly ProductContext _context;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly FileEncoding _fileEncoding;
    private readonly ZipArchiveWriterOptions _writerOptions;

    public DownloadProductRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ProductContext context,
        ZipArchiveWriterOptions writerOptions,
        RecyclableMemoryStreamManager streamManager,
        FileEncoding fileEncoding,
        IStreetNameCache cache,
        ILogger<DownloadProductRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
        _writerOptions = writerOptions;
        _streamManager = streamManager;
        _fileEncoding = fileEncoding;
        _cache = cache;
    }

    protected override async Task<DownloadProductResponse> InnerHandleAsync(DownloadProductRequest request, CancellationToken cancellationToken)
    {
        var info = await _context.RoadNetworkInfo.SingleOrDefaultAsync(cancellationToken);
        if (info is null || !info.CompletedImport)
            throw new DownloadProductNotFoundException();

        var result = LocalDatePattern.CreateWithInvariantCulture("yyyyMMdd").Parse(request.Date);
        if (!result.Success)
            throw new ValidationException(new[]
            {
                new ValidationFailure("date",
                    "'date' path parameter is not a valid date according to format yyyyMMdd.")
            });

        return new DownloadProductResponse($"wegenregister-{request.Date}.zip", new MediaTypeHeaderValue("application/zip"), async (stream, ct) =>
        {
            var writer = new RoadNetworkForProductToZipArchiveWriter(result.Value, _writerOptions, _cache, _streamManager, _fileEncoding);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
            await writer.WriteAsync(archive, _context, cancellationToken);
        });
    }
}
