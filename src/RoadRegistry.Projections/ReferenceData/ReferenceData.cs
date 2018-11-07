namespace RoadRegistry.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Messages;
    using Aiv.Vbr.Shaperon;

    public class ReferenceData
    {
        public static IReadOnlyCollection<RoadNodeTypeDbaseRecord> RoadNodeTypes => CreateDbaseRecords<RoadNodeTypeDbaseRecord, RoadNodeType>();
        public static IReadOnlyCollection<RoadSegmentAccessRestrictionDbaseRecord> RoadSegmentAccessRestrictions => CreateDbaseRecords<RoadSegmentAccessRestrictionDbaseRecord, RoadSegmentAccessRestriction>();
        public static IReadOnlyCollection<RoadSegmentGeometryDrawMethodDbaseRecord> RoadSegmentGeometryDrawMethods => CreateDbaseRecords<RoadSegmentGeometryDrawMethodDbaseRecord, RoadSegmentGeometryDrawMethod>();
        public static IReadOnlyCollection<RoadSegmentStatusDbaseRecord> RoadSegmentStatuses => CreateDbaseRecords<RoadSegmentStatusDbaseRecord, RoadSegmentStatus>();
        public static IReadOnlyCollection<RoadSegmentMorphologyDbaseRecord> RoadSegmentMorphologies => CreateDbaseRecords<RoadSegmentMorphologyDbaseRecord, RoadSegmentMorphology>();
        public static IReadOnlyCollection<RoadSegmentCategoryDbaseRecord> RoadSegmentCategories => CreateDbaseRecords<RoadSegmentCategoryDbaseRecord, RoadSegmentCategory>();
        public static IReadOnlyCollection<HardeningTypeDbaseRecord> HardeningTypes => CreateDbaseRecords<HardeningTypeDbaseRecord, HardeningType>();
        public static IReadOnlyCollection<LaneDirectionDbaseRecord> LaneDirections => CreateDbaseRecords<LaneDirectionDbaseRecord, LaneDirection>();
        public static IReadOnlyCollection<NumberedRoadSegmentDirectionDbaseRecord> NumberedRoadSegmentDirections => CreateDbaseRecords<NumberedRoadSegmentDirectionDbaseRecord, NumberedRoadSegmentDirection>();
        public static IReadOnlyCollection<ReferencePointTypeDbaseRecord> ReferencePointTypes => CreateDbaseRecords<ReferencePointTypeDbaseRecord, ReferencePointType>();
        public static IReadOnlyCollection<GradeSeparatedJunctionTypeDbaseRecord> GradeSeparatedJunctionTypes => CreateDbaseRecords<GradeSeparatedJunctionTypeDbaseRecord, GradeSeparatedJunctionType>();

        private static IReadOnlyCollection<TDbaseRecord> CreateDbaseRecords<TDbaseRecord, TEnum>()
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
