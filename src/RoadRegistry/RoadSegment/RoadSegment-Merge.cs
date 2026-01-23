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
    public static (RoadSegment?, Problems) Merge(MergeRoadSegmentChange change, Provenance provenance, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
    {
        var problems = Problems.For(change.TemporaryId);

        problems += new RoadSegmentGeometryValidator().Validate(change.TemporaryId, change.GeometryDrawMethod, change.Geometry.Value);

        var segmentLength = change.Geometry.Value.Length;
        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction.TryCleanEntireLengthCoverages(segmentLength),
            Category = change.Category.TryCleanEntireLengthCoverages(segmentLength),
            Morphology = change.Morphology.TryCleanEntireLengthCoverages(segmentLength),
            Status = change.Status.TryCleanEntireLengthCoverages(segmentLength),
            StreetNameId = change.StreetNameId.TryCleanEntireLengthCoverages(segmentLength),
            MaintenanceAuthorityId = change.MaintenanceAuthorityId.TryCleanEntireLengthCoverages(segmentLength),
            SurfaceType = change.SurfaceType.TryCleanEntireLengthCoverages(segmentLength),
            CarAccess = change.CarAccess.TryCleanEntireLengthCoverages(segmentLength),
            BikeAccess = change.BikeAccess.TryCleanEntireLengthCoverages(segmentLength),
            PedestrianAccess = change.PedestrianAccess.TryCleanEntireLengthCoverages(segmentLength),
            EuropeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList()
        };
        problems += new RoadSegmentAttributesValidator().Validate(change.TemporaryId, attributes, segmentLength);

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentWasMerged
        {
            RoadSegmentId = idGenerator.NewRoadSegmentId(),
            OriginalIds = change.OriginalIds,
            Geometry = change.Geometry,
            StartNodeId = idTranslator.TranslateToPermanentId(change.StartNodeId),
            EndNodeId = idTranslator.TranslateToPermanentId(change.EndNodeId),
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
