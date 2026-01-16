namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;

public interface IRoadNetworkChangesProjection
{
    Task Project(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellationToken);
}
