namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;

public interface IRoadNetworkChangesProjection
{
    Task Project(IReadOnlyList<IEvent> events, IDocumentOperations operations, CancellationToken cancellationToken);
}
