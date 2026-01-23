namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public partial class RoadSegment
{
    public static (RoadSegment?, Problems) Migrate(MigrateRoadSegmentChange change, Provenance provenance)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;
        var problems = Problems.For(originalId);

        var geometryDrawMethod = change.GeometryDrawMethod;
        var geometry = change.Geometry.Value;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

        var segmentLength = geometry.Length;
        var accessRestriction = change.AccessRestriction.TryCleanEntireLengthCoverages(segmentLength);
        var category = change.Category.TryCleanEntireLengthCoverages(segmentLength);
        var morphology = change.Morphology.TryCleanEntireLengthCoverages(segmentLength);
        var status = change.Status.TryCleanEntireLengthCoverages(segmentLength);
        var streetNameId = change.StreetNameId.TryCleanEntireLengthCoverages(segmentLength);
        var maintenanceAuthorityId = change.MaintenanceAuthorityId.TryCleanEntireLengthCoverages(segmentLength);
        var surfaceType = change.SurfaceType.TryCleanEntireLengthCoverages(segmentLength);
        var carAccess = change.CarAccess.TryCleanEntireLengthCoverages(segmentLength);
        var bikeAccess = change.BikeAccess.TryCleanEntireLengthCoverages(segmentLength);
        var pedestrianAccess = change.PedestrianAccess.TryCleanEntireLengthCoverages(segmentLength);
        var europeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList();
        var nationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList();
        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = geometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            Status = status,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarAccess = carAccess,
            BikeAccess = bikeAccess,
            PedestrianAccess = pedestrianAccess,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers
        };
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentWasMigrated
        {
            RoadSegmentId = change.RoadSegmentId,
            OriginalId = change.OriginalId,
            StartNodeId = change.StartNodeId,
            EndNodeId = change.EndNodeId,
            Geometry = geometry.ToRoadSegmentGeometry(),
            GeometryDrawMethod = geometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            Status = status,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarAccess = carAccess,
            BikeAccess = bikeAccess,
            PedestrianAccess = pedestrianAccess,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers,
            Provenance = new ProvenanceData(provenance)
        });

        return (segment, problems);
    }
}
