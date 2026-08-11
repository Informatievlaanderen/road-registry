namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    // Records that this segment is now knotted into the network and realized. The geometry is the one it ended up
    // with after snapping, and the attributes are already remapped onto it by the caller.
    public Problems Realize(RoadSegmentGeometry geometry, RoadSegmentAttributes attributes, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        problems += geometry.ValidateRoadSegmentGeometryDomainV2();
        problems += new RoadSegmentAttributesValidator().Validate(attributes, geometry.Value.Length);

        // A realized segment hangs off a road node at either end. They are resolved from whatever sits on the
        // endpoints, so the nodes have to be in place before this is called.
        var startEndNodes = context.RoadNetwork.FindStartEndNodes(geometry);
        problems += startEndNodes.Problems;

        if (problems.HasError())
        {
            return problems;
        }

        Apply(new RoadSegmentWasRealized
        {
            RoadSegmentId = RoadSegmentId,
            Geometry = geometry,
            StartNodeId = startEndNodes.StartNodeId,
            EndNodeId = startEndNodes.EndNodeId,
            AccessRestriction = attributes.AccessRestriction,
            Category = attributes.Category,
            Morphology = attributes.Morphology,
            StreetNameId = attributes.StreetNameId,
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId,
            SurfaceType = attributes.SurfaceType,
            CarTrafficDirection = attributes.CarTrafficDirection,
            BikeTrafficDirection = attributes.BikeTrafficDirection,
            PedestrianTrafficDirection = attributes.PedestrianTrafficDirection,
            Provenance = new ProvenanceData(context.Provenance)
        });

        return problems;
    }
}
