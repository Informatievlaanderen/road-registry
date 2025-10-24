namespace RoadRegistry.RoadSegment;

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

        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);
        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;

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

        ApplyChange(new RoadSegmentModified
        {
            Id = Id,
            OriginalId = change.OriginalId,
            //Version = afterSegment.Version,
            StartNodeId = change.StartNodeId ?? StartNodeId,
            EndNodeId = change.EndNodeId ?? EndNodeId,
            Geometry = (change.Geometry ?? Geometry).ToRoadSegmentGeometry(),
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
