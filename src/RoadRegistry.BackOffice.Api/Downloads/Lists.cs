namespace RoadRegistry.Api.Downloads
{
    using System;
    using BackOffice.Schema.ReferenceData;

    public static class Lists
    {
        public static GradeSeparatedJunctionTypeDbaseRecord[] AllGradeSeparatedJunctionTypeDbaseRecords =
            Array.ConvertAll(
                Model.GradeSeparatedJunctionType.All,
                item => new GradeSeparatedJunctionTypeDbaseRecord
                {
                    TYPE = {Value = item.Translation.Identifier},
                    LBLTYPE = {Value = item.Translation.Name},
                    DEFTYPE = {Value = item.Translation.Description}
                });

        public static LaneDirectionDbaseRecord[] AllLaneDirectionDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentLaneDirection.All,
                item => new LaneDirectionDbaseRecord
                {
                    RICHTING = { Value = item.Translation.Identifier},
                    LBLRICHT= { Value = item.Translation.Name},
                    DEFRICHT= { Value = item.Translation.Description}
                });

        public static NumberedRoadSegmentDirectionDbaseRecord[] AllNumberedRoadSegmentDirectionDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentNumberedRoadDirection.All,
                item => new NumberedRoadSegmentDirectionDbaseRecord
                {
                    RICHTING = { Value = item.Translation.Identifier},
                    LBLRICHT= { Value = item.Translation.Name},
                    DEFRICHT= { Value = item.Translation.Description}
                });

        public static RoadNodeTypeDbaseRecord[] AllRoadNodeTypeDbaseRecords =
            Array.ConvertAll(
                Model.RoadNodeType.All,
                item => new RoadNodeTypeDbaseRecord
                {
                    TYPE = {Value = item.Translation.Identifier},
                    LBLTYPE = {Value = item.Translation.Name},
                    DEFTYPE = {Value = item.Translation.Description}
                });

        public static RoadSegmentAccessRestrictionDbaseRecord[] AllRoadSegmentAccessRestrictionDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentAccessRestriction.All,
                item => new RoadSegmentAccessRestrictionDbaseRecord
                {
                    TYPE = {Value = item.Translation.Identifier},
                    LBLTYPE = {Value = item.Translation.Name},
                    DEFTYPE = {Value = item.Translation.Description}
                });

        public static RoadSegmentCategoryDbaseRecord[] AllRoadSegmentCategoryDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentCategory.All,
                item => new RoadSegmentCategoryDbaseRecord
                {
                    WEGCAT = { Value = item.Translation.Identifier },
                    LBLWEGCAT = {Value = item.Translation.Name},
                    DEFWEGCAT = {Value = item.Translation.Description}
                });

        public static RoadSegmentGeometryDrawMethodDbaseRecord[] AllRoadSegmentGeometryDrawMethodDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentGeometryDrawMethod.All,
                item => new RoadSegmentGeometryDrawMethodDbaseRecord
                {
                    METHODE = { Value = item.Translation.Identifier},
                    LBLMETHOD = {Value = item.Translation.Name},
                    DEFMETHOD = {Value = item.Translation.Description}
                });

        public static RoadSegmentMorphologyDbaseRecord[] AllRoadSegmentMorphologyDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentMorphology.All,
                item => new RoadSegmentMorphologyDbaseRecord
                {
                    MORF = { Value = item.Translation.Identifier},
                    LBLMORF = {Value = item.Translation.Name},
                    DEFMORF = {Value = item.Translation.Description}
                });

        public static RoadSegmentStatusDbaseRecord[] AllRoadSegmentStatusDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentStatus.All,
                item => new RoadSegmentStatusDbaseRecord
                {
                    STATUS = { Value = item.Translation.Identifier},
                    LBLSTATUS = {Value = item.Translation.Name},
                    DEFSTATUS = {Value = item.Translation.Description}
                });

        public static SurfaceTypeDbaseRecord[] AllSurfaceTypeDbaseRecords =
            Array.ConvertAll(
                Model.RoadSegmentSurfaceType.All,
                item => new SurfaceTypeDbaseRecord
                {
                    TYPE = {Value = item.Translation.Identifier},
                    LBLTYPE = {Value = item.Translation.Name},
                    DEFTYPE = {Value = item.Translation.Description}
                });
    }
}
