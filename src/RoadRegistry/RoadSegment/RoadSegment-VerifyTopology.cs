namespace RoadRegistry.RoadSegment;

using System.Linq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems VerifyTopology(ScopedRoadNetworkChangeContext context)
    {
        var idReference = context.IdTranslator.TranslateToTemporaryId(RoadSegmentId);
        var problems = Problems.WithContext(idReference);
        var line = Geometry.Value.GetSingleLineString();

        if (StartNodeId is not null)
        {
            if (!context.RoadNetwork.RoadNodes.TryGetValue(StartNodeId.Value, out var startNode) || startNode.IsRemoved)
            {
                problems += new RoadSegmentStartNodeMissing();
            }
            else
            {
                if (!line.StartPoint.EqualsExact(startNode.Geometry.Value))
                {
                    problems += new RoadSegmentStartPointDoesNotMatchNodeGeometry();
                }
            }
        }

        if (EndNodeId is not null)
        {
            if (!context.RoadNetwork.RoadNodes.TryGetValue(EndNodeId.Value, out var endNode) || endNode.IsRemoved)
            {
                problems += new RoadSegmentEndNodeMissing();
            }
            else
            {
                if (!line.EndPoint.EqualsExact(endNode.Geometry.Value))
                {
                    problems += new RoadSegmentEndPointDoesNotMatchNodeGeometry();
                }
            }
        }

        problems += context.RoadNetwork.ValidatePartiallyOverlappingRoadSegments(Geometry, [RoadSegmentId], context.IdTranslator);

        return problems;
    }

    public bool HasTrafficOverlap(RoadSegment otherRoadSegment, Point intersection, ScopedRoadNetworkChangeContext context)
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

        var carAccess = GetAttributeValueAtPosition(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(segment.Attributes?.CarTrafficDirection ?? new()), positionAlongLine.Value, tolerances)
                        || GetAttributeValueAtPosition(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(segment.Attributes?.CarTrafficDirection ?? new()), positionAlongLine.Value, tolerances);
        var bikeAccess = GetAttributeValueAtPosition(RoadSegmentTrafficDirectionTranslation.ToForwardAccess(segment.Attributes?.BikeTrafficDirection ?? new()), positionAlongLine.Value, tolerances)
                         || GetAttributeValueAtPosition(RoadSegmentTrafficDirectionTranslation.ToBackwardAccess(segment.Attributes?.BikeTrafficDirection ?? new()), positionAlongLine.Value, tolerances);
        var pedestrianAccess = GetAttributeValueAtPosition(RoadSegmentTrafficDirectionTranslation.ToPedestrianAccess(segment.Attributes?.PedestrianTrafficDirection ?? new()), positionAlongLine.Value, tolerances);

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
