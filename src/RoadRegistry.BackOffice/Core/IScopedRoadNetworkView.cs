namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using NetTopologySuite.Geometries;

public interface IScopedRoadNetworkView
{
    IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }

    IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes { get; }
    Envelope Scope { get; }
    IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments { get; }

    IRoadNetworkView View { get; }
}