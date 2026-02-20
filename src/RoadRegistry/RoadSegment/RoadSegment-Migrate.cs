namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    public static (RoadSegment?, Problems) Migrate(MigrateRoadSegmentChange change, ScopedRoadNetworkContext context)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;
        var problems = Problems.For(originalId);

        var geometryDrawMethod = change.GeometryDrawMethod;
        var geometry = change.Geometry.Value;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

        var segmentLength = geometry.Length;
        var status = change.Status;
        var accessRestriction = change.AccessRestriction;
        var category = change.Category;
        var morphology = change.Morphology;
        var streetNameId = change.StreetNameId;
        var maintenanceAuthorityId = change.MaintenanceAuthorityId;
        var surfaceType = change.SurfaceType;
        var carAccessForward = change.CarAccessForward;
        var carAccessBackward = change.CarAccessBackward;
        var bikeAccessForward = change.BikeAccessForward;
        var bikeAccessBackward = change.BikeAccessBackward;
        var pedestrianAccess = change.PedestrianAccess;
        var europeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList();
        var nationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList();
        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = geometryDrawMethod,
            Status = status,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarAccessForward = carAccessForward,
            CarAccessBackward = carAccessBackward,
            BikeAccessForward = bikeAccessForward,
            BikeAccessBackward = bikeAccessBackward,
            PedestrianAccess = pedestrianAccess,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers
        };
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        var startEndNodes = context.RoadNetwork.FindStartEndNodes(originalId, change.GeometryDrawMethod, change.Geometry, context.Tolerances);
        problems += startEndNodes.Problems;

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentWasMigrated
        {
            RoadSegmentId = change.RoadSegmentId,
            OriginalId = change.OriginalId,
            StartNodeId = startEndNodes.StartNodeId!.Value,
            EndNodeId = startEndNodes.EndNodeId!.Value,
            Geometry = geometry.ToRoadSegmentGeometry(),
            GeometryDrawMethod = geometryDrawMethod,
            Status = status,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarAccessForward = carAccessForward,
            CarAccessBackward = carAccessBackward,
            BikeAccessForward = bikeAccessForward,
            BikeAccessBackward = bikeAccessBackward,
            PedestrianAccess = pedestrianAccess,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers,
            Provenance = new ProvenanceData(context.Provenance)
        });

        return (segment, problems);
    }
}
