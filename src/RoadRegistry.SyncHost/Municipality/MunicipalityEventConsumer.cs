namespace RoadRegistry.SyncHost.Municipality;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts;
using Microsoft.Extensions.Logging;
using Sync.MunicipalityRegistry;
using Resolve = Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Resolve;

public class MunicipalityEventConsumer : RoadRegistryBackgroundService
{
    private readonly IMunicipalityEventTopicConsumer _consumer;

    public MunicipalityEventConsumer(
        IMunicipalityEventTopicConsumer consumer,
        ILogger<MunicipalityEventConsumer> logger
    ) : base(logger)
    {
        _consumer = consumer.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        var projector =
            new ConnectedProjector<MunicipalityEventConsumerContext>(
                Resolve.WhenEqualToHandlerMessageType(new MunicipalityEventProjection().Handlers));

        await _consumer.ConsumeContinuously(async (message, dbContext) =>
        {
            await ConsumeHandler(message, projector, dbContext);
        }, cancellationToken);
    }

    private async Task ConsumeHandler(object message, ConnectedProjector<MunicipalityEventConsumerContext> projector, MunicipalityEventConsumerContext dbContext)
    {
        Logger.LogInformation("Processing {Type}", message.GetType().Name);

        await projector.ProjectAsync(dbContext, message).ConfigureAwait(false);

        await dbContext.SaveChangesAsync(CancellationToken.None);

        Logger.LogInformation("Processed {Type}", message.GetType().Name);
    }
}
