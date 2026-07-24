namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // Knippen op kruising: split two road segments that cross each other at a (grade or grade-separated) junction, on the
    // crossing point. The junction is removed and a real road node (grensknoop=false) is inserted at the crossing, so the
    // (up to four) resulting parts share that node. Existing attributes/road numbers carry over; grade-separated junctions
    // that referenced an original segment are re-pointed to the correct new part.
    public IReadOnlyList<RoadSegmentId> SplitRoadSegmentsByJunction(
        RoadSegmentId roadSegmentId1,
        RoadSegmentId roadSegmentId2,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        var problems = Problems.None;

        // VAL-2: both must be existing, non-removed road segments. Re-validated here because the domain can be invoked
        // directly and the API request may have become stale in the meantime.
        if (!_roadSegments.TryGetValue(roadSegmentId1, out var segment1) || segment1.IsRemoved)
        {
            problems += new RoadSegmentsSplitByJunctionRoadSegmentNotFound(roadSegmentId1);
        }

        if (!_roadSegments.TryGetValue(roadSegmentId2, out var segment2) || segment2.IsRemoved)
        {
            problems += new RoadSegmentsSplitByJunctionRoadSegmentNotFound(roadSegmentId2);
        }

        problems.ThrowIfError();

        // VAL-3: both must have status 'gerealiseerd'.
        if (segment1!.Status != RoadSegmentStatusV2.Gerealiseerd)
        {
            problems += new RoadSegmentsSplitByJunctionStatusNotValid(roadSegmentId1);
        }

        if (segment2!.Status != RoadSegmentStatusV2.Gerealiseerd)
        {
            problems += new RoadSegmentsSplitByJunctionStatusNotValid(roadSegmentId2);
        }

        problems.ThrowIfError();

        // VAL-4: a grade (gelijkgrondse) or grade-separated (ongelijkgrondse) junction must exist between the two segments.
        var junction = FindJunctionBetween(roadSegmentId1, roadSegmentId2);
        if (junction?.Geometry is null)
        {
            problems += new RoadSegmentsSplitByJunctionNoJunctionBetweenRoadSegments(roadSegmentId1, roadSegmentId2);
            problems.ThrowIfError();
        }

        var sharedNodeCoordinate = junction!.Value.Geometry!.Value.Coordinate;

        var idTranslator = new IdentifierTranslator();
        var context = new ScopedRoadNetworkChangeContext(this, idTranslator, provenance, logger);

        // VAL-5: the crossing must lie at least 1m (measured along each segment) from the begin and end node of both
        // segments. Validate both before applying any change.
        var cut1 = PlanCut(segment1, sharedNodeCoordinate);
        var cut2 = PlanCut(segment2, sharedNodeCoordinate);

        RebuildSpatialIndexes(logger);

        // A single real road node (grensknoop=false) is inserted at the crossing; both segments' cut endpoints are forced
        // to its coordinate so the resulting parts resolve to this shared node.
        problems += AddRoadNode(new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(1),
            Geometry = RoadNodeGeometry.Create(segment1.Geometry.Value.Factory.CreatePoint(sharedNodeCoordinate).WithSrid(segment1.Geometry.SRID)),
            Grensknoop = false
        }, idGenerator, context);
        problems.ThrowIfError();

        var result1 = SplitSegmentAtCut(segment1, roadSegmentId1, cut1, sharedNodeCoordinate, idGenerator, provenance, context);
        var result2 = SplitSegmentAtCut(segment2, roadSegmentId2, cut2, sharedNodeCoordinate, idGenerator, provenance, context);

        // Remove the junction: the two segments now meet at a real node instead of crossing.
        if (junction.Value.GradeJunctionId is { } gradeJunctionId)
        {
            problems += RemoveGradeJunction(gradeJunctionId, context);
        }
        else if (junction.Value.GradeSeparatedJunctionId is { } gradeSeparatedJunctionId)
        {
            problems += RemoveGradeSeparatedJunction(new RemoveGradeSeparatedJunctionChange { GradeSeparatedJunctionId = gradeSeparatedJunctionId }, context);
        }

        problems.ThrowIfError();

        // Re-point junctions that referenced either original segment to the correct new part (A -> E), both for
        // grade-separated (ongelijkgrondse) and grade (gelijkgrondse) junctions.
        ReassignGradeSeparatedJunctions(roadSegmentId1, result1.FirstPartRoadSegmentId, result1.SecondPartRoadSegmentId, context);
        ReassignGradeSeparatedJunctions(roadSegmentId2, result2.FirstPartRoadSegmentId, result2.SecondPartRoadSegmentId, context);
        ReassignGradeJunctions(roadSegmentId1, result1.FirstPartRoadSegmentId, result1.SecondPartRoadSegmentId, context);
        ReassignGradeJunctions(roadSegmentId2, result2.FirstPartRoadSegmentId, result2.SecondPartRoadSegmentId, context);

        problems += VerifyAndUpdateJunctions(idGenerator, context);
        problems.ThrowIfError();

        return [.. result1.ResultRoadSegmentIds, .. result2.ResultRoadSegmentIds];
    }

    private (GradeJunctionId? GradeJunctionId, GradeSeparatedJunctionId? GradeSeparatedJunctionId, JunctionGeometry? Geometry)? FindJunctionBetween(
        RoadSegmentId roadSegmentId1,
        RoadSegmentId roadSegmentId2)
    {
        var gradeJunction = _gradeJunctions.Values
            .FirstOrDefault(x => !x.IsRemoved && x.IsConnectedTo(roadSegmentId1) && x.IsConnectedTo(roadSegmentId2));
        if (gradeJunction is not null)
        {
            return (gradeJunction.GradeJunctionId, null, gradeJunction.Geometry);
        }

        var gradeSeparatedJunction = _gradeSeparatedJunctions.Values
            .FirstOrDefault(x => !x.IsRemoved && x.IsConnectedTo(roadSegmentId1) && x.IsConnectedTo(roadSegmentId2));
        if (gradeSeparatedJunction is not null)
        {
            return (null, gradeSeparatedJunction.GradeSeparatedJunctionId, gradeSeparatedJunction.Geometry);
        }

        return null;
    }

    // Mirrors ReassignGradeSeparatedJunctions for grade (gelijkgrondse) junctions: any grade junction that still
    // references the original segment (e.g. at a second crossing K2 with a third segment) is re-pointed to the part that
    // actually intersects the other segment.
    private void ReassignGradeJunctions(
        RoadSegmentId originalRoadSegmentId,
        RoadSegmentId firstPartRoadSegmentId,
        RoadSegmentId secondPartRoadSegmentId,
        ScopedRoadNetworkChangeContext context)
    {
        var connectedJunctions = _gradeJunctions
            .Where(x => !x.Value.IsRemoved && x.Value.IsConnectedTo(originalRoadSegmentId))
            .Select(x => x.Value)
            .ToArray();

        foreach (var junction in connectedJunctions)
        {
            RoadSegmentId? newRoadSegmentId1 = null;
            RoadSegmentId? newRoadSegmentId2 = null;
            var changed = false;

            if (junction.RoadSegmentId1 == originalRoadSegmentId)
            {
                var newId = PickIntersectingPart(junction.RoadSegmentId2, firstPartRoadSegmentId, secondPartRoadSegmentId);
                if (newId != junction.RoadSegmentId1)
                {
                    newRoadSegmentId1 = newId;
                    changed = true;
                }
            }

            if (junction.RoadSegmentId2 == originalRoadSegmentId)
            {
                var newId = PickIntersectingPart(junction.RoadSegmentId1, firstPartRoadSegmentId, secondPartRoadSegmentId);
                if (newId != junction.RoadSegmentId2)
                {
                    newRoadSegmentId2 = newId;
                    changed = true;
                }
            }

            if (!changed)
            {
                continue;
            }

            junction.Modify(newRoadSegmentId1, newRoadSegmentId2, context.Provenance);
            context.Summary.GradeJunctions.Modified.Add(junction.GradeJunctionId);
        }
    }

    // Computes where a segment is cut for the crossing coordinate and enforces VAL-5 (>= 1m from begin/end node).
    private CutPlan PlanCut(RoadRegistry.RoadSegment.RoadSegment segment, Coordinate crossingCoordinate)
    {
        var lineString = segment.Geometry.Value.GetSingleLineString();
        var totalLength = lineString.Length;

        var indexedLine = new LengthIndexedLine(lineString);
        var cutMeasure = indexedLine.Project(crossingCoordinate).RoundToCm();

        var minimumDistance = Distances.RoadSegmentSplitMinimumDistanceToRoadNode;
        if (cutMeasure < minimumDistance)
        {
            throw new RoadRegistryProblemsException(Problems.Single(new RoadSegmentSplitPositionTooCloseToRoadNode(segment.StartNodeId ?? RoadNodeId.Zero, minimumDistance)));
        }

        if (totalLength - cutMeasure < minimumDistance)
        {
            throw new RoadRegistryProblemsException(Problems.Single(new RoadSegmentSplitPositionTooCloseToRoadNode(segment.EndNodeId ?? RoadNodeId.Zero, minimumDistance)));
        }

        return new CutPlan(lineString, indexedLine, totalLength, cutMeasure);
    }

    // Splits one Gerealiseerd segment at the (already validated) crossing, forcing the shared endpoint onto the shared
    // node coordinate. Mirrors ScopedRoadNetwork.SplitRoadSegment's per-segment logic but reuses the pre-created shared node.
    private SplitResult SplitSegmentAtCut(
        RoadRegistry.RoadSegment.RoadSegment segment,
        RoadSegmentId roadSegmentId,
        CutPlan cut,
        Coordinate sharedNodeCoordinate,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var lineString = cut.LineString;
        var indexedLine = cut.IndexedLine;
        var totalLength = cut.TotalLength;
        var cutMeasure = cut.CutMeasure;

        var startCoordinate = lineString.Coordinates[0];
        var endCoordinate = lineString.Coordinates[^1];

        var firstCoordinates = ((LineString)indexedLine.ExtractLine(0, cutMeasure)).Coordinates;
        firstCoordinates[0] = startCoordinate;
        firstCoordinates[^1] = sharedNodeCoordinate;

        var secondCoordinates = ((LineString)indexedLine.ExtractLine(cutMeasure, totalLength)).Coordinates;
        secondCoordinates[0] = sharedNodeCoordinate;
        secondCoordinates[^1] = endCoordinate;

        var firstGeometry = new MultiLineString([lineString.Factory.CreateLineString(firstCoordinates)])
            .WithSrid(segment.Geometry.SRID)
            .RoundToCm()
            .ToRoadSegmentGeometry();
        var secondGeometry = new MultiLineString([lineString.Factory.CreateLineString(secondCoordinates)])
            .WithSrid(segment.Geometry.SRID)
            .RoundToCm()
            .ToRoadSegmentGeometry();

        var cutPositionMeasure = new RoadSegmentPositionV2(cutMeasure);

        var originalStatus = segment.Status;
        var attributes = segment.Attributes!;
        var accessRestriction = attributes.AccessRestriction.SplitAt(cutPositionMeasure, totalLength);
        var category = attributes.Category.SplitAt(cutPositionMeasure, totalLength);
        var morphology = attributes.Morphology.SplitAt(cutPositionMeasure, totalLength);
        var streetNameId = attributes.StreetNameId.SplitAt(cutPositionMeasure, totalLength);
        var maintenanceAuthorityId = attributes.MaintenanceAuthorityId.SplitAt(cutPositionMeasure, totalLength);
        var surfaceType = attributes.SurfaceType.SplitAt(cutPositionMeasure, totalLength);
        var carTrafficDirection = attributes.CarTrafficDirection.SplitAt(cutPositionMeasure, totalLength);
        var bikeTrafficDirection = attributes.BikeTrafficDirection.SplitAt(cutPositionMeasure, totalLength);
        var pedestrianTrafficDirection = attributes.PedestrianTrafficDirection.SplitAt(cutPositionMeasure, totalLength);

        var firstPart = new SplitPart(firstGeometry, accessRestriction.First, category.First, morphology.First, streetNameId.First, maintenanceAuthorityId.First, surfaceType.First, carTrafficDirection.First, bikeTrafficDirection.First, pedestrianTrafficDirection.First);
        var secondPart = new SplitPart(secondGeometry, accessRestriction.Second, category.Second, morphology.Second, streetNameId.Second, maintenanceAuthorityId.Second, surfaceType.Second, carTrafficDirection.Second, bikeTrafficDirection.Second, pedestrianTrafficDirection.Second);

        var firstLength = cutMeasure;
        var secondLength = totalLength - cutMeasure;
        var keepIdentifier = System.Math.Max(firstLength, secondLength) / totalLength >= RoadRegistry.RoadSegment.RoadSegmentConstants.MinimumPercentageToKeepIdentifier;

        RoadSegmentId firstPartRoadSegmentId, secondPartRoadSegmentId;
        IReadOnlyList<RoadSegmentId> resultRoadSegmentIds;

        if (keepIdentifier)
        {
            // Situation 2: the largest part keeps the original identifier (modified in place); only the smallest is added.
            var firstIsLongest = firstLength >= secondLength;
            var longestPart = firstIsLongest ? firstPart : secondPart;
            var shortestPart = firstIsLongest ? secondPart : firstPart;

            problems += AddRoadSegment(BuildAddChange(new RoadSegmentIdReference(new RoadSegmentId(1)), originalStatus, attributes, shortestPart), idGenerator, context);
            problems.ThrowIfError();
            var shortestRoadSegmentId = context.Summary.RoadSegments.Added.Last();

            var startEndNodes = FindStartEndNodes(longestPart.Geometry);
            problems += startEndNodes.Problems;
            problems.ThrowIfError();

            var oldEnvelope = segment.Geometry.Value.EnvelopeInternal;
            problems += segment.Split([roadSegmentId, shortestRoadSegmentId], BuildModifications(longestPart, startEndNodes.StartNodeId, startEndNodes.EndNodeId), provenance);
            problems.ThrowIfError();
            _roadSegmentsSpatialIndex.Update(oldEnvelope, segment.Geometry.Value.EnvelopeInternal, segment);
            context.Summary.RoadSegments.Modified.Add(roadSegmentId);

            firstPartRoadSegmentId = firstIsLongest ? roadSegmentId : shortestRoadSegmentId;
            secondPartRoadSegmentId = firstIsLongest ? shortestRoadSegmentId : roadSegmentId;
            resultRoadSegmentIds = [roadSegmentId, shortestRoadSegmentId];
        }
        else
        {
            // Situation 1: both parts are smaller than the keep-identifier threshold; the original is historized and two
            // new segments are added.
            problems += segment.RetireBecauseOfSplit(provenance);
            problems.ThrowIfError();
            _roadSegmentsSpatialIndex.Remove(segment.Geometry.Value.EnvelopeInternal, segment);
            context.Summary.RoadSegments.Removed.Add(roadSegmentId);

            problems += AddRoadSegment(BuildAddChange(new RoadSegmentIdReference(new RoadSegmentId(1)), originalStatus, attributes, firstPart), idGenerator, context);
            problems.ThrowIfError();
            firstPartRoadSegmentId = context.Summary.RoadSegments.Added.Last();

            problems += AddRoadSegment(BuildAddChange(new RoadSegmentIdReference(new RoadSegmentId(2)), originalStatus, attributes, secondPart), idGenerator, context);
            problems.ThrowIfError();
            secondPartRoadSegmentId = context.Summary.RoadSegments.Added.Last();

            problems += segment.Split([firstPartRoadSegmentId, secondPartRoadSegmentId], null, provenance);
            problems.ThrowIfError();

            resultRoadSegmentIds = [firstPartRoadSegmentId, secondPartRoadSegmentId];
        }

        return new SplitResult(firstPartRoadSegmentId, secondPartRoadSegmentId, resultRoadSegmentIds);
    }

    private sealed record CutPlan(LineString LineString, LengthIndexedLine IndexedLine, double TotalLength, double CutMeasure);

    private sealed record SplitResult(RoadSegmentId FirstPartRoadSegmentId, RoadSegmentId SecondPartRoadSegmentId, IReadOnlyList<RoadSegmentId> ResultRoadSegmentIds);
}
