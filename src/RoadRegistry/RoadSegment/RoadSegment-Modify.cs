namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

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
        var carTrafficDirection = change.CarTrafficDirection;
        var bikeTrafficDirection = change.BikeTrafficDirection;
        var pedestrianTrafficDirection = change.PedestrianTrafficDirection;
        var attributes = Attributes! with
        {
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
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

        RoadSegmentNodeIds? nodeIds = null;

        var status = change.Status ?? Status;
        if (change.Geometry is not null && status == RoadSegmentStatusV2.Gerealiseerd)
        {
            var startEndNodes = context.RoadNetwork.FindStartEndNodes(change.Geometry);
            problems += startEndNodes.Problems;
            nodeIds = startEndNodes.NodeIds;
        }
        else if (Status == RoadSegmentStatusV2.Gerealiseerd && status != RoadSegmentStatusV2.Gerealiseerd)
        {
            // Only a realized segment is knotted into the network; leaving that status detaches it from its nodes.
            nodeIds = new RoadSegmentNodeIds();
        }

        if (problems.HasError())
        {
            return problems;
        }

        if (nodeIds is not null && nodeIds.Start == StartNodeId && nodeIds.End == EndNodeId)
        {
            nodeIds = null;
        }

        var hasChanges = (change.Geometry is not null && Geometry != change.Geometry)
                         || (change.Status is not null && Status != change.Status)
                         || nodeIds is not null
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
            NodeIds = nodeIds,
            GeometryDrawMethod = change.GeometryDrawMethod,
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
