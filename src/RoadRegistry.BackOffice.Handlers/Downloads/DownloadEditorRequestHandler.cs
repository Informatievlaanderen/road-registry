namespace RoadRegistry.BackOffice.Handlers.Downloads;

using System.IO.Compression;
using System.Net;
using System.Text;
using Abstractions;
using Abstractions.Downloads;
using Abstractions.Exceptions;
using Editor.Schema;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using RoadRegistry.Extracts;
using RoadRegistry.Infrastructure;
using ZipArchiveWriters.ForEditor;

public class DownloadEditorRequestHandler : EndpointRequestHandler<DownloadEditorRequest, DownloadEditorResponse>
{
    private readonly IStreetNameCache _cache;
    private readonly EditorContext _context;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly FileEncoding _fileEncoding;
    private readonly ZipArchiveWriterOptions _writerOptions;

    public DownloadEditorRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext context,
        RecyclableMemoryStreamManager streamManager,
        FileEncoding fileEncoding,
        ZipArchiveWriterOptions writerOptions,
        IStreetNameCache cache,
        ILogger<DownloadEditorRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
        _writerOptions = writerOptions;
        _streamManager = streamManager;
        _fileEncoding = fileEncoding;
        _cache = cache;
    }

    protected override async Task<DownloadEditorResponse> InnerHandleAsync(DownloadEditorRequest request, CancellationToken cancellationToken)
    {
        var info = await _context.RoadNetworkInfo.SingleOrDefaultAsync(cancellationToken);

        if (info is null || !info.CompletedImport)
            throw new DownloadEditorNotFoundException("Could not find the requested information", HttpStatusCode.ServiceUnavailable);

        var filename = $"wegenregister-{DateTime.Today.ToString("yyyyMMdd")}.zip";
        return new DownloadEditorResponse(filename, new MediaTypeHeaderValue("application/zip"), async (stream, ct) =>
        {
            var writer = new RoadNetworkForEditorToZipArchiveWriter(_writerOptions, _cache, _streamManager, _fileEncoding);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
            await writer.WriteAsync(archive, _context, cancellationToken);
        });
    }
}
