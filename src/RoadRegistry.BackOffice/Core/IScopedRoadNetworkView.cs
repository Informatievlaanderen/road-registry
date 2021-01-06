namespace RoadRegistry.BackOffice.Core
{
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;

    public interface IScopedRoadNetworkView
    {
        Envelope Scope { get; }

        IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes { get; }
        IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments { get; }
        IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }

        IRoadNetworkView View { get; }
    }
}
