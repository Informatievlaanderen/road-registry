namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class DownloadExtractRequestHandler : ExtractRequestHandler<DownloadExtractRequest, DownloadExtractResponse>
{
    private readonly WKTReader _reader;

    public DownloadExtractRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        WKTReader reader,
        ILogger<DownloadExtractRequestHandler> logger) : base(context, dispatcher, logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public override async Task<DownloadExtractResponse> HandleRequestAsync(DownloadExtractRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        await DispatchCommandWithContextAddAsync(
            new ExtractRequestRecord
            {
                RequestedOn = DateTime.UtcNow,
                ExternalRequestId = request.RequestId,
                Contour = _reader.Read(request.Contour),
                DownloadId = downloadId,
                Description = request.RequestId,
                UploadExpected = request.UploadExpected
            },
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = request.RequestId,
                Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry((IPolygonal)_reader.Read(request.Contour)),
                DownloadId = downloadId,
                Description = request.RequestId,
                UploadExpected = request.UploadExpected
            }, cancellationToken);

        return new DownloadExtractResponse(downloadId, request.UploadExpected);
    }
}
