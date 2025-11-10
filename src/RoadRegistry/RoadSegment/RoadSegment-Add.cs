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

        problems += line.GetProblemsForRoadSegmentGeometry(originalIdOrId);

        //TODO-pr validate all dynamic attributes
        /*AccessRestriction
Category
Morphology
Status
StreetNameId
MaintenanceAuthorityId
SurfaceType*/
        // problems += line.GetProblemsForRoadSegmentLanes(change.Lanes, context.Tolerances);

        if (change.EuropeanRoadNumbers.Count != change.EuropeanRoadNumbers.Distinct().Count())
        {
            problems = problems.Add(new Error(ProblemCode.RoadSegment.EuropeanRoads.NotUnique));
        }

        if (change.NationalRoadNumbers.Count != change.NationalRoadNumbers.Distinct().Count())
        {
            problems = problems.Add(new Error(ProblemCode.RoadSegment.NationalRoads.NotUnique));
        }

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
