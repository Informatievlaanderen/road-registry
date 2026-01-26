namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork;
using ValueObjects;

public partial class RoadSegment
{
    public static (RoadSegment?, Problems) Add(AddRoadSegmentChange change, Provenance provenance, IRoadNetworkIdGenerator idGenerator)
    {
        var originalId = change.OriginalId ?? change.TemporaryId;
        var problems = Problems.For(originalId);

        problems += new RoadSegmentGeometryValidator().Validate(originalId, change.GeometryDrawMethod, change.Geometry.Value);

        var segmentLength = change.Geometry.Value.Length;
        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction.TryCleanCoverages(segmentLength),
            Category = change.Category.TryCleanCoverages(segmentLength),
            Morphology = change.Morphology.TryCleanCoverages(segmentLength),
            Status = change.Status.TryCleanCoverages(segmentLength),
            StreetNameId = change.StreetNameId.TryCleanCoverages(segmentLength),
            MaintenanceAuthorityId = change.MaintenanceAuthorityId.TryCleanCoverages(segmentLength),
            SurfaceType = change.SurfaceType.TryCleanCoverages(segmentLength),
            CarAccess = change.CarAccess.TryCleanCoverages(segmentLength),
            BikeAccess = change.BikeAccess.TryCleanCoverages(segmentLength),
            PedestrianAccess = change.PedestrianAccess.TryCleanCoverages(segmentLength),
            EuropeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList()
        };
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentWasAdded
        {
            RoadSegmentId = idGenerator.NewRoadSegmentId(),
            OriginalId = change.OriginalId,
            Geometry = change.Geometry,
            StartNodeId = change.StartNodeId,
            EndNodeId = change.EndNodeId,
            GeometryDrawMethod = attributes.GeometryDrawMethod,
            AccessRestriction = attributes.AccessRestriction,
            Category = attributes.Category,
            Morphology = attributes.Morphology,
            Status = attributes.Status,
            StreetNameId = attributes.StreetNameId,
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId,
            SurfaceType = attributes.SurfaceType,
            CarAccess = attributes.CarAccess,
            BikeAccess = attributes.BikeAccess,
            PedestrianAccess = attributes.PedestrianAccess,
            EuropeanRoadNumbers = attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = attributes.NationalRoadNumbers,
            Provenance = new ProvenanceData(provenance)
        });

        return (segment, problems);
    }
}
