namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class CloseRoadNetworkExtractRequestHandler : EndpointRequestHandler<CloseRoadNetworkExtractRequest, CloseRoadNetworkExtractResponse>
{
    public CloseRoadNetworkExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
    }

    public override async Task<CloseRoadNetworkExtractResponse> HandleAsync(CloseRoadNetworkExtractRequest request, CancellationToken cancellationToken)
    {
        var message = new CloseRoadNetworkExtract
        {
            DownloadId = request.DownloadId,
            Reason = request.Reason
        };

        var command = new Command(message);
        await Dispatcher(command, cancellationToken);

        return new CloseRoadNetworkExtractResponse();
    }
}
