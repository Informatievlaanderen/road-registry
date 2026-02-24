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
    public static (RoadSegment?, Problems) Add(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context)
    {
        var originalId = change.OriginalId ?? change.TemporaryId;
        var problems = Problems.For(originalId);

        problems += new RoadSegmentGeometryValidator().Validate(originalId, change.GeometryDrawMethod, change.Geometry.Value);

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
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        var startEndNodes = context.RoadNetwork.FindStartEndNodes(originalId, change.GeometryDrawMethod, change.Geometry, context.Tolerances);
        problems += startEndNodes.Problems;

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentWasAdded
        {
            RoadSegmentId = idGenerator.NewRoadSegmentId(),
            OriginalId = change.OriginalId,
            Geometry = change.Geometry,
            StartNodeId = startEndNodes.StartNodeId!.Value,
            EndNodeId = startEndNodes.EndNodeId!.Value,
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
        });

        return (segment, problems);
    }
}
