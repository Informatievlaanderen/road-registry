namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

public class CloseRoadNetworkExtractRequestHandler : EndpointRequestHandler<CloseRoadNetworkExtractRequest, CloseRoadNetworkExtractResponse>
{
    private readonly EditorContext _editorContext;

    public CloseRoadNetworkExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _editorContext = editorContext;
    }

    public override async Task<CloseRoadNetworkExtractResponse> HandleAsync(CloseRoadNetworkExtractRequest request, CancellationToken cancellationToken)
    {
        var extractRequest = await _editorContext.ExtractRequests.FindAsync(request.DownloadId.ToGuid(), cancellationToken: cancellationToken);
        if (extractRequest is null)
        {
            throw new UploadExtractNotFoundException("Could not close extract");
        }

        var message = new CloseRoadNetworkExtract
        {
            ExternalRequestId = new ExternalExtractRequestId(extractRequest.ExternalRequestId),
            Reason = request.Reason
        };

        var command = new Command(message);
        await Dispatch(command, cancellationToken);

        return new CloseRoadNetworkExtractResponse();
    }
}
