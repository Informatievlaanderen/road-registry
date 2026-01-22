namespace RoadRegistry.Extracts.Schemas.DomainV2.Lists;

using System;

public static class Lists
{
    public static GradeSeparatedJunctionTypeDbaseRecord[] AllGradeSeparatedJunctionTypeDbaseRecords =>
        Array.ConvertAll(
            GradeSeparatedJunctionType.All,
            item => new GradeSeparatedJunctionTypeDbaseRecord
            {
                TYPE = { Value = item.Translation.Identifier },
                LBLTYPE = { Value = item.Translation.Name },
                DEFTYPE = { Value = item.Translation.Description }
            });

    public static RoadNodeTypeDbaseRecord[] AllRoadNodeTypeDbaseRecords =>
        Array.ConvertAll(
            RoadNodeTypeV2.All,
            item => new RoadNodeTypeDbaseRecord
            {
                TYPE = { Value = item.Translation.Identifier },
                LBLTYPE = { Value = item.Translation.Name },
                DEFTYPE = { Value = item.Translation.Description }
            });

    public static RoadSegmentAccessRestrictionDbaseRecord[] AllRoadSegmentAccessRestrictionDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentAccessRestrictionV2.All,
            item => new RoadSegmentAccessRestrictionDbaseRecord
            {
                TYPE = { Value = item.Translation.Identifier },
                LBLTYPE = { Value = item.Translation.Name },
                DEFTYPE = { Value = item.Translation.Description }
            });

    public static RoadSegmentCategoryDbaseRecord[] AllRoadSegmentCategoryDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentCategoryV2.All,
            item => new RoadSegmentCategoryDbaseRecord
            {
                WEGCAT = { Value = item.Translation.Identifier },
                LBLWEGCAT = { Value = item.Translation.Name },
                DEFWEGCAT = { Value = item.Translation.Description }
            });

    public static RoadSegmentMorphologyDbaseRecord[] AllRoadSegmentMorphologyDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentMorphologyV2.All,
            item => new RoadSegmentMorphologyDbaseRecord
            {
                MORF = { Value = item.Translation.Identifier },
                LBLMORF = { Value = item.Translation.Name },
                DEFMORF = { Value = item.Translation.Description }
            });

    public static RoadSegmentStatusDbaseRecord[] AllRoadSegmentStatusDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentStatusV2.All,
            item => new RoadSegmentStatusDbaseRecord
            {
                STATUS = { Value = item.Translation.Identifier },
                LBLSTATUS = { Value = item.Translation.Name },
                DEFSTATUS = { Value = item.Translation.Description }
            });

    public static SurfaceTypeDbaseRecord[] AllSurfaceTypeDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentSurfaceTypeV2.All,
            item => new SurfaceTypeDbaseRecord
            {
                TYPE = { Value = item.Translation.Identifier },
                LBLTYPE = { Value = item.Translation.Name },
                DEFTYPE = { Value = item.Translation.Description }
            });
}
