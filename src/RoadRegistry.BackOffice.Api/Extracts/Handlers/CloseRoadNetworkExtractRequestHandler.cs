namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

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

    protected override async Task<CloseRoadNetworkExtractResponse> InnerHandleAsync(CloseRoadNetworkExtractRequest request, CancellationToken cancellationToken)
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

        var command = new Command(message).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        return new CloseRoadNetworkExtractResponse();
    }
}
