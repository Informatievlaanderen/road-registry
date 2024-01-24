namespace RoadRegistry.Hosts;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;

public class DbContextEventProcessorProjections<TDbContextEventProcessor, TDbContext>
    where TDbContextEventProcessor : DbContextEventProcessor<TDbContext>
    where TDbContext : RunnerDbContext<TDbContext>
{
    public DbContextEventProcessorProjections(ConnectedProjection<TDbContext>[] projections)
    {
        ArgumentNullException.ThrowIfNull(projections);
        if (!projections.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(projections));
        }

        Resolver = Resolve.WhenEqualToHandlerMessageType(projections.SelectMany(projection => projection.Handlers).ToArray());
        Filter = AcceptStreamMessage<TDbContext>.WhenEqualToMessageType(projections, DbContextEventProcessor<TDbContext>.EventMapping);
    }

    public ConnectedProjectionHandlerResolver<TDbContext> Resolver { get; }
    public AcceptStreamMessageFilter Filter { get; }
}
