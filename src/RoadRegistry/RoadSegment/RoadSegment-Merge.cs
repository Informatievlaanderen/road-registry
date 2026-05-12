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
            Status = change.Status,
            AccessRestriction = change.AccessRestriction,
            Category = change.Category,
            Morphology = change.Morphology,
            StreetNameId = change.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType,
            CarAccessForward = change.CarAccessForward,
            CarAccessBackward = change.CarAccessBackward,
            BikeAccessForward = change.BikeAccessForward,
            BikeAccessBackward = change.BikeAccessBackward,
            PedestrianAccess = change.PedestrianAccess,
            EuropeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList()
        };
        problems += new RoadSegmentAttributesValidator().Validate(attributes, segmentLength);

        RoadNodeId? startNodeId = null, endNodeId = null;
        if (attributes.Status == RoadSegmentStatusV2.Gerealiseerd)
        {
            var startEndNodes = context.RoadNetwork.FindStartEndNodes(change.Geometry);
            startNodeId = startEndNodes.StartNodeId;
            endNodeId = startEndNodes.EndNodeId;
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
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            GeometryDrawMethod = attributes.GeometryDrawMethod,
            Status = attributes.Status,
            AccessRestriction = attributes.AccessRestriction,
            Category = attributes.Category,
            Morphology = attributes.Morphology,
            StreetNameId = attributes.StreetNameId,
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId,
            SurfaceType = attributes.SurfaceType,
            CarAccessForward = attributes.CarAccessForward,
            CarAccessBackward = attributes.CarAccessBackward,
            BikeAccessForward = attributes.BikeAccessForward,
            BikeAccessBackward = attributes.BikeAccessBackward,
            PedestrianAccess = attributes.PedestrianAccess,
            EuropeanRoadNumbers = attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = attributes.NationalRoadNumbers,
            Provenance = new ProvenanceData(context.Provenance)
        };

        Apply(@event);

        return problems;
    }
}
