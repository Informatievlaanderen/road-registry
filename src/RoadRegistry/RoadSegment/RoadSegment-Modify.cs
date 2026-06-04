namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change, ScopedRoadNetworkChangeContext context)
    {
        var roadSegmentIdReference = change.RoadSegmentIdReference;
        var problems = Problems.WithContext(roadSegmentIdReference);

        var geometry = (change.Geometry ?? Geometry).Value;

        problems += change.Geometry.ValidateRoadSegmentGeometryDomainV2();

        var segmentLength = geometry.Length;
        var accessRestriction = change.AccessRestriction;
        var category = change.Category;
        var morphology = change.Morphology;
        var streetNameId = change.StreetNameId;
        var maintenanceAuthorityId = change.MaintenanceAuthorityId;
        var surfaceType = change.SurfaceType;
        var carAccessForward = change.CarAccessForward;
        var carAccessBackward = change.CarAccessBackward;
        var bikeAccessForward = change.BikeAccessForward;
        var bikeAccessBackward = change.BikeAccessBackward;
        var pedestrianAccess = change.PedestrianAccess;
        var attributes = Attributes! with
        {
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = accessRestriction ?? Attributes.AccessRestriction,
            Category = category ?? Attributes.Category,
            Morphology = morphology ?? Attributes.Morphology,
            StreetNameId = streetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = surfaceType ?? Attributes.SurfaceType,
            CarAccessForward = carAccessForward ?? Attributes.CarAccessForward,
            CarAccessBackward = carAccessBackward ?? Attributes.CarAccessBackward,
            BikeAccessForward = bikeAccessForward ?? Attributes.BikeAccessForward,
            BikeAccessBackward = bikeAccessBackward ?? Attributes.BikeAccessBackward,
            PedestrianAccess = pedestrianAccess ?? Attributes.PedestrianAccess
        };
        problems += new RoadSegmentAttributesValidator().Validate(attributes, segmentLength);

        RoadNodeId? startNodeId = null, endNodeId = null;

        var status = change.Status ?? Status;
        if (change.Geometry is not null && status == RoadSegmentStatusV2.Gerealiseerd)
        {
            var startEndNodes = context.RoadNetwork.FindStartEndNodes(change.Geometry);
            problems += startEndNodes.Problems;
            startNodeId = startEndNodes.StartNodeId;
            endNodeId = startEndNodes.EndNodeId;
        }

        if (problems.HasError())
        {
            return problems;
        }

        var hasChanges = (change.Geometry is not null && Geometry != change.Geometry)
                         || (change.Status is not null && Status != change.Status)
                         || (startNodeId is not null && StartNodeId != startNodeId)
                         || (endNodeId is not null && EndNodeId != endNodeId)
                         || !Attributes.Equals(attributes);
        if (!hasChanges)
        {
            return problems;
        }

        Apply(new RoadSegmentWasModified
        {
            RoadSegmentId = RoadSegmentId,
            OriginalRoadSegmentIdReference = roadSegmentIdReference,
            Geometry = change.Geometry,
            Status = change.Status,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarAccessForward = carAccessForward,
            CarAccessBackward = carAccessBackward,
            BikeAccessForward = bikeAccessForward,
            BikeAccessBackward = bikeAccessBackward,
            PedestrianAccess = pedestrianAccess,
            Provenance = new ProvenanceData(context.Provenance)
        });

        return problems;
    }
}
