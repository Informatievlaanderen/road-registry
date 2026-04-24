namespace RoadRegistry.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
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

    private static Coordinate[] MergeSegmentsCoordinates(IEnumerable<Coordinate[]> segments, VerificationContextTolerances tolerances)
    {
        var coordinates = new List<Coordinate>();

        foreach (var segment in segments)
        {
            foreach (var coordinate in segment)
            {
                if (coordinates.Count == 0 || !coordinate.IsReasonablyEqualTo(coordinates.Last(), tolerances))
                {
                    coordinates.Add(coordinate);
                }
            }
        }

        return coordinates.ToArray();
    }
}

public sealed record MergeRoadSegmentsResult
{
    public required RoadSegmentGeometry Geometry { get; init; }
    public required IReadOnlyCollection<RoadSegment> RoadSegments { get; init; }
}
