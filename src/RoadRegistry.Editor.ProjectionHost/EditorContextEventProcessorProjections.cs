namespace RoadRegistry.Editor.ProjectionHost;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts;
using Schema;

public class EditorContextEventProcessorProjections<TDbContextEventProcessor>
    where TDbContextEventProcessor : EditorContextEventProcessor
{
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

    public ConnectedProjectionHandlerResolver<EditorContext> Resolver { get; }
    public AcceptStreamMessageFilter Filter { get; }
}
