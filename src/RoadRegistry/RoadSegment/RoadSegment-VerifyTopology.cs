namespace RoadRegistry.RoadSegment;

using System.Linq;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems VerifyTopology(ScopedRoadNetworkContext context)
    {
        var idReference = context.IdTranslator.TranslateToTemporaryId(RoadSegmentId);
        var problems = Problems.WithContext(idReference);

        if (IsRemoved || Attributes?.Status != RoadSegmentStatusV2.Gerealiseerd)
        {
            return problems;
        }

        var line = Geometry.Value.GetSingleLineString();

        if (!context.RoadNetwork.RoadNodes.TryGetValue(StartNodeId, out var startNode) || startNode.IsRemoved)
        {
            problems += new RoadSegmentStartNodeMissing();
        }
        else
        {
            if (!line.StartPoint.IsReasonablyEqualTo(startNode.Geometry.Value, context.Tolerances))
            {
                problems += new RoadSegmentStartPointDoesNotMatchNodeGeometry();
            }
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(EndNodeId, out var endNode) || endNode.IsRemoved)
        {
            problems += new RoadSegmentEndNodeMissing();
        }
        else
        {
            if (!line.EndPoint.IsReasonablyEqualTo(endNode.Geometry.Value, context.Tolerances))
            {
                problems += new RoadSegmentEndPointDoesNotMatchNodeGeometry();
            }
        }

        return problems;
    }

    public bool HasTrafficOverlap(RoadSegment otherRoadSegment, Point intersection, ScopedRoadNetworkContext context)
    {
        // Get the attributes at this specific intersection point for both segments
        var segment1Access = GetAccessAtPosition(this, intersection, context.Tolerances);
        var segment2Access = GetAccessAtPosition(otherRoadSegment, intersection, context.Tolerances);

        // Check if there is overlap in traffic types (if there is overlap, NO grade junction needed)
        var hasOverlapInTrafficTypes =
            (segment1Access.CarAccess && segment2Access.CarAccess) ||
            (segment1Access.BikeAccess && segment2Access.BikeAccess) ||
            (segment1Access.PedestrianAccess && segment2Access.PedestrianAccess);
        return hasOverlapInTrafficTypes;
    }

    private static (bool CarAccess, bool BikeAccess, bool PedestrianAccess) GetAccessAtPosition(RoadSegment segment, Point position, VerificationContextTolerances tolerances)
    {
        var distance = segment.Geometry.Value.Distance(position);
        if (distance > tolerances.GeometryTolerance)
        {
            return (false, false, false);
        }

        var lineString = segment.Geometry.Value.GetSingleLineString();
        var positionAlongLine = FindPositionAlongLine(lineString, position, tolerances);
        if (positionAlongLine is null)
        {
            return (false, false, false);
        }

        var carAccess = GetAttributeValueAtPosition(segment.Attributes!.CarAccessForward, positionAlongLine.Value, tolerances)
                        || GetAttributeValueAtPosition(segment.Attributes.CarAccessBackward, positionAlongLine.Value, tolerances);
        var bikeAccess = GetAttributeValueAtPosition(segment.Attributes.BikeAccessForward, positionAlongLine.Value, tolerances)
                         || GetAttributeValueAtPosition(segment.Attributes.BikeAccessBackward, positionAlongLine.Value, tolerances);
        var pedestrianAccess = GetAttributeValueAtPosition(segment.Attributes.PedestrianAccess, positionAlongLine.Value, tolerances);

        return (carAccess, bikeAccess, pedestrianAccess);
    }

    private static bool GetAttributeValueAtPosition(
        RoadSegmentDynamicAttributeValues<bool> attributeValues,
        double positionAlongLine,
        VerificationContextTolerances tolerances)
    {
        return attributeValues.Values
            .Where(attributeValue => positionAlongLine.IsReasonablyGreaterThan(attributeValue.Coverage.From.ToDouble(), tolerances) &&
                                     positionAlongLine.IsReasonablyLessOrEqualThan(attributeValue.Coverage.To.ToDouble(), tolerances))
            .Any(x => x.Value);
    }

    private static double? FindPositionAlongLine(LineString lineString, Point point, VerificationContextTolerances tolerances)
    {
        var coordinates = lineString.Coordinates;
        var cumulativeLength = 0.0;

        for (var i = 0; i < coordinates.Length - 1; i++)
        {
            var segmentStart = new Point(coordinates[i]);
            var segment = new LineString([coordinates[i], coordinates[i + 1]]);

            if (point.IsWithinDistance(segment, tolerances.GeometryTolerance))
            {
                // Point is on this segment, calculate exact position
                var distanceAlongSegment = segmentStart.Distance(point);
                return cumulativeLength + distanceAlongSegment;
            }

            cumulativeLength += segment.Length;
        }

        return null;
    }
}
