namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    public Problems Migrate(MigrateRoadSegmentChange change, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(change.RoadSegmentIdReference);

        var geometryDrawMethod = change.GeometryDrawMethod;
        var geometry = change.Geometry.Value;

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
        var europeanRoadNumbers = change.EuropeanRoadNumbers.ToImmutableList();
        var nationalRoadNumbers = change.NationalRoadNumbers.ToImmutableList();
        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = geometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarTrafficDirection = carTrafficDirection,
            BikeTrafficDirection = bikeTrafficDirection,
            PedestrianTrafficDirection = pedestrianTrafficDirection,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers
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
            return problems;
        }

        Apply(new RoadSegmentWasMigrated
        {
            RoadSegmentId = RoadSegmentId,
            OriginalRoadSegmentIdReference = change.RoadSegmentIdReference,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            Geometry = geometry.ToRoadSegmentGeometry(),
            Status = change.Status,
            GeometryDrawMethod = geometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarTrafficDirection = carTrafficDirection,
            BikeTrafficDirection = bikeTrafficDirection,
            PedestrianTrafficDirection = pedestrianTrafficDirection,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers,
            Provenance = new ProvenanceData(context.Provenance)
        });

        return problems;
    }
}
