namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change, Provenance provenance)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;
        var problems = Problems.For(originalId);

        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;
        var geometry = (change.Geometry ?? Geometry).Value;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

        var segmentLength = geometry.Length;
        var accessRestriction = change.AccessRestriction?.TryCleanCoverages(segmentLength);
        var category = change.Category?.TryCleanCoverages(segmentLength);
        var morphology = change.Morphology?.TryCleanCoverages(segmentLength);
        var status = change.Status?.TryCleanCoverages(segmentLength);
        var streetNameId = change.StreetNameId?.TryCleanCoverages(segmentLength);
        var maintenanceAuthorityId = change.MaintenanceAuthorityId?.TryCleanCoverages(segmentLength);
        var surfaceType = change.SurfaceType?.TryCleanCoverages(segmentLength);
        var carAccess = change.CarAccess?.TryCleanCoverages(segmentLength);
        var bikeAccess = change.BikeAccess?.TryCleanCoverages(segmentLength);
        var pedestrianAccess = change.PedestrianAccess?.TryCleanCoverages(segmentLength);
        var attributes = Attributes with
        {
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = accessRestriction ?? Attributes.AccessRestriction,
            Category = category ?? Attributes.Category,
            Morphology = morphology ?? Attributes.Morphology,
            Status = status ?? Attributes.Status,
            StreetNameId = streetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = surfaceType ?? Attributes.SurfaceType,
            CarAccess = carAccess ?? Attributes.CarAccess,
            BikeAccess = bikeAccess ?? Attributes.BikeAccess,
            PedestrianAccess = pedestrianAccess ?? Attributes.PedestrianAccess
        };
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        if (problems.HasError())
        {
            return problems;
        }

        Apply(new RoadSegmentWasModified
        {
            RoadSegmentId = RoadSegmentId,
            OriginalId = change.OriginalId,
            StartNodeId = change.StartNodeId,
            EndNodeId = change.EndNodeId,
            Geometry = change.Geometry,
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            Status = status,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarAccess = carAccess,
            BikeAccess = bikeAccess,
            PedestrianAccess = pedestrianAccess,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
