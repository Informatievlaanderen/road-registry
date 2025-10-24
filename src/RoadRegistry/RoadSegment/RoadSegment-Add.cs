namespace RoadRegistry.RoadSegment;

using System.Linq;
using BackOffice;
using BackOffice.Core;
using Events;
using NetTopologySuite.Geometries;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    public static (RoadSegment, Problems) Register(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var segment = new RoadSegment();
        var problems = segment.Add(change, context);
        return (segment, problems);
    }

    private Problems Add(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(change.TemporaryId);

        var line = change.Geometry.GetSingleLineString();

        if (change.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);

            return problems;
        }

        problems += line.GetProblemsForRoadSegmentGeometry(originalIdOrId, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentLanes(change.Lanes, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentWidths(change.Widths, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentSurfaces(change.Surfaces, context.Tolerances);

        if (problems.HasError())
        {
            return problems;
        }

        ApplyChange(new RoadSegmentAdded
        {
            Id = change.PermanentId ?? context.IdGenerator.NewRoadSegmentId(),
            OriginalId = change.OriginalId,
            Geometry = change.Geometry.ToRoadSegmentGeometry(),
            StartNodeId = change.StartNodeId,
            EndNodeId = change.EndNodeId,
            AccessRestriction = change.AccessRestriction,
            Category = change.Category,
            GeometryDrawMethod = change.GeometryDrawMethod,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId,
            Morphology = change.Morphology,
            Status = change.Status,
            LeftSide = new RoadSegmentSideAttribute
            {
                StreetNameId = change.LeftSideStreetNameId
            },
            RightSide = new RoadSegmentSideAttribute
            {
                StreetNameId = change.RightSideStreetNameId
            },
            Lanes = change.Lanes
                .Select(x => new RoadSegmentLaneAttribute(context.IdGenerator.NewRoadSegmentLaneAttributeId(), x.From, x.To, x.Count, x.Direction))
                .ToArray(),
            Surfaces = change.Surfaces
                .Select(x => new RoadSegmentSurfaceAttribute(context.IdGenerator.NewRoadSegmentSurfaceAttributeId(), x.From, x.To, x.Type))
                .ToArray(),
            Widths = change.Widths
                .Select(x => new RoadSegmentWidthAttribute(context.IdGenerator.NewRoadSegmentWidthAttributeId(), x.From, x.To, x.Width))
                .ToArray()
        });

        return problems;
    }
}
