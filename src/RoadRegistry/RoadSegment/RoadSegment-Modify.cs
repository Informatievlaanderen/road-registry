namespace RoadRegistry.RoadSegment;

using System;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using Events;
using NetTopologySuite.Geometries;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;
        // var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);
        //
        // if (!context.BeforeView.Segments.TryGetValue(Id, out var beforeSegment) && !ConvertedFromOutlined)
        // {
        //     problems = problems.Add(new RoadSegmentNotFound(originalIdOrId));
        // }
        //
        // var line = Geometry?.GetSingleLineString();
        //
        // if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        // {
        //     if (line is not null)
        //     {
        //         problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);
        //     }
        //
        //     return problems;
        // }
        //
        // if (line is not null)
        // {
        //     problems += line.GetProblemsForRoadSegmentGeometry(originalIdOrId, context.Tolerances);
        // }
        //
        // if (beforeSegment is not null && CategoryModified is not null && !CategoryModified.Value && RoadSegmentCategory.IsUpgraded(beforeSegment.AttributeHash.Category))
        // {
        //     _correctedCategory = beforeSegment.AttributeHash.Category;
        //     problems += new RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion(originalIdOrId);
        // }

        if (problems.HasError())
        {
            return problems;
        }

        //TODO-pr current implement
        // generate + apply events
        throw new NotImplementedException();
    }
}
