namespace RoadRegistry.Extracts.Schemas.ExtractV1.Lists;

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

    public static LaneDirectionDbaseRecord[] AllLaneDirectionDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentLaneDirection.All,
            item => new LaneDirectionDbaseRecord
            {
                RICHTING = { Value = item.Translation.Identifier },
                LBLRICHT = { Value = item.Translation.Name },
                DEFRICHT = { Value = item.Translation.Description }
            });

    public static NumberedRoadSegmentDirectionDbaseRecord[] AllNumberedRoadSegmentDirectionDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentNumberedRoadDirection.All,
            item => new NumberedRoadSegmentDirectionDbaseRecord
            {
                RICHTING = { Value = item.Translation.Identifier },
                LBLRICHT = { Value = item.Translation.Name },
                DEFRICHT = { Value = item.Translation.Description }
            });

    public static RoadNodeTypeDbaseRecord[] AllRoadNodeTypeDbaseRecords =>
        Array.ConvertAll(
            RoadNodeType.All,
            item => new RoadNodeTypeDbaseRecord
            {
                TYPE = { Value = item.Translation.Identifier },
                LBLTYPE = { Value = item.Translation.Name },
                DEFTYPE = { Value = item.Translation.Description }
            });

    public static RoadSegmentAccessRestrictionDbaseRecord[] AllRoadSegmentAccessRestrictionDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentAccessRestriction.All,
            item => new RoadSegmentAccessRestrictionDbaseRecord
            {
                TYPE = { Value = item.Translation.Identifier },
                LBLTYPE = { Value = item.Translation.Name },
                DEFTYPE = { Value = item.Translation.Description }
            });

    public static RoadSegmentCategoryDbaseRecord[] AllRoadSegmentCategoryDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentCategory.All,
            item => new RoadSegmentCategoryDbaseRecord
            {
                WEGCAT = { Value = item.Translation.Identifier },
                LBLWEGCAT = { Value = item.Translation.Name },
                DEFWEGCAT = { Value = item.Translation.Description }
            });

    public static RoadSegmentGeometryDrawMethodDbaseRecord[] AllRoadSegmentGeometryDrawMethodDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentGeometryDrawMethod.All,
            item => new RoadSegmentGeometryDrawMethodDbaseRecord
            {
                METHODE = { Value = item.Translation.Identifier },
                LBLMETHOD = { Value = item.Translation.Name },
                DEFMETHOD = { Value = item.Translation.Description }
            });

    public static RoadSegmentMorphologyDbaseRecord[] AllRoadSegmentMorphologyDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentMorphology.All,
            item => new RoadSegmentMorphologyDbaseRecord
            {
                MORF = { Value = item.Translation.Identifier },
                LBLMORF = { Value = item.Translation.Name },
                DEFMORF = { Value = item.Translation.Description }
            });

    public static RoadSegmentStatusDbaseRecord[] AllRoadSegmentStatusDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentStatus.All,
            item => new RoadSegmentStatusDbaseRecord
            {
                STATUS = { Value = item.Translation.Identifier },
                LBLSTATUS = { Value = item.Translation.Name },
                DEFSTATUS = { Value = item.Translation.Description }
            });

    public static SurfaceTypeDbaseRecord[] AllSurfaceTypeDbaseRecords =>
        Array.ConvertAll(
            RoadSegmentSurfaceType.All,
            item => new SurfaceTypeDbaseRecord
            {
                TYPE = { Value = item.Translation.Identifier },
                LBLTYPE = { Value = item.Translation.Name },
                DEFTYPE = { Value = item.Translation.Description }
            });
}
