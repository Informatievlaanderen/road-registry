namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

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

        var message = new RequestRoadNetworkExtract
        {
            ExternalRequestId = randomExternalRequestId,
            Contour = contour,
            DownloadId = downloadId,
            Description = request.Description,
            IsInformative = request.IsInformative
        };

        var command = new Command(message);
        await Dispatcher(command, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId, request.IsInformative);
    }
}
