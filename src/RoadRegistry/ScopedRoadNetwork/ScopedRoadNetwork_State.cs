namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Events.V2;
using Extensions;
using Microsoft.Extensions.Logging;
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

    private readonly LazyQuadtree<RoadNode> _roadNodesSpatialIndex;
    private readonly LazyQuadtree<RoadSegment> _roadSegmentsSpatialIndex;

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

        _roadNodesSpatialIndex = new LazyQuadtree<RoadNode>(tree=>
        {
            foreach (var roadNode in _roadNodes.Values.Where(x => !x.IsRemoved))
            {
                tree.Insert(roadNode.Geometry.Value.EnvelopeInternal, roadNode);
            }
        });
        _roadSegmentsSpatialIndex = new LazyQuadtree<RoadSegment>(tree=>
        {
            foreach (var roadSegment in _roadSegments.Values.Where(x => !x.IsRemoved))
            {
                tree.Insert(
                    roadSegment.Geometry.Value.EnvelopeInternal,
                    roadSegment);
            }
        });
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

        var envelope = new Envelope(geometry.Value.EnvelopeInternal);
        envelope.ExpandBy(bufferDistance);

        var roughGeometry = geometry.Value.Envelope.Buffer(bufferDistance);
        var nearbyOtherSegments = _roadSegmentsSpatialIndex
            .Query(envelope)
            .Where(x => x.Attributes?.Status == RoadSegmentStatusV2.Gerealiseerd)
            .Where(x => !excludeRoadSegmentIds.Contains(x.RoadSegmentId))
            .Where(x => x.Geometry.Value.Intersects(roughGeometry))
            .Select(x => (x.RoadSegmentId, x.Geometry))
            .ToList();

        if (!nearbyOtherSegments.Any())
        {
            return Problems.None;
        }

        var overlappingOtherSegmentId = GetOtherRoadSegmentIdWhoOverlapsPartially(geometry, bufferDistance, nearbyOtherSegments)
                                        ?? nearbyOtherSegments
                                            .Select(otherSegment => GetOtherRoadSegmentIdWhoOverlapsPartially(otherSegment.Geometry, bufferDistance, [(otherSegment.RoadSegmentId, geometry)]))
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
        // Pre-cache buffered line segments to avoid repeated geometry operations
        var bufferedSegments = new List<Geometry>();

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
                bufferedSegments.Add(lineSegment.Buffer(bufferDistance));
            }
        }

        // Pre-cache other segment points to avoid repeated Point creation
        var otherSegmentsWithPoints = otherSegments
            .Select(otherSegment => (
                otherSegment.RoadSegmentId,
                Points: otherSegment.Geometry.Value.Coordinates
                    .Select(coord => otherSegment.Geometry.Value.Factory.CreatePoint(coord))
                    .ToList()
            ))
            .ToList();

        // Check if buffered area intersects with at least 2 vertices of another segment
        foreach (var bufferedLineSegment in bufferedSegments)
        {
            foreach (var otherSegment in otherSegmentsWithPoints)
            {
                var intersectionCount = 0;

                foreach (var _ in otherSegment.Points
                             .Where(point => point.Intersects(bufferedLineSegment)))
                {
                    intersectionCount++;
                    if (intersectionCount >= 2)
                    {
                        return otherSegment.RoadSegmentId;
                    }
                }
            }
        }

        return null;
    }

    private void RebuildSpatialIndexes(ILogger logger)
    {
        using var _ = logger.TimeAction();

        _roadNodesSpatialIndex.Rebuild();
        _roadSegmentsSpatialIndex.Rebuild();
    }

    private RoadNode? FindRoadNode(Coordinate coordinate, VerificationContextTolerances tolerance)
    {
        var envelope = new Envelope(coordinate);
        envelope.ExpandBy(tolerance.GeometryTolerance);

        var candidates = _roadNodesSpatialIndex.Query(envelope);
        var point = new Point(coordinate.X, coordinate.Y);

        return candidates.FirstOrDefault(x => x.Geometry.Value.IsReasonablyEqualTo(point, tolerance));
    }
}
