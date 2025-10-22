namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Collections.Immutable;
using Messages;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

public interface IRoadNetworkView
{
    // data
    IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableLaneAttributeIdentifiers { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableSurfaceAttributeIdentifiers { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableWidthAttributeIdentifiers { get; }
    IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments { get; }
    IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }

    // scoping
    IScopedRoadNetworkView CreateScopedView(Envelope envelope);
    IRoadNetworkView RestoreFromEvent(object @event);

    // based on events
    IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events);
    IRoadNetworkView RestoreFromSnapshot(RoadNetworkSnapshot snapshot);

    // snapshot support
    RoadNetworkSnapshot TakeSnapshot();

    // based on command
    IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes);
}
