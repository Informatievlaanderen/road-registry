namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems Migrate(ModifyRoadSegmentChange change, ImmutableList<EuropeanRoadNumber> europeanRoadNumbers, ImmutableList<NationalRoadNumber> nationalRoadNumbers, Provenance provenance)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;
        var problems = Problems.For(originalId);

        var geometryDrawMethod = change.GeometryDrawMethod!;
        var geometry = change.Geometry!;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

        var segmentLength = geometry.Length;
        var accessRestriction = change.AccessRestriction!.TryCleanEntireLengthCoverages(segmentLength);
        var category = change.Category!.TryCleanEntireLengthCoverages(segmentLength);
        var morphology = change.Morphology!.TryCleanEntireLengthCoverages(segmentLength);
        var status = change.Status!.TryCleanEntireLengthCoverages(segmentLength);
        var streetNameId = change.StreetNameId!.TryCleanEntireLengthCoverages(segmentLength);
        var maintenanceAuthorityId = change.MaintenanceAuthorityId!.TryCleanEntireLengthCoverages(segmentLength);
        var surfaceType = change.SurfaceType!.TryCleanEntireLengthCoverages(segmentLength);
        var attributes = Attributes with
        {
            GeometryDrawMethod = geometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            Status = status,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType
        };
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

        if (problems.HasError())
        {
            return problems;
        }

        Apply(new RoadSegmentWasMigrated
        {
            RoadSegmentId = RoadSegmentId,
            OriginalId = change.OriginalId,
            StartNodeId = change.StartNodeId!.Value,
            EndNodeId = change.EndNodeId!.Value,
            Geometry = geometry.ToGeometryObject(),
            GeometryDrawMethod = geometryDrawMethod,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            Status = status,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            Provenance = new ProvenanceData(provenance),
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers
        });

        return problems;
    }
}
