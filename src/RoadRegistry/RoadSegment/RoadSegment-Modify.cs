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
    public Problems Modify(ModifyRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);
        var geometryDrawMethod = change.GeometryDrawMethod ?? AttributeHash.GeometryDrawMethod;

        var line = change.Geometry?.GetSingleLineString();
        if (line is not null)
        {
            problems += geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                ? line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances)
                : line.GetProblemsForRoadSegmentGeometry(originalIdOrId, context.Tolerances);
        }

        if (geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            return problems;
        }

        var category = change.Category;
        if (category is not null
            && change.CategoryModified is not null
            && !change.CategoryModified.Value
            && category != AttributeHash.Category
            && RoadSegmentCategory.IsUpgraded(AttributeHash.Category))
        {
            category = AttributeHash.Category;
            problems += new RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion(originalIdOrId);
        }

        if (problems.HasError())
        {
            return problems;
        }

        var laneIdentifiers = new ReusableAttributeIdGenerator(context.IdGenerator, g => g.NewRoadSegmentLaneAttributeId(), Lanes);
        var surfaceIdentifiers = new ReusableAttributeIdGenerator(context.IdGenerator, g => g.NewRoadSegmentSurfaceAttributeId(), Surfaces);
        var widthIdentifiers = new ReusableAttributeIdGenerator(context.IdGenerator, g => g.NewRoadSegmentWidthAttributeId(), Widths);

        ApplyChange(new RoadSegmentModified
        {
            Id = Id,
            OriginalId = change.OriginalId,
            //Version = afterSegment.Version,
            StartNodeId = change.StartNodeId ?? StartNodeId,
            EndNodeId = change.EndNodeId ?? EndNodeId,
            Geometry = (change.Geometry ?? Geometry).ToRoadSegmentGeometry(),
            //GeometryVersion = afterSegment.GeometryVersion,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId ?? AttributeHash.MaintenanceAuthorityId,
            GeometryDrawMethod = change.GeometryDrawMethod ?? AttributeHash.GeometryDrawMethod,
            Morphology = change.Morphology ?? AttributeHash.Morphology,
            Status = change.Status ?? AttributeHash.Status,
            Category = category ?? AttributeHash.Category,
            AccessRestriction = change.AccessRestriction ?? AttributeHash.AccessRestriction,
            LeftSide = new()
            {
                StreetNameId = change.LeftSideStreetNameId ?? AttributeHash.LeftStreetNameId
            },
            RightSide = new()
            {
                StreetNameId = change.RightSideStreetNameId ?? AttributeHash.RightStreetNameId
            },
            Lanes = change.Lanes is not null
                ? change.Lanes
                    .Select(x => new RoadSegmentLaneAttribute(laneIdentifiers.GetNextId(), x.From, x.To, x.Count, x.Direction))
                    .ToArray()
                : Lanes.ToArray(),
            Surfaces = change.Surfaces is not null
                ? change.Surfaces
                    .Select(x => new RoadSegmentSurfaceAttribute(surfaceIdentifiers.GetNextId(), x.From, x.To, x.Type))
                    .ToArray()
                : Surfaces.ToArray(),
            Widths = change.Widths is not null
                ? change.Widths
                     .Select(x => new RoadSegmentWidthAttribute(widthIdentifiers.GetNextId(), x.From, x.To, x.Width))
                     .ToArray()
                : Widths.ToArray()
            //ConvertedFromOutlined = ConvertedFromOutlined
        });

        return problems;
    }
}
