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
    public Problems Modify(ModifyRoadSegmentChange change)
    {
        var problems = Problems.None;

        var originalIdOrId = change.OriginalId ?? change.RoadSegmentId;
        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;

        var line = change.Geometry?.GetSingleLineString();
        if (line is not null)
        {
            problems += geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                ? line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId)
                : line.ValidateRoadSegmentGeometry(originalIdOrId);
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


        problems += line.ValidateRoadSegmentGeometry(originalIdOrId);

        var segmentLength = (change.Geometry ?? Geometry).Length;
        problems += change.AccessRestriction.Validate(originalIdOrId, segmentLength, ProblemCode.RoadSegment.AccessRestriction.DynamicAttributeProblemCodes);
        problems += change.Category.Validate(originalIdOrId, segmentLength, ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes);
        problems += change.Morphology.Validate(originalIdOrId, segmentLength, ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes);
        problems += change.Status.Validate(originalIdOrId, segmentLength, ProblemCode.RoadSegment.Status.DynamicAttributeProblemCodes);
        problems += change.StreetNameId.Validate(originalIdOrId, segmentLength, ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes);
        problems += change.MaintenanceAuthorityId.Validate(originalIdOrId, segmentLength, ProblemCode.RoadSegment.MaintenanceAuthority.DynamicAttributeProblemCodes);
        problems += change.SurfaceType.Validate(originalIdOrId, segmentLength, ProblemCode.RoadSegment.SurfaceType.DynamicAttributeProblemCodes);
        problems += change.EuropeanRoadNumbers.ValidateCollectionMustBeUnique(originalIdOrId, ProblemCode.RoadSegment.EuropeanRoads.NotUnique);
        problems += change.NationalRoadNumbers.ValidateCollectionMustBeUnique(originalIdOrId, ProblemCode.RoadSegment.NationalRoads.NotUnique);

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
