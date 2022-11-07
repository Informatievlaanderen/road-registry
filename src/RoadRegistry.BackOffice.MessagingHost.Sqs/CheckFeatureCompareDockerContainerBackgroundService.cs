namespace RoadRegistry.BackOffice.MessagingHost.Sqs;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Configuration;
using Abstractions.FeatureCompare;
using Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

public class CheckFeatureCompareDockerContainerBackgroundService : ApplicationBackgroundService
{
    public CheckFeatureCompareDockerContainerBackgroundService(
        IMediator mediator,
        ILogger<CheckFeatureCompareDockerContainerBackgroundService> logger,
        FeatureCompareMessagingOptions messagingOptions)
        : base(mediator, logger, messagingOptions.ConsumerDelaySeconds)
    {
    }

    protected override async Task ExecuteCallbackAsync(CancellationToken cancellationToken)
    {
        var request = new CheckFeatureCompareDockerContainerMessageRequest();
        await Mediator.Send(request, cancellationToken);
    }
}
