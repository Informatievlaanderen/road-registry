namespace RoadRegistry.RoadSegment;

using BackOffice;
using BackOffice.Core;
using Events;
using NetTopologySuite.Geometries;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public static (RoadSegment?, Problems) Add(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var originalIdOrId = change.OriginalId ?? change.TemporaryId;

        var line = change.Geometry.GetSingleLineString();

        if (change.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);

            return (null, problems);
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(change.StartNodeId, out var startRoadNode))
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        }
        if (!context.RoadNetwork.RoadNodes.TryGetValue(change.EndNodeId, out var endRoadNode))
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        }

        problems += line.GetProblemsForRoadSegmentGeometry(originalIdOrId, context.Tolerances);

        //TODO-pr validate all dynamic attributes
        /*AccessRestriction
Category
Morphology
Status
StreetNameId
MaintenanceAuthorityId
LaneCount
LaneDirection
SurfaceType
Width
EuropeanRoadNumber
NationalRoadNumber
NumberedRoadDirection
NumberedRoadNumber
NumberedRoadOrdinal*/
        // problems += line.GetProblemsForRoadSegmentLanes(change.Lanes, context.Tolerances);
        // problems += line.GetProblemsForRoadSegmentWidths(change.Widths, context.Tolerances);
        // problems += line.GetProblemsForRoadSegmentSurfaces(change.Surfaces, context.Tolerances);

        if (problems.HasError())
        {
            return (null, problems);
        }

        var segment = Create(new RoadSegmentAdded
        {
            Id = change.PermanentId ?? context.IdGenerator.NewRoadSegmentId(),
            OriginalId = change.OriginalId,
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

        startRoadNode!.ConnectWith(segment.RoadSegmentId);
        endRoadNode!.ConnectWith(segment.RoadSegmentId);

        return (segment, problems);
    }
}
