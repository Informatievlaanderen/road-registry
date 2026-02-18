namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Events.V2;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadSegment;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public partial class ScopedRoadNetwork : MartenAggregateRootEntity<ScopedRoadNetworkId>
{
    public ScopedRoadNetworkId RoadNetworkId { get; }
    public RoadNetworkChangesSummary? SummaryOfLastChange { get; private set; }

    public IReadOnlyDictionary<RoadNodeId, RoadNode> RoadNodes { get; }
    public IReadOnlyDictionary<RoadSegmentId, RoadSegment> RoadSegments { get; }
    public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }
    private readonly Dictionary<RoadNodeId, RoadNode> _roadNodes;
    private readonly Dictionary<RoadSegmentId, RoadSegment> _roadSegments;
    private readonly Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> _gradeSeparatedJunctions;

    public ScopedRoadNetwork(ScopedRoadNetworkId roadNetworkId)
        : this(roadNetworkId, [], [], [])
    {
    }

    public ScopedRoadNetwork(
        ScopedRoadNetworkId roadNetworkId,
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

    public static ScopedRoadNetwork Create(RoadNetworkWasChanged @event)
    {
        var roadNetwork = new ScopedRoadNetwork(@event.RoadNetworkId)
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

    public (RoadNodeId? StartNodeId, RoadNodeId? EndNodeId, Problems Problems) FindStartEndNodes(RoadSegmentId roadSegmentId, RoadSegmentGeometryDrawMethodV2 method, RoadSegmentGeometry geometry, VerificationContextTolerances tolerances)
    {
        if (method == RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        {
            return (RoadNodeId.Zero, RoadNodeId.Zero, Problems.None);
        }

        var problems = Problems.None;

        var startNodeId = FindRoadNode(geometry.Value.Coordinate, tolerances)?.RoadNodeId;
        if (startNodeId is null)
        {
            problems += new RoadSegmentStartNodeMissing(roadSegmentId);
        }

        var endNodeId = FindRoadNode(geometry.Value.Coordinates.Last(), tolerances)?.RoadNodeId;
        if (endNodeId is null)
        {
            problems += new RoadSegmentEndNodeMissing(roadSegmentId);
        }

        return (startNodeId, endNodeId, problems);
    }

    private RoadNode? FindRoadNode(Coordinate coordinate, VerificationContextTolerances tolerance)
    {
        var point = new Point(coordinate.X, coordinate.Y);
        return _roadNodes.Values
            .FirstOrDefault(x => !x.IsRemoved && x.Geometry.Value.IsReasonablyEqualTo(point, tolerance));
    }
}
