namespace RoadRegistry.Editor.ProjectionHost;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts;
using Schema;
using System;
using System.Linq;

public class EditorContextEventProcessorProjections<TDbContextEventProcessor>
    where TDbContextEventProcessor : EditorContextEventProcessor
{
    public ConnectedProjectionHandlerResolver<EditorContext> Resolver { get; }
    public AcceptStreamMessageFilter Filter { get; }

    public EditorContextEventProcessorProjections(ConnectedProjection<EditorContext>[] projections)
    {
        ArgumentNullException.ThrowIfNull(projections);
        if (!projections.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(projections));
        }

        Resolver = Resolve.WhenEqualToHandlerMessageType(projections.SelectMany(projection => projection.Handlers).ToArray());
        Filter = AcceptStreamMessage<EditorContext>.WhenEqualToMessageType(projections, DbContextEventProcessor<EditorContext>.EventMapping);
    }
}
