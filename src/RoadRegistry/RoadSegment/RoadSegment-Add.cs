namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    public static (RoadSegment?, Problems) Add(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(change.RoadSegmentIdReference);

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

        RoadNodeId? startNodeId = null, endNodeId = null;
        if (change.Status == RoadSegmentStatusV2.Gerealiseerd)
        {
            var startEndNodes = context.RoadNetwork.FindStartEndNodes(change.Geometry);
            startNodeId = startEndNodes.StartNodeId;
            endNodeId = startEndNodes.EndNodeId;
            problems += startEndNodes.Problems;
        }

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentWasAdded
        {
            RoadSegmentId = idGenerator.NewRoadSegmentId(),
            OriginalRoadSegmentIdReference = change.RoadSegmentIdReference,
            Geometry = change.Geometry,
            Status = change.Status,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
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
        });

        return (segment, problems);
    }
}
