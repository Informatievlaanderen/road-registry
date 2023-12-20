namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Collections.Immutable;
using Messages;
using NetTopologySuite.Geometries;

public interface IRoadNetworkView
{
    IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }

    // data
    IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableLaneAttributeIdentifiers { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableSurfaceAttributeIdentifiers { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableWidthAttributeIdentifiers { get; }
    IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments { get; }

    // scoping
    IScopedRoadNetworkView CreateScopedView(Envelope envelope);
    IRoadNetworkView RestoreFromEvent(object @event);

    // based on events
    IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events);
    IRoadNetworkView RestoreFromSnapshot(RoadNetworkSnapshot snapshot);

    // snapshot support
    RoadNetworkSnapshot TakeSnapshot();

    // im-/mutable support
    IRoadNetworkView ToBuilder();
    IRoadNetworkView ToImmutable();

    // based on command
    IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes);
}
