namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;

public interface IRoadNetworkChangesProjection
{
    Task Project(ICollection<IEvent> events, IDocumentSession session);
}
