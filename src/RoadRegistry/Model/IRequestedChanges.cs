namespace RoadRegistry.Model
{
    using System.Collections.Generic;

    public interface IRequestedChanges
    {
        IReadOnlyCollection<IRequestedChange> Changes { get; }

        bool TryResolvePermanent(RoadNodeId id, out RoadNodeId permanent);
        bool TryResolvePermanent(RoadSegmentId id, out RoadSegmentId permanent);
        bool TryResolveTemporary(RoadNodeId id, out RoadNodeId temporary);
        bool TryResolveTemporary(RoadSegmentId id, out RoadSegmentId temporary);
    }
}
