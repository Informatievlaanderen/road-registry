namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Events.V2;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadSegment;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public partial class ScopedRoadNetwork : MartenAggregateRootEntity<ScopedRoadNetworkId>
{
    public ScopedRoadNetworkId RoadNetworkId { get; }
    public RoadNetworkChangesSummary? SummaryOfLastChange { get; private set; }
    public IReadOnlyDictionary<RoadNodeId, RoadNode> RoadNodes { get; }
    public IReadOnlyDictionary<RoadSegmentId, RoadSegment> RoadSegments { get; }
    public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }
    public IReadOnlyDictionary<GradeJunctionId, GradeJunction> GradeJunctions { get; }

    private readonly Dictionary<RoadNodeId, RoadNode> _roadNodes;
    private readonly Dictionary<RoadSegmentId, RoadSegment> _roadSegments;
    private readonly Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> _gradeSeparatedJunctions;
    private readonly Dictionary<GradeJunctionId, GradeJunction> _gradeJunctions;

    public ScopedRoadNetwork(
        ScopedRoadNetworkId roadNetworkId,
        IReadOnlyCollection<RoadNode>? roadNodes = null,
        IReadOnlyCollection<RoadSegment>? roadSegments = null,
        IReadOnlyCollection<GradeSeparatedJunction>? gradeSeparatedJunctions = null,
        IReadOnlyCollection<GradeJunction>? gradeJunctions = null)
        : base(roadNetworkId)
    {
        RoadNetworkId = roadNetworkId;

        _roadNodes = (roadNodes ?? []).ToDictionary(x => x.RoadNodeId, x => x);
        RoadNodes = _roadNodes.AsReadOnly();

        _roadSegments = (roadSegments ?? []).ToDictionary(x => x.RoadSegmentId, x => x);
        RoadSegments = _roadSegments.AsReadOnly();

        _gradeSeparatedJunctions = (gradeSeparatedJunctions ?? []).ToDictionary(x => x.GradeSeparatedJunctionId, x => x);
        GradeSeparatedJunctions = _gradeSeparatedJunctions.AsReadOnly();

        _gradeJunctions = (gradeJunctions ?? []).ToDictionary(x => x.GradeJunctionId, x => x);
        GradeJunctions = _gradeJunctions.AsReadOnly();
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

    public (RoadNodeId StartNodeId, RoadNodeId EndNodeId, Problems Problems) FindStartEndNodes(
        RoadSegmentGeometry geometry,
        VerificationContextTolerances tolerances)
    {
        var problems = Problems.None;

        var startNodeId = FindRoadNode(geometry.Value.Coordinate, tolerances)?.RoadNodeId;
        if (startNodeId is null)
        {
            problems += new RoadSegmentStartNodeMissing();
        }

        var endNodeId = FindRoadNode(geometry.Value.Coordinates.Last(), tolerances)?.RoadNodeId;
        if (endNodeId is null)
        {
            problems += new RoadSegmentEndNodeMissing();
        }

        return (startNodeId ?? RoadNodeId.Zero, endNodeId ?? RoadNodeId.Zero, problems);
    }

    public Problems ValidatePartiallyOverlappingRoadSegments(
        RoadSegmentGeometry geometry,
        IReadOnlyCollection<RoadSegmentId> excludeRoadSegmentIds,
        IIdentifierTranslator idTranslator)
    {
        const double bufferDistance = 0.015;

        var roughGeometry = geometry.Value.Envelope.Buffer(bufferDistance);
        var nearbyOtherSegments = _roadSegments
            .Where(x => x.Value.Attributes.Status == RoadSegmentStatusV2.Gerealiseerd)
            .Where(x => !excludeRoadSegmentIds.Contains(x.Key) && x.Value.Geometry.Value.Intersects(roughGeometry))
            .Select(x => (x.Key, x.Value.Geometry))
            .ToList();

        if (!nearbyOtherSegments.Any())
        {
            return Problems.None;
        }

        var overlappingOtherSegmentId = GetOtherRoadSegmentIdWhoOverlapsPartially(geometry, bufferDistance, nearbyOtherSegments)
                                        ?? nearbyOtherSegments
                                            .Select(otherSegment => GetOtherRoadSegmentIdWhoOverlapsPartially(otherSegment.Geometry, bufferDistance, [(otherSegment.Key, geometry)]))
                                            .FirstOrDefault();
        if (overlappingOtherSegmentId is not null)
        {
            return Problems.Single(new Error(
                ProblemCode.RoadSegment.PartiallyOverlapsWithAnotherRoadSegment.ToString(),
                idTranslator.TranslateToTemporaryId(overlappingOtherSegmentId.Value).ToRoadSegmentProblemParameters("OtherWegsegment").ToArray()));
        }

        return Problems.None;
    }

    private static RoadSegmentId? GetOtherRoadSegmentIdWhoOverlapsPartially(
        RoadSegmentGeometry selfGeometry,
        double bufferDistance,
        IReadOnlyCollection<(RoadSegmentId RoadSegmentId, RoadSegmentGeometry Geometry)> otherSegments)
    {
        foreach (var selfLineSegment in selfGeometry.Value.Geometries)
        {
            var coordinates = selfLineSegment.Coordinates;

            // Traverse each coordinate starting from the 2nd one
            for (var i = 1; i < coordinates.Length; i++)
            {
                var previousVertex = coordinates[i - 1];
                var currentVertex = coordinates[i];

                // Create a line segment between previous and current vertex
                var segmentCoordinates = new[] { previousVertex, currentVertex };
                var lineSegment = selfGeometry.Value.Factory.CreateLineString(segmentCoordinates);

                // Create buffer around the line segment
                var bufferedLineSegment = lineSegment.Buffer(bufferDistance);

                // Check if buffered area intersects with at least 2 vertices of another segment
                foreach (var otherSegment in otherSegments)
                {
                    var oneCoordinateIntersected = false;

                    for (var c = 0; c < otherSegment.Geometry.Value.Coordinates.Length; c++)
                    {
                        if (otherSegment.Geometry.Value.Factory.CreatePoint(otherSegment.Geometry.Value.Coordinates[c]).Intersects(bufferedLineSegment))
                        {
                            if (oneCoordinateIntersected)
                            {
                                return otherSegment.RoadSegmentId;
                            }

                            oneCoordinateIntersected = true;
                        }
                    }
                }
            }
        }

        return null;
    }

    private RoadNode? FindRoadNode(Coordinate coordinate, VerificationContextTolerances tolerance)
    {
        var point = new Point(coordinate.X, coordinate.Y);
        return _roadNodes.Values
            .FirstOrDefault(x => !x.IsRemoved && x.Geometry.Value.IsReasonablyEqualTo(point, tolerance));
    }
}
