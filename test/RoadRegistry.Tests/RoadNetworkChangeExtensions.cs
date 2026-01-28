namespace RoadRegistry.Tests;

using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;

public static class RoadNetworkChangeExtensions
{
    public static RoadSegmentDynamicAttributeValues<T> OnEntireGeometry<T>(this RoadSegmentDynamicAttributeValues<T> attributeValues, RoadSegmentGeometry geometry)
        where T : notnull
    {
        if (!attributeValues.Values.Any())
        {
            return attributeValues;
        }

        var newAttributeValues = new RoadSegmentDynamicAttributeValues<T>();
        newAttributeValues.Add(RoadSegmentPosition.Zero, RoadSegmentPosition.FromDouble(geometry.Value.Length), RoadSegmentAttributeSide.Both, attributeValues.Values.Single().Value);
        return newAttributeValues;
    }

    public static AddRoadSegmentChange WithDynamicAttributePositionsOnEntireGeometryLength(this AddRoadSegmentChange change)
    {
        return change with
        {
            AccessRestriction = change.AccessRestriction.OnEntireGeometry(change.Geometry),
            Category = change.Category.OnEntireGeometry(change.Geometry),
            Morphology = change.Morphology.OnEntireGeometry(change.Geometry),
            Status = change.Status.OnEntireGeometry(change.Geometry),
            StreetNameId = change.StreetNameId.OnEntireGeometry(change.Geometry),
            MaintenanceAuthorityId = change.MaintenanceAuthorityId.OnEntireGeometry(change.Geometry),
            SurfaceType = change.SurfaceType.OnEntireGeometry(change.Geometry),
            CarAccess = change.CarAccess.OnEntireGeometry(change.Geometry),
            BikeAccess = change.BikeAccess.OnEntireGeometry(change.Geometry),
            PedestrianAccess = change.PedestrianAccess.OnEntireGeometry(change.Geometry)
        };
    }
}
