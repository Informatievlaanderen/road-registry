namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Collections.Immutable;
using Messages;
using NetTopologySuite.Geometries;

public interface IRoadNetworkView
{
    // data
    IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes { get; }
    IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments { get; }
    IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }
    TransactionId MaximumTransactionId { get; }
    RoadNodeId MaximumNodeId { get; }
    RoadSegmentId MaximumSegmentId { get; }
    GradeSeparatedJunctionId MaximumGradeSeparatedJunctionId { get; }
    AttributeId MaximumEuropeanRoadAttributeId { get; }
    AttributeId MaximumNationalRoadAttributeId { get; }
    AttributeId MaximumNumberedRoadAttributeId { get; }
    AttributeId MaximumLaneAttributeId { get; }
    AttributeId MaximumWidthAttributeId { get; }
    AttributeId MaximumSurfaceAttributeId { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableLaneAttributeIdentifiers { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableWidthAttributeIdentifiers { get; }
    ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableSurfaceAttributeIdentifiers { get; }

    // based on events
    IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events);
    IRoadNetworkView RestoreFromEvent(object @event);

    // based on command
    IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes);

    // snapshot support
    RoadNetworkSnapshot TakeSnapshot();
    IRoadNetworkView RestoreFromSnapshot(RoadNetworkSnapshot snapshot);

    // im-/mutable support
    IRoadNetworkView ToBuilder();
    IRoadNetworkView ToImmutable();

    // scoping
    IScopedRoadNetworkView CreateScopedView(Envelope envelope);
}
