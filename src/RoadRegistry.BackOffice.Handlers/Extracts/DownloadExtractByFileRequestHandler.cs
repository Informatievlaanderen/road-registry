namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class DownloadExtractByFileRequestHandler : EndpointRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    private readonly DownloadExtractByFileRequestItemTranslator _translator;

    public DownloadExtractByFileRequestHandler(
        CommandHandlerDispatcher dispatcher,
        DownloadExtractByFileRequestItemTranslator translator,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _translator = translator;
    }

    public override async Task<DownloadExtractByFileResponse> HandleAsync(DownloadExtractByFileRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");

        var contour = _translator.Translate(request.ShpFile, request.Buffer);

        var message = new Command(
            new RequestRoadNetworkExtract
            {
                ExternalRequestId = randomExternalRequestId,
                Contour = contour,
                DownloadId = downloadId,
                Description = request.Description
            });
        await Dispatcher(message, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId);
    }
}