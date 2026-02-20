namespace RoadRegistry.Tests;

using Extensions;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;

public static class RoadNetworkChangeExtensions
{
    public static RoadSegmentDynamicAttributeValues<T> ForEntireGeometry<T>(this RoadSegmentDynamicAttributeValues<T> attributeValues, RoadSegmentGeometry geometry)
        where T : notnull
    {
        if (!attributeValues.Values.Any())
        {
            return attributeValues;
        }

        var newAttributeValues = new RoadSegmentDynamicAttributeValues<T>();
        newAttributeValues.Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length.RoundToCm()), RoadSegmentAttributeSide.Both, attributeValues.Values.Single().Value);
        return newAttributeValues;
    }

    public static AddRoadSegmentChange WithDynamicAttributePositionsOnEntireGeometryLength(this AddRoadSegmentChange change)
    {
        return change with
        {
            AccessRestriction = change.AccessRestriction.ForEntireGeometry(change.Geometry),
            Category = change.Category.ForEntireGeometry(change.Geometry),
            Morphology = change.Morphology.ForEntireGeometry(change.Geometry),
            StreetNameId = change.StreetNameId.ForEntireGeometry(change.Geometry),
            MaintenanceAuthorityId = change.MaintenanceAuthorityId.ForEntireGeometry(change.Geometry),
            SurfaceType = change.SurfaceType.ForEntireGeometry(change.Geometry),
            CarAccessForward = change.CarAccessForward.ForEntireGeometry(change.Geometry),
            CarAccessBackward = change.CarAccessBackward.ForEntireGeometry(change.Geometry),
            BikeAccessForward = change.BikeAccessForward.ForEntireGeometry(change.Geometry),
            BikeAccessBackward = change.BikeAccessBackward.ForEntireGeometry(change.Geometry),
            PedestrianAccess = change.PedestrianAccess.ForEntireGeometry(change.Geometry)
        };
    }
}
