namespace RoadRegistry.RoadSegment;

using System.Linq;
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

        problems += change.AccessRestriction.Validate(originalIdOrId, nameof(change.AccessRestriction), line.Length);
        problems += change.Category.Validate(originalIdOrId, nameof(change.Category), line.Length);
        problems += change.Morphology.Validate(originalIdOrId, nameof(change.Morphology), line.Length);
        problems += change.Status.Validate(originalIdOrId, nameof(change.Status), line.Length);
        problems += change.StreetNameId.Validate(originalIdOrId, nameof(change.StreetNameId), line.Length);
        problems += change.MaintenanceAuthorityId.Validate(originalIdOrId, nameof(change.MaintenanceAuthorityId), line.Length);
        problems += change.SurfaceType.Validate(originalIdOrId, nameof(change.SurfaceType), line.Length);
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
