namespace RoadRegistry.Editor.ProjectionHost;

using System;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.Extensions.Logging;
using Schema;
using SqlStreamStore;

public class EventProcessor : DbContextEventProcessor<EditorContext>
{
    public EventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessageFilter filter,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<EditorContext> resolver,
        Func<EditorContext> dbContextFactory,
        Scheduler scheduler,
        ILogger<EventProcessor> logger)
        : base(QueueName, streamStore, filter, envelopeFactory, resolver, dbContextFactory, scheduler, logger)
    {
    }

    private const string QueueName = "roadregistry-editor-projectionhost";
}