namespace RoadRegistry.BackOffice.Handlers.Downloads;

using System.IO.Compression;
using System.Text;
using Abstractions;
using Abstractions.Downloads;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using NodaTime.Text;
using Product.Schema;
using ZipArchiveWriters.ForProduct;

public class DownloadProductRequestHandler : EndpointRequestHandler<DownloadProductRequest, DownloadProductResponse>, IRequestExceptionHandler<DownloadProductRequest, DownloadProductResponse, DownloadProductNotFoundException>
{
    private readonly IStreetNameCache _cache;
    private readonly ProductContext _context;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly ZipArchiveWriterOptions _writerOptions;

    public DownloadProductRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ProductContext context,
        ZipArchiveWriterOptions writerOptions,
        RecyclableMemoryStreamManager streamManager,
        IStreetNameCache cache,
        ILogger<DownloadProductRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
        _writerOptions = writerOptions;
        _streamManager = streamManager;
        _cache = cache;
    }

    public Task Handle(
        DownloadProductRequest request,
        DownloadProductNotFoundException exception,
        RequestExceptionHandlerState<DownloadProductResponse> state,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override async Task<DownloadProductResponse> HandleAsync(DownloadProductRequest request, CancellationToken cancellationToken)
    {
        var info = await _context.RoadNetworkInfo.SingleOrDefaultAsync(cancellationToken);
        if (info is null || !info.CompletedImport)
            throw new DownloadProductNotFoundException("");

        var result = LocalDatePattern.CreateWithInvariantCulture("yyyyMMdd").Parse(request.Date);
        if (!result.Success)
            throw new ValidationException(new[]
            {
                new ValidationFailure("date",
                    "'date' path parameter is not a valid date according to format yyyyMMdd.")
            });

        return new DownloadProductResponse(
            $"wegenregister-{request.Date}.zip",
            new MediaTypeHeaderValue("application/zip"),
            CallbackInternal
        );

        async Task CallbackInternal(Stream stream, CancellationToken cancellationToken)
        {
            var encoding = Encoding.GetEncoding(1252);
            var writer = new RoadNetworkForProductToZipArchiveWriter(result.Value, _writerOptions, _cache, _streamManager, encoding);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
            await writer.WriteAsync(archive, _context, cancellationToken);
        }
    }
}
