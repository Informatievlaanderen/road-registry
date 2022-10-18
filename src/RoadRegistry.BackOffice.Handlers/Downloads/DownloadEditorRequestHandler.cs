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
using ZipArchiveWriters.ForEditor;

public class DownloadEditorRequestHandler : EndpointRequestHandler<DownloadEditorRequest, DownloadEditorResponse>
{
    public DownloadEditorRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext context,
        RecyclableMemoryStreamManager streamManager,
        ZipArchiveWriterOptions writerOptions,
        IStreetNameCache cache,
        ILogger<DownloadEditorRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
        _writerOptions = writerOptions;
        _streamManager = streamManager;
        _cache = cache;
    }

    private readonly IStreetNameCache _cache;
    private readonly EditorContext _context;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly ZipArchiveWriterOptions _writerOptions;

    public override async Task<DownloadEditorResponse> HandleAsync(DownloadEditorRequest request, CancellationToken cancellationToken)
    {
        var info = await _context.RoadNetworkInfo.SingleOrDefaultAsync(cancellationToken);

        if (info is null || !info.CompletedImport)
            throw new DownloadEditorNotFoundException("Could not find the requested information", HttpStatusCode.ServiceUnavailable);

        var filename = $"wegenregister-{DateTime.Today.ToString("yyyyMMdd")}.zip";
        return new DownloadEditorResponse(filename, new MediaTypeHeaderValue("application/zip"), async (stream, ct) =>
        {
            var encoding = Encoding.GetEncoding(1252);
            var writer = new RoadNetworkForEditorToZipArchiveWriter(_writerOptions, _cache, _streamManager, encoding);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
            await writer.WriteAsync(archive, _context, cancellationToken);
        });
    }
}
