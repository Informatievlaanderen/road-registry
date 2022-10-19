namespace RoadRegistry.BackOffice.MessagingHost.Sqs;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Configuration;
using Abstractions.FeatureCompare;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class FeatureCompareProcessor : BackgroundService
{
    private readonly ILogger<FeatureCompareProcessor> _logger;
    private readonly IMediator _mediator;
    private readonly FeatureCompareMessagingOptions _messagingOptions;

    public FeatureCompareProcessor(
        FeatureCompareMessagingOptions messagingOptions,
        IMediator mediator,
        ILogger<FeatureCompareProcessor> logger)
    {
        _messagingOptions = messagingOptions;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Run periodically and check if we can start another Docker container</summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            var request = new CheckFeatureCompareDockerContainerMessageRequest();
            await _mediator.Send(request, cancellationToken);

            await Task.Delay(_messagingOptions.ConsumerDelaySeconds * 1000, cancellationToken);
        }
    }
}
