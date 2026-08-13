namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    public Problems Merge(MergeRoadSegmentChange change, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        problems += change.Geometry.ValidateRoadSegmentGeometryDomainV2();

        var segmentLength = change.Geometry.Value.Length;
        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction,
            Category = change.Category,
            Morphology = change.Morphology,
            StreetNameId = change.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType,
            CarTrafficDirection = change.CarTrafficDirection,
            BikeTrafficDirection = change.BikeTrafficDirection,
            PedestrianTrafficDirection = change.PedestrianTrafficDirection,
            EuropeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList()
        };
        problems += new RoadSegmentAttributesValidator().Validate(attributes, segmentLength);

        RoadSegmentNodeIds? nodeIds = null;

        if (change.Status == RoadSegmentStatusV2.Gerealiseerd)
        {
            var startEndNodes = context.RoadNetwork.FindStartEndNodes(change.Geometry);
            nodeIds = startEndNodes.NodeIds;
            problems += startEndNodes.Problems;
        }

        if (problems.HasError())
        {
            return problems;
        }

        var @event = new RoadSegmentWasMerged
        {
            RoadSegmentId = RoadSegmentId,
            OtherRoadSegmentId = change.OtherRoadSegmentId,
            Geometry = change.Geometry,
            Status = change.Status,
            StartNodeId = nodeIds?.Start,
            EndNodeId = nodeIds?.End,
            GeometryDrawMethod = attributes.GeometryDrawMethod,
            AccessRestriction = attributes.AccessRestriction,
            Category = attributes.Category,
            Morphology = attributes.Morphology,
            StreetNameId = attributes.StreetNameId,
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId,
            SurfaceType = attributes.SurfaceType,
            CarTrafficDirection = attributes.CarTrafficDirection,
            BikeTrafficDirection = attributes.BikeTrafficDirection,
            PedestrianTrafficDirection = attributes.PedestrianTrafficDirection,
            EuropeanRoadNumbers = attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = attributes.NationalRoadNumbers,
            Provenance = new ProvenanceData(context.Provenance)
        };

        Apply(@event);

        return problems;
    }
}
