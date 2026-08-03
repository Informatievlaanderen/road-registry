namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Microsoft.Extensions.Logging;

public interface IRoadNetworkChangesProjection<in TSession>
{
    Task Project(TSession session, IReadOnlyList<IEvent> events, CancellationToken cancellationToken);
    bool IsCatchingUp { get; set; }

    // Set by the driver so a sub-projection can report an event it was handed but has no handler for. Silently
    // ignoring such an event is indistinguishable from applying it: nothing fails and the progression still advances.
    ILogger? Logger { get; set; }
}
