using RoadRegistry.BackOffice.Framework;

namespace RoadRegistry.Api.BackOffice.Handlers
{
    using System.IO.Compression;
    using System.Text;
    using Editor.Schema;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;

    internal class DownloadEditorRequestHandler : EndpointRequestHandler<DownloadEditorRequest, DownloadEditorResponse>,
        IRequestExceptionHandler<DownloadEditorRequest, DownloadEditorResponse, DownloadEditorNotFoundException>
    {
        private readonly EditorContext _context;
        private readonly ZipArchiveWriterOptions _writerOptions

        public DownloadEditorRequestHandler(CommandHandlerDispatcher dispatcher, EditorContext context) : base(dispatcher)
        {
            _context = context;
        }

        public Task Handle(
            DownloadEditorRequest request,
            DownloadEditorNotFoundException exception,
            RequestExceptionHandlerState<DownloadEditorResponse> state,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task<DownloadEditorResponse> HandleAsync(DownloadEditorRequest request, CancellationToken cancellationToken)
        {
            var info = await _context.RoadNetworkInfo.SingleOrDefaultAsync(cancellationToken);
            if (info is null || !info.CompletedImport)
                throw new DownloadEditorNotFoundException("Could not find the requested information", StatusCodes.Status503ServiceUnavailable);

            return new DownloadEditorResponse(new FileCallbackResult(
                new MediaTypeHeaderValue("application/zip"),
                async (stream, actionContext) =>
                {
                    var encoding = Encoding.GetEncoding(1252);
                    var writer = new RoadNetworkForEditorToZipArchiveWriter(_writerOptions, _cache, _manager, encoding);
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        await writer.WriteAsync(archive, _context, cancellationToken);
                    }
                })
                {
                    FileDownloadName = "wegenregister.zip"
                }
            );
        }
    }
}
