namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

public class ImmutableScopedRoadNetworkView : IScopedRoadNetworkView
{
    internal ImmutableScopedRoadNetworkView(
        Envelope scope,
        IReadOnlyDictionary<RoadNodeId, RoadNode> nodes,
        IReadOnlyDictionary<RoadSegmentId, RoadSegment> segments,
        IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> gradeSeparatedJunctions,
        IRoadNetworkView view)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        GradeSeparatedJunctions = gradeSeparatedJunctions ?? throw new ArgumentNullException(nameof(gradeSeparatedJunctions));
        View = view ?? throw new ArgumentNullException(nameof(view));
    }

    public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }
    public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes { get; }
    public Envelope Scope { get; }
    public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments { get; }
    public IRoadNetworkView View { get; }
}
