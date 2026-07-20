namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;

public interface IRoadNetworkChangesProjection<in TSession>
{
    Task Project(TSession session, IReadOnlyList<IEvent> events, CancellationToken cancellationToken);
    bool IsCatchingUp { get; set; }
}
