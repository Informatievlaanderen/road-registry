namespace RoadRegistry.Projections
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Events;
    using Aiv.Vbr.Shaperon;

    public class TypeReferences
    {
        public static RoadNodeTypeDbaseRecord[] RoadNodeTypes => CreateDbaseRecords<RoadNodeTypeDbaseRecord, RoadNodeType>();
        public static RoadSegmentAccessRestrictionDbaseRecord[] RoadSegmentAccessRestrictions => CreateDbaseRecords<RoadSegmentAccessRestrictionDbaseRecord, RoadSegmentAccessRestriction>();
        public static RoadSegmentGeometryDrawMethodDbaseRecord[] RoadSegmentGeometryDrawMethods => CreateDbaseRecords<RoadSegmentGeometryDrawMethodDbaseRecord, RoadSegmentGeometryDrawMethod>();
        public static RoadSegmentStatusDbaseRecord[] RoadSegmentStatuses => CreateDbaseRecords<RoadSegmentStatusDbaseRecord, RoadSegmentStatus>();
        public static RoadSegmentMorphologyDbaseRecord[] RoadSegmentMorphologies => CreateDbaseRecords<RoadSegmentMorphologyDbaseRecord, RoadSegmentMorphology>();
        public static RoadSegmentCategoryDbaseRecord[] RoadSegmentCategories => CreateDbaseRecords<RoadSegmentCategoryDbaseRecord, RoadSegmentCategory>();
        public static HardeningTypeDbaseRecord[] HardeningTypes => CreateDbaseRecords<HardeningTypeDbaseRecord, HardeningType>();
        public static LaneDirectionDbaseRecord[] LaneDirections => CreateDbaseRecords<LaneDirectionDbaseRecord, LaneDirection>();
        public static NumberedRoadSegmentDirectionDbaseRecord[] NumberedRoadSegmentDirections => CreateDbaseRecords<NumberedRoadSegmentDirectionDbaseRecord, NumberedRoadSegmentDirection>();
        public static ReferencePointTypeDbaseRecord[] ReferencePointTypes => CreateDbaseRecords<ReferencePointTypeDbaseRecord, ReferencePointType>();
        public static GradeSeparatedJunctionTypeDbaseRecord[] GradeSeparatedJunctionTypes => CreateDbaseRecords<GradeSeparatedJunctionTypeDbaseRecord, GradeSeparatedJunctionType>();

        private static TDbaseRecord[] CreateDbaseRecords<TDbaseRecord, TEnum>()
            where TDbaseRecord : DbaseRecord
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            return ((TEnum[]) Enum.GetValues(typeof(TEnum)))
                .OrderBy(value => value.ToInt32(CultureInfo.InvariantCulture))
                .Select(value => (TDbaseRecord)Activator.CreateInstance(typeof(TDbaseRecord), value))
                .ToArray();
        }
    }
}
