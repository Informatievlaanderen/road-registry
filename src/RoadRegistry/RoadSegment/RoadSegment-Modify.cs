namespace RoadRegistry.RoadSegment;

using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change)
    {
        var problems = Problems.None;

        var originalId = change.OriginalId ?? change.RoadSegmentId;
        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;
        var geometry = change.Geometry ?? Geometry;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction ?? Attributes.AccessRestriction,
            Category = change.Category ?? Attributes.Category,
            Morphology = change.Morphology ?? Attributes.Morphology,
            Status = change.Status ?? Attributes.Status,
            StreetNameId = change.StreetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType ?? Attributes.SurfaceType
        };
        var segmentLength = geometry.Length;
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        if (problems.HasError())
        {
            return problems;
        }

        Apply(new RoadSegmentModified
        {
            RoadSegmentId = RoadSegmentId,
            OriginalId = change.OriginalId,
            StartNodeId = change.StartNodeId ?? StartNodeId,
            EndNodeId = change.EndNodeId ?? EndNodeId,
            Geometry = (change.Geometry ?? Geometry).ToGeometryObject(),
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction ?? Attributes.AccessRestriction,
            Category = change.Category ?? Attributes.Category,
            Morphology = change.Morphology ?? Attributes.Morphology,
            Status = change.Status ?? Attributes.Status,
            StreetNameId = change.StreetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType ?? Attributes.SurfaceType
        });

        return problems;
    }
}
