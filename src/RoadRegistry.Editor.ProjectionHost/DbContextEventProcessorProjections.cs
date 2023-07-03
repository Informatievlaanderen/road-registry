namespace RoadRegistry.Editor.ProjectionHost;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;

public class DbContextEventProcessorProjections<TDbContextEventProcessor, TDbContext>
    where TDbContextEventProcessor : DbContextEventProcessor<TDbContext>
    where TDbContext : RunnerDbContext<TDbContext>
{
    public ConnectedProjectionHandlerResolver<TDbContext> Resolver { get; }
    public AcceptStreamMessageFilter Filter { get; }

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
}
