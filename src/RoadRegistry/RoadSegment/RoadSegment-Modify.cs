namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change, ScopedRoadNetworkContext context)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;
        var problems = Problems.For(originalId);

        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;
        var geometry = (change.Geometry ?? Geometry).Value;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

        var segmentLength = geometry.Length;
        var accessRestriction = change.AccessRestriction;
        var category = change.Category;
        var morphology = change.Morphology;
        var status = change.Status;
        var streetNameId = change.StreetNameId;
        var maintenanceAuthorityId = change.MaintenanceAuthorityId;
        var surfaceType = change.SurfaceType;
        var carAccessForward = change.CarAccessForward;
        var carAccessBackward = change.CarAccessBackward;
        var bikeAccessForward = change.BikeAccessForward;
        var bikeAccessBackward = change.BikeAccessBackward;
        var pedestrianAccess = change.PedestrianAccess;
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
            CarAccessForward = carAccessForward ?? Attributes.CarAccessForward,
            CarAccessBackward = carAccessBackward ?? Attributes.CarAccessBackward,
            BikeAccessForward = bikeAccessForward ?? Attributes.BikeAccessForward,
            BikeAccessBackward = bikeAccessBackward ?? Attributes.BikeAccessBackward,
            PedestrianAccess = pedestrianAccess ?? Attributes.PedestrianAccess
        };
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        RoadNodeId? startNodeId = null, endNodeId = null;

        if (change.Geometry is not null)
        {
            var startEndNodes = context.RoadNetwork.FindStartEndNodes(originalId, attributes.GeometryDrawMethod, change.Geometry, context.Tolerances);
            problems += startEndNodes.Problems;
            startNodeId = startEndNodes.StartNodeId;
            endNodeId = startEndNodes.EndNodeId;
        }

        if (problems.HasError())
        {
            return problems;
        }

        Apply(new RoadSegmentWasModified
        {
            RoadSegmentId = RoadSegmentId,
            OriginalId = change.OriginalId,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            Geometry = change.Geometry,
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            Status = status,
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
