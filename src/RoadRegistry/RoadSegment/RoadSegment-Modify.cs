namespace RoadRegistry.RoadSegment;

using System.Collections.Immutable;
using BackOffice.Core;
using Changes;
using Events;
using ValueObjects;

public partial class RoadSegment
{
    public Problems Modify(ModifyRoadSegmentChange change)
    {
        var problems = Problems.None;

        var originalId = change.OriginalId;
        var geometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod;
        var geometry = change.Geometry ?? Geometry;

        problems += new RoadSegmentGeometryValidator().Validate(originalId, geometryDrawMethod, geometry);

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

        var attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = change.AccessRestriction ?? Attributes.AccessRestriction,
            Category = change.Category ?? Attributes.Category,
            Morphology = change.Morphology ?? Attributes.Morphology,
            Status = change.Status ?? Attributes.Status,
            StreetNameId = change.StreetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType ?? Attributes.SurfaceType,
            EuropeanRoadNumbers = change.EuropeanRoadNumbers?.ToImmutableList() ?? Attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = change.NationalRoadNumbers?.ToImmutableList() ?? Attributes.NationalRoadNumbers
        };
        var segmentLength = geometry.Length;
        problems += new RoadSegmentAttributesValidator().Validate(originalId, attributes, segmentLength);

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
