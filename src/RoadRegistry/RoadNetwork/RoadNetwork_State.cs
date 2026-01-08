namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Events.V2;
using GradeSeparatedJunction;
using JasperFx.Events;
using RoadNode;
using RoadSegment;
using ValueObjects;

public partial class RoadNetwork : MartenAggregateRootEntity<RoadNetworkId>
{
    public RoadNetworkId RoadNetworkId { get; }
    public RoadNetworkChangesSummary? SummaryOfLastChange { get; private set; }

    public IReadOnlyDictionary<RoadNodeId, RoadNode> RoadNodes { get; }
    public IReadOnlyDictionary<RoadSegmentId, RoadSegment> RoadSegments { get; }
    public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }
    private readonly Dictionary<RoadNodeId, RoadNode> _roadNodes;
    private readonly Dictionary<RoadSegmentId, RoadSegment> _roadSegments;
    private readonly Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> _gradeSeparatedJunctions;

    public RoadNetwork(RoadNetworkId roadNetworkId)
        : this(roadNetworkId, [], [], [])
    {
    }

    public RoadNetwork(
        RoadNetworkId roadNetworkId,
        IReadOnlyCollection<RoadNode> roadNodes,
        IReadOnlyCollection<RoadSegment> roadSegments,
        IReadOnlyCollection<GradeSeparatedJunction> gradeSeparatedJunctions)
        : base(roadNetworkId)
    {
        RoadNetworkId = roadNetworkId;

        _roadNodes = roadNodes.ToDictionary(x => x.RoadNodeId, x => x);
        RoadNodes = _roadNodes.AsReadOnly();

        _roadSegments = roadSegments.ToDictionary(x => x.RoadSegmentId, x => x);
        RoadSegments = _roadSegments.AsReadOnly();

        _gradeSeparatedJunctions = gradeSeparatedJunctions.ToDictionary(x => x.GradeSeparatedJunctionId, x => x);
        GradeSeparatedJunctions = _gradeSeparatedJunctions.AsReadOnly();
    }

    public static RoadNetwork Create(RoadNetworkWasChanged @event)
    {
        var roadNetwork = new RoadNetwork(@event.RoadNetworkId)
        {
            SummaryOfLastChange = @event.Summary.ToRoadNetworkChangesSummary()
        };
        return roadNetwork;
    }

    public void Apply(RoadNetworkWasChanged @event)
    {
        UncommittedEvents.Add(@event);

        SummaryOfLastChange = @event.Summary.ToRoadNetworkChangesSummary();
    }

    public IEnumerable<RoadNode> GetNonRemovedRoadNodes()
    {
        return _roadNodes.Values.Where(x => !x.IsRemoved);
    }
    public IEnumerable<RoadSegment> GetNonRemovedRoadSegments()
    {
        return _roadSegments.Values.Where(x => !x.IsRemoved);
    }
    public IEnumerable<GradeSeparatedJunction> GetNonRemovedGradeSeparatedJunctions()
    {
        return _gradeSeparatedJunctions.Values.Where(x => !x.IsRemoved);
    }
}
