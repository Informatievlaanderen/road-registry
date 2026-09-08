namespace RoadRegistry.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Valid;
using RoadRegistry.BackOffice;
using RoadRegistry.Extensions;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;

public static class RoadSegmentGeometryHelper
{
    public static RoadSegmentGeometryDrawMethodV2? DetermineMethod(
        IReadOnlyCollection<(MultiLineString Geometry, RoadSegmentGeometryDrawMethodV2? Method)> segments,
        MultiLineString mergedGeometry)
    {
        if (segments.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segments), "Must have at least one segment");
        }

        if (segments.Any(x => x.Method is null))
        {
            return null;
        }

        var ingemetenTotalLength = segments
            .Where(x => x.Method == RoadSegmentGeometryDrawMethodV2.Ingemeten)
            .Sum(x => x.Geometry.Length);

        var ingemetenPercentage = ingemetenTotalLength / mergedGeometry.Length;
        if (ingemetenPercentage >= RoadSegmentConstants.MinimumPercentageForIngemeten)
        {
            return RoadSegmentGeometryDrawMethodV2.Ingemeten;
        }

        return RoadSegmentGeometryDrawMethodV2.Ingeschetst;
    }

    // How far along a road the direction it leaves a node in is measured.
    //
    // It is the minimum distance between two vertices, which makes it the shortest probe that is guaranteed to land on
    // real geometry rather than between two coincident points - and, because a road's turn can begin within the first
    // metre, the largest one that still measures the departure direction instead of where the road has already turned
    // to. The cost is angular noise: coordinates are stored to the centimetre, so one centimetre of jitter on the
    // first vertex rotates the bearing by atan(0.01 / 0.15) = 3.8 degrees. Roads that leave a node less than about
    // four degrees apart therefore cannot be ordered by this reliably.
    private static readonly double BearingProbeDistance = Distances.MinimumDistanceBetweenVertices;

    // The direction a road leaves a node in, in radians [0, 2pi), whichever way the road was digitised.
    //
    // It is measured close to the node on purpose: a road is judged on how it leaves, not on where it ends up. A road
    // that heads west out of the node and then swings north to end north-east of it - a perfectly normal bridge
    // situation - leaves westwards, and taking the bearing from its far end would say otherwise.
    public static double BearingAwayFromNode(LineString geometry, Coordinate node, double? probeDistance = null)
    {
        var awayFromNode = geometry.StartPoint.Coordinate.Distance(node) <= geometry.EndPoint.Coordinate.Distance(node)
            ? geometry
            : (LineString)geometry.Reverse();

        // Clamped only as a backstop: a valid road segment is a metre long at least, so this cannot bite unless the
        // caller is handed legacy or not-yet-validated geometry.
        var probe = new LengthIndexedLine(awayFromNode)
            .ExtractPoint(Math.Min(probeDistance ?? BearingProbeDistance, awayFromNode.Length));

        var angle = Math.Atan2(probe.Y - node.Y, probe.X - node.X);
        return angle < 0 ? angle + 2 * Math.PI : angle;
    }

    // The two pairs of roads that lie across from one another at a four-way node.
    //
    // The rule is purely positional, whatever the angles: order the roads around the node and pair each one with the
    // road that is neither its clockwise nor its counter-clockwise neighbour. Exactly one road qualifies, which is why
    // this needs a four-way node - with three every other road is a neighbour, with five there are two non-neighbours.
    //
    // Drawing a circle around the node and walking its border computes precisely that ordering, and "skip one, take
    // the next" is "not a neighbour". So the ordering can be taken directly from the bearing of each road as it leaves
    // the node: no buffer, no boundary intersection, no radius to tune, and nothing to go wrong when a road is shorter
    // than the radius or crosses the circle more than once.
    public static IReadOnlyList<(T First, T Second)> PairOppositeLegs<T>(
        IReadOnlyList<T> legs,
        Func<T, LineString> getGeometry,
        Coordinate node,
        double? probeDistance = null)
    {
        if (legs.Count != 4)
        {
            throw new ArgumentException($"An opposite road is only well defined on a four-way node, got {legs.Count}.", nameof(legs));
        }

        var ordered = legs
            .OrderBy(leg => BearingAwayFromNode(getGeometry(leg), node, probeDistance))
            .ToArray();

        return [(ordered[0], ordered[2]), (ordered[1], ordered[3])];
    }

    public static MultiLineString MergeGeometries(
        RoadSegment segment1, RoadSegment segment2,
        RoadNodeId commonNodeId,
        ScopedRoadNetworkChangeContext context)
    {
        var startNodeId = segment1.GetOppositeNode(commonNodeId)!.Value;
        var endNodeId = segment2.GetOppositeNode(commonNodeId)!.Value;

        var segment1HasIdealDirection = startNodeId == segment1.StartNodeId;
        var segment2HasIdealDirection = endNodeId == segment2.EndNodeId;
        var geometry1Coordinates = segment1.Geometry.Value.GetSingleLineString().Coordinates;
        var geometry2Coordinates = segment2.Geometry.Value.GetSingleLineString().Coordinates;

        var startNodeCoordinate = context.RoadNetwork.RoadNodes[startNodeId].Geometry.Value.Coordinate;
        var endNodeCoordinate = context.RoadNetwork.RoadNodes[endNodeId].Geometry.Value.Coordinate;
        var commonNodeCoordinate = context.RoadNetwork.RoadNodes[commonNodeId].Geometry.Value.Coordinate;

        var coordinates = Enumerable.Empty<Coordinate>()
            .Concat([startNodeCoordinate])
            .Concat((segment1HasIdealDirection ? geometry1Coordinates : geometry1Coordinates.Reverse()).ExcludeFirstAndLast())
            .Concat([commonNodeCoordinate])
            .Concat((segment2HasIdealDirection ? geometry2Coordinates : geometry2Coordinates.Reverse()).ExcludeFirstAndLast())
            .Concat([endNodeCoordinate])
            .ToArray();

        return new MultiLineString([new LineString(coordinates)])
            .WithSrid(segment1.Geometry.SRID);
    }

    public static InvalidGeometrySection? GetSelfIntersectingInvalidGeometrySection(
        MultiLineString geometry,
        VerificationContextTolerances tolerances)
    {
        if (!geometry.SelfIntersects())
        {
            return null;
        }

        var isValidOp = new IsSimpleOp(geometry);
        if (isValidOp.IsSimple())
        {
            throw new InvalidOperationException("IsSimpleOp.IsSimple() returns true for self-intersecting geometry. This should not happen.");
        }

        var geometryWhichMustContainNode = ExtractLoop(geometry.GetSingleLineString(), isValidOp.NonSimpleLocation, tolerances);
        var loopIndexedLine = new LengthIndexedLine(geometryWhichMustContainNode);
        var middlePoint = loopIndexedLine.ExtractPoint(geometryWhichMustContainNode.Length / 2.0);

        return new InvalidGeometrySection(isValidOp.NonSimpleLocation, geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint));
    }

    public static InvalidGeometrySection? GetSameStartEndNodeInvalidGeometrySection(MultiLineString geometry, VerificationContextTolerances tolerances)
    {
        var coords = geometry.Coordinates;
        var startPoint = coords.First();
        var endPoint = coords.Last();

        if (startPoint.IsReasonablyEqualTo(endPoint, tolerances))
        {
            var geometryLengthIndexedLine = new LengthIndexedLine(geometry);
            var geometryWhichMustContainNode = (LineString)geometryLengthIndexedLine.ExtractLine(tolerances.GeometryTolerance, geometry.Length - tolerances.GeometryTolerance);
            var middlePoint = geometryLengthIndexedLine.ExtractPoint(geometry.Length / 2.0);

            return new InvalidGeometrySection(startPoint, geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint));
        }

        return null;
    }

    public static InvalidGeometrySection? GetFirstMultipleIntersectionsInvalidGeometrySection(
        MultiLineString geometry,
        MultiLineString otherGeometry,
        VerificationContextTolerances tolerances)
    {
        if (!geometry.Intersects(otherGeometry))
        {
            return null;
        }

        var startEndCoordinates = new[] { geometry.Coordinates.First(), geometry.Coordinates.Last() };
        var intersectionCoordinates = geometry.Intersection(otherGeometry).Coordinates
            .Where(x => startEndCoordinates.All(startEndCoordinate => !startEndCoordinate.IsReasonablyEqualTo(x, tolerances))) // exclude start-end coordinates
            .ToArray();
        if (intersectionCoordinates.Length < 2)
        {
            return null;
        }

        var geometryLengthIndexedLine = new LengthIndexedLine(geometry);
        var intersectionPositions = intersectionCoordinates
            .Select(p => geometryLengthIndexedLine.IndexOf(p))
            .OrderBy(x => x)
            .ToList();

        var startPosition = intersectionPositions[0];
        var endPosition = intersectionPositions[1];

        var geometryWhichMustContainNode = (LineString)geometryLengthIndexedLine.ExtractLine(startPosition + tolerances.GeometryTolerance, endPosition - tolerances.GeometryTolerance);
        var middlePoint = geometryLengthIndexedLine.ExtractPoint((endPosition - startPosition) / 2.0);

        return new InvalidGeometrySection(intersectionCoordinates[0], geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint));
    }

    private static LineString ExtractLoop(LineString geometry, Coordinate intersectionPoint, VerificationContextTolerances tolerances)
    {
        var coords = geometry.Coordinates;
        var factory = geometry.Factory;

        // Build the loop coordinates
        var loopCoords = new List<Coordinate>();
        var isInLoop = false;

        Coordinate? previousCoord = null;
        for (var i = 0; i < coords.Length - 1; i++)
        {
            var currentCoord = coords[i];
            if (previousCoord is not null && previousCoord.IsReasonablyEqualTo(currentCoord, tolerances))
            {
                continue;
            }

            if (loopCoords.Count > 0 && loopCoords[^1].IsReasonablyEqualTo(currentCoord, tolerances))
            {
                continue;
            }

            var nextCoord = coords[i + 1];

            var segment = new LineSegment(currentCoord, nextCoord);
            var distance = segment.Distance(intersectionPoint);

            if (distance < tolerances.GeometryTolerance)
            {
                if (isInLoop)
                {
                    // end the loop
                    isInLoop = false;
                    if (!intersectionPoint.IsReasonablyEqualTo(currentCoord, tolerances))
                    {
                        loopCoords.Add(currentCoord.Copy());
                    }
                    loopCoords.Add(intersectionPoint.Copy());
                    break;
                }

                // start the loop
                isInLoop = true;
                loopCoords.Add(intersectionPoint.Copy());
                if (!intersectionPoint.IsReasonablyEqualTo(nextCoord, tolerances))
                {
                    loopCoords.Add(nextCoord.Copy());
                }
            }
            else if (isInLoop)
            {
                // add coord to loop
                loopCoords.Add(currentCoord.Copy());
            }

            previousCoord = currentCoord;
        }

        if (isInLoop || loopCoords.Count == 0)
        {
            throw new InvalidOperationException($"No loop found. Intersection point: {intersectionPoint.X.ToRoundedMeasurementString()} {intersectionPoint.Y.ToRoundedMeasurementString()}");
        }

        return factory.CreateLineString(loopCoords.ToArray());
    }
}

public sealed record InvalidGeometrySection(Coordinate Intersection, LineString GeometrySection, Point IdealNodeLocation);
