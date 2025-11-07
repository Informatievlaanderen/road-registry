namespace RoadRegistry.RoadSegment;

using BackOffice;
using BackOffice.Core;
using Changes;
using Events;
using NetTopologySuite.Geometries;
using RoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var originalIdOrId = context.IdTranslator.TranslateToTemporaryId(RoadSegmentId);
        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;

        var line = change.Geometry?.GetSingleLineString();
        if (line is not null)
        {
            problems += geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                ? line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances)
                : line.GetProblemsForRoadSegmentGeometry(originalIdOrId, context.Tolerances);
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(change.StartNodeId ?? StartNodeId, out var startRoadNode))
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        }
        if (!context.RoadNetwork.RoadNodes.TryGetValue(change.EndNodeId ?? EndNodeId, out var endRoadNode))
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        }

        if (geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            return problems;
        }

        //TODO-pr category logica upgrade validatie herbekijken eens FC is aangepast om de dynamische structuur te ondersteunen
        // var category = change.Category;
        // if (category is not null
        //     && change.CategoryModified is not null
        //     && !change.CategoryModified.Value
        //     && category != AttributeHash.Category
        //     && RoadSegmentCategory.IsUpgraded(AttributeHash.Category))
        // {
        //     category = AttributeHash.Category;
        //     problems += new RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion(originalIdOrId);
        // }

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

        if (problems.HasError())
        {
            return problems;
        }

        Apply(new RoadSegmentModified
        {
            RoadSegmentId = RoadSegmentId,
            OriginalId = change.OriginalId,
            //Version = afterSegment.Version,
            StartNodeId = change.StartNodeId ?? StartNodeId,
            EndNodeId = change.EndNodeId ?? EndNodeId,
            Geometry = (change.Geometry ?? Geometry).ToGeometryObject(),
            //GeometryVersion = afterSegment.GeometryVersion,
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction ?? Attributes.AccessRestriction,
            Category = change.Category ?? Attributes.Category,
            Morphology = change.Morphology ?? Attributes.Morphology,
            Status = change.Status ?? Attributes.Status,
            StreetNameId = change.StreetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType ?? Attributes.SurfaceType,
            EuropeanRoadNumbers = change.EuropeanRoadNumbers ?? Attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = change.NationalRoadNumbers ?? Attributes.NationalRoadNumbers
            //ConvertedFromOutlined = ConvertedFromOutlined
        });

        return problems;
    }
}
