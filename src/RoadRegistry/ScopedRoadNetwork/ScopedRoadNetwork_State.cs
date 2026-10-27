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
using RoadRegistry.RoadSegment.ValueObjects;
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
        : base(roadNetworkId, EventOrdinalProvider.None)
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

        _roadNodesSpatialIndex = new LazyQuadtree<RoadNode>(tree =>
        {
            foreach (var roadNode in _roadNodes.Values.Where(x => !x.IsRemoved))
            {
                tree.Insert(roadNode.Geometry.Value.EnvelopeInternal, roadNode);
            }
        });
        _roadSegmentsSpatialIndex = new LazyQuadtree<RoadSegment>(tree =>
        {
            foreach (var roadSegment in _roadSegments.Values.Where(x => !x.IsRemoved))
            {
                tree.Insert(
                    roadSegment.Geometry.Value.EnvelopeInternal,
                    roadSegment);
            }
        });
    }

    // Attaches the change's single ordinal provider to the network and every aggregate it currently holds, so
    // that all events subsequently raised during the change are stamped from one monotonic counter. Aggregates
    // created later in the change (via RoadNode.Add / RoadSegment.Add / GradeSeparatedJunction.Add /
    // GradeJunction.Add) receive the provider through the context.
    internal override void AttachOrdinalProvider(IEventOrdinalProvider ordinalProvider)
    {
        base.AttachOrdinalProvider(ordinalProvider);

        foreach (var roadNode in _roadNodes.Values)
        {
            roadNode.AttachOrdinalProvider(ordinalProvider);
        }
        foreach (var roadSegment in _roadSegments.Values)
        {
            roadSegment.AttachOrdinalProvider(ordinalProvider);
        }
        foreach (var gradeSeparatedJunction in _gradeSeparatedJunctions.Values)
        {
            gradeSeparatedJunction.AttachOrdinalProvider(ordinalProvider);
        }
        foreach (var gradeJunction in _gradeJunctions.Values)
        {
            gradeJunction.AttachOrdinalProvider(ordinalProvider);
        }
    }

    public static ScopedRoadNetwork Create(RoadNetworkWasChanged @event)
    {
        return CreateFrom(@event.RoadNetworkId, @event.Summary);
    }

    private static ScopedRoadNetwork CreateFrom(ScopedRoadNetworkId roadNetworkId, RoadNetworkChangedSummary summary)
    {
        return new ScopedRoadNetwork(roadNetworkId)
        {
            SummaryOfLastChange = summary.ToRoadNetworkChangesSummary()
        };
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

    public (RoadSegmentNodeIds NodeIds, Problems Problems) FindStartEndNodes(
        RoadSegmentGeometry geometry)
    {
        var problems = Problems.None;

        var startNodeId = FindRoadNode(geometry.Value.Coordinate)?.RoadNodeId;
        if (startNodeId is null)
        {
            problems += new RoadSegmentStartNodeMissing();
        }

        var endNodeId = FindRoadNode(geometry.Value.Coordinates.Last())?.RoadNodeId;
        if (endNodeId is null)
        {
            problems += new RoadSegmentEndNodeMissing();
        }

        return (new RoadSegmentNodeIds { Start = startNodeId, End = endNodeId }, problems);
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
            .Where(x => x.Status == RoadSegmentStatusV2.Gerealiseerd)
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
                var intersectionPoints = otherSegment.Points
                    .Where(point => point.Intersects(bufferedLineSegment))
                    .Take(3)
                    .ToArray();

                if (intersectionPoints.Length > 1)
                {
                    // Special case: exclude if both intersection points are all end vertices of both segments
                    if (intersectionPoints.Length == 2)
                    {
                        var selfStartEndCoordinates = new[] { selfGeometry.Value.Coordinates.First(), selfGeometry.Value.Coordinates.Last() };
                        var otherStartEndCoordinates = new[] { otherSegment.Points.First().Coordinate, otherSegment.Points.Last().Coordinate };

                        if (intersectionPoints.All(ip => selfStartEndCoordinates.Any(c => c.IsReasonablyEqualTo(ip.Coordinate, bufferDistance)))
                            && intersectionPoints.All(ip => otherStartEndCoordinates.Any(c => c.IsReasonablyEqualTo(ip.Coordinate, bufferDistance))))
                        {
                            continue;
                        }
                    }

                    return otherSegment.RoadSegmentId;
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

    // An inwinning is read in on the centimetre - RoundToCm, all through the DomainV2 feature compare - while a road
    // node the register has not migrated yet still sits where V1 put it, down to the last double. Within a
    // municipality that never shows: the inwinning collects its own road nodes, so the same upload rewrites them on
    // the centimetre. Across a border it does: the road node there belongs to the neighbour, is not collected, and
    // comparing a rounded coordinate with an unrounded one finds nothing - leaving the road segment that runs up to
    // it reported as missing a road node that is plainly there (GAWR-7827).
    //
    // So both sides go onto the centimetre grid before they are compared. Rounding, not a distance tolerance: two
    // coordinates are then equal or they are not. A tolerance is not transitive - a road node within a centimetre of
    // two others that are two centimetres apart - and the answer would come to depend on which candidate the spatial
    // index handed over first.
    private RoadNode? FindRoadNode(Coordinate coordinate)
    {
        // Wide enough to reach the road node as it is stored, which rounding can move by up to half a centimetre in
        // each direction.
        var envelope = new Envelope(coordinate);
        envelope.ExpandBy(0.01);

        return _roadNodesSpatialIndex
            .Query(envelope)
            .FirstOrDefault(x => x.Geometry.Value.Coordinate.EqualsOnTheCentimetre(coordinate));
    }
}
