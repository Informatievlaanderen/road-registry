namespace RoadRegistry.Producer.Snapshot.ProjectionHost.NationalRoad;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class NationalRoadEventProcessor : RunnerDbContextEventProcessor<NationalRoadProducerSnapshotContext>
{
    private const string ProjectionStateName = "roadregistry-producer-nationalroad-snapshot-projectionhost";

    public NationalRoadEventProcessor(
        IStreamStore streamStore,
        AcceptStreamMessage<NationalRoadProducerSnapshotContext> acceptStreamMessage,
        EnvelopeFactory envelopeFactory,
        ConnectedProjectionHandlerResolver<NationalRoadProducerSnapshotContext> resolver,
        IDbContextFactory<NationalRoadProducerSnapshotContext> dbContextFactory,
        Scheduler scheduler,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
        : base(
            ProjectionStateName,
            streamStore,
            acceptStreamMessage,
            envelopeFactory,
            resolver,
            dbContextFactory,
            scheduler,
            loggerFactory,
            configuration,
            1,
            1)
    {
    }
}
