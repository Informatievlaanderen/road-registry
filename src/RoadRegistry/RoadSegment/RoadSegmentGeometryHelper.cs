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

    public static MultiLineString MergeGeometries(
        RoadSegment segment1, RoadSegment segment2,
        RoadNodeId commonNodeId,
        ScopedRoadNetworkContext context)
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

        var geometryWhichMustContainNode = ExtractLoop(geometry.GetSingleLineString(), isValidOp.NonSimpleLocation, tolerances.GeometryTolerance);
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

    private static LineString ExtractLoop(LineString geometry, Coordinate intersectionPoint, double tolerance)
    {
        var coords = geometry.Coordinates;
        var factory = geometry.Factory;

        // Find all segments that contain the intersection point
        var segmentIndices = new List<int>();

        for (var i = 0; i < coords.Length - 1; i++)
        {
            var segment = new LineSegment(coords[i], coords[i + 1]);
            var distance = segment.Distance(intersectionPoint);

            if (distance < tolerance)
            {
                segmentIndices.Add(i);
            }
        }

        if (segmentIndices.Count < 2)
        {
            throw new InvalidOperationException($"No loop found. Intersection point: {intersectionPoint.X.ToRoundedMeasurementString()} {intersectionPoint.Y.ToRoundedMeasurementString()}");
        }

        // Build the loop coordinates
        var loopCoords = new List<Coordinate>();

        // Add the intersection point at the start
        loopCoords.Add(intersectionPoint.Copy());

        // Add all coordinates between the two segments
        for (var i = segmentIndices[0] + 1; i <= segmentIndices[1]; i++)
        {
            loopCoords.Add(coords[i].Copy());
        }

        // Close the loop with the intersection point
        loopCoords.Add(intersectionPoint.Copy());

        return factory.CreateLineString(loopCoords.ToArray());
    }
}

public sealed record InvalidGeometrySection(Coordinate Intersection, LineString GeometrySection, Point IdealNodeLocation);
