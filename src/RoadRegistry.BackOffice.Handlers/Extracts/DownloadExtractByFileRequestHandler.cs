namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

public class DownloadExtractByFileRequestHandler : ExtractRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    private readonly DownloadExtractByFileRequestItemTranslator _translator;

    public DownloadExtractByFileRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        DownloadExtractByFileRequestItemTranslator translator,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(context, dispatcher, logger)
    {
        _translator = translator;
    }

    public override async Task<DownloadExtractByFileResponse> HandleRequestAsync(DownloadExtractByFileRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
    {
        var contour = _translator.Translate(request.ShpFile, request.Buffer);

        await DispatchCommandWithContextAddAsync(
            new ExtractRequestRecord
            {
                DownloadId = downloadId,
                Contour = BackOffice.GeometryTranslator.Translate(contour) as Geometry,
                Description = request.Description,
                UploadExpected = request.UploadExpected
            },
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = contour,
                DownloadId = downloadId,
                Description = request.Description,
                UploadExpected = request.UploadExpected
            }, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId, request.UploadExpected);
    }
}
