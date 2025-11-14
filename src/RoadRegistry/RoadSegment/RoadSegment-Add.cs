namespace RoadRegistry.RoadSegment;

using BackOffice;
using BackOffice.Core;
using BackOffice.Core.ProblemCodes;
using Changes;
using Events;
using NetTopologySuite.Geometries;

public partial class RoadSegment
{
    public static (RoadSegment?, Problems) Add(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;

        var originalIdOrId = change.OriginalId ?? change.TemporaryId;

        var line = change.Geometry.GetSingleLineString();

        if (change.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId);

            return (null, problems);
        }

        problems += line.ValidateRoadSegmentGeometry(originalIdOrId);

        problems += change.AccessRestriction.Validate(originalIdOrId, line.Length, ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes);
        problems += change.Category.Validate(originalIdOrId, line.Length, ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes);
        problems += change.Morphology.Validate(originalIdOrId, line.Length, ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes);
        problems += change.Status.Validate(originalIdOrId, line.Length, ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes);
        problems += change.StreetNameId.Validate(originalIdOrId, line.Length, ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes);
        problems += change.MaintenanceAuthorityId.Validate(originalIdOrId, line.Length, ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes);
        problems += change.SurfaceType.Validate(originalIdOrId, line.Length, ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes);
        problems += change.EuropeanRoadNumbers.ValidateCollectionMustBeUnique(originalIdOrId, ProblemCode.RoadSegment.EuropeanRoads.NotUnique);
        problems += change.NationalRoadNumbers.ValidateCollectionMustBeUnique(originalIdOrId, ProblemCode.RoadSegment.NationalRoads.NotUnique);

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentAdded
        {
            RoadSegmentId = change.PermanentId ?? idGenerator.NewRoadSegmentId(),
            OriginalId = change.OriginalId ?? change.TemporaryId,
            Geometry = change.Geometry.ToGeometryObject(),
            StartNodeId = change.StartNodeId,
            EndNodeId = change.EndNodeId,
            GeometryDrawMethod = change.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction,
            Category = change.Category,
            Morphology = change.Morphology,
            Status = change.Status,
            StreetNameId = change.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType,
            EuropeanRoadNumbers = change.EuropeanRoadNumbers,
            NationalRoadNumbers = change.NationalRoadNumbers,
        });

        return (segment, problems);
    }
}
