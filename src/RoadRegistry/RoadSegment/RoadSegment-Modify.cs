namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change, Provenance provenance)
    {
        var problems = Problems.None;

        var originalId = change.OriginalId ?? change.RoadSegmentId;
        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;
        var geometry = change.Geometry ?? Geometry;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

        var segmentLength = geometry.Length;
        var accessRestriction = change.AccessRestriction?.TryCleanEntireLengthCoverages(segmentLength);
        var category = change.Category?.TryCleanEntireLengthCoverages(segmentLength);
        var morphology = change.Morphology?.TryCleanEntireLengthCoverages(segmentLength);
        var status = change.Status?.TryCleanEntireLengthCoverages(segmentLength);
        var streetNameId = change.StreetNameId?.TryCleanEntireLengthCoverages(segmentLength);
        var maintenanceAuthorityId = change.MaintenanceAuthorityId?.TryCleanEntireLengthCoverages(segmentLength);
        var surfaceType = change.SurfaceType?.TryCleanEntireLengthCoverages(segmentLength);
        var attributes = Attributes with
        {
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = accessRestriction ?? Attributes.AccessRestriction,
            Category = category ?? Attributes.Category,
            Morphology = morphology ?? Attributes.Morphology,
            Status = status ?? Attributes.Status,
            StreetNameId = streetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = surfaceType ?? Attributes.SurfaceType
        };
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        if (problems.HasError())
        {
            return problems;
        }

        Apply(new RoadSegmentModified
        {
            RoadSegmentId = RoadSegmentId,
            OriginalId = change.OriginalId,
            StartNodeId = change.StartNodeId,
            EndNodeId = change.EndNodeId,
            Geometry = change.Geometry?.ToGeometryObject(),
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            Status = status,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
