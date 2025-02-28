namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class DownloadExtractByFileRequestHandler : ExtractRequestHandler<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    private readonly IDownloadExtractByFileRequestItemTranslator _translator;

    public DownloadExtractByFileRequestHandler(
        CommandHandlerDispatcher dispatcher,
        IDownloadExtractByFileRequestItemTranslator translator,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _translator = translator;
    }

    protected override async Task<DownloadExtractByFileResponse> HandleRequestAsync(DownloadExtractByFileRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken)
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

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new DownloadExtractByFileResponse(downloadId, request.IsInformative);
    }
}
