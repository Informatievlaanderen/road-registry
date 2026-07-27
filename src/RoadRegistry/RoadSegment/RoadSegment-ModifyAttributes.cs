namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadSegment
{
    // Dedicated attribute-only modification: validates the new dynamically-segmented attribute values against the
    // segment length and emits RoadSegmentAttributesWasModified. Geometry, status and nodes are never touched here.
    public Problems ModifyAttributes(ModifyRoadSegmentAttributesChange change, ScopedRoadNetworkChangeContext context)
    {
        var roadSegmentIdReference = change.RoadSegmentIdReference;
        var problems = Problems.WithContext(roadSegmentIdReference);

        var segmentLength = Geometry.Value.Length;

        var accessRestriction = change.AccessRestriction;
        var category = change.Category;
        var morphology = change.Morphology;
        var streetNameId = change.StreetNameId;
        var maintenanceAuthorityId = change.MaintenanceAuthorityId;
        var surfaceType = change.SurfaceType;
        var carTrafficDirection = change.CarTrafficDirection;
        var bikeTrafficDirection = change.BikeTrafficDirection;
        var pedestrianTrafficDirection = change.PedestrianTrafficDirection;

        var attributes = Attributes! with
        {
            AccessRestriction = accessRestriction ?? Attributes.AccessRestriction,
            Category = category ?? Attributes.Category,
            Morphology = morphology ?? Attributes.Morphology,
            StreetNameId = streetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = surfaceType ?? Attributes.SurfaceType,
            CarTrafficDirection = carTrafficDirection ?? Attributes.CarTrafficDirection,
            BikeTrafficDirection = bikeTrafficDirection ?? Attributes.BikeTrafficDirection,
            PedestrianTrafficDirection = pedestrianTrafficDirection ?? Attributes.PedestrianTrafficDirection
        };
        problems += new RoadSegmentAttributesValidator().Validate(attributes, segmentLength);

        if (problems.HasError())
        {
            return problems;
        }

        if (Attributes.Equals(attributes))
        {
            return problems;
        }

        Apply(new RoadSegmentAttributesWasModified
        {
            RoadSegmentId = RoadSegmentId,
            OriginalRoadSegmentIdReference = roadSegmentIdReference,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarTrafficDirection = carTrafficDirection,
            BikeTrafficDirection = bikeTrafficDirection,
            PedestrianTrafficDirection = pedestrianTrafficDirection,
            Provenance = new ProvenanceData(context.Provenance)
        });

        return problems;
    }
}
