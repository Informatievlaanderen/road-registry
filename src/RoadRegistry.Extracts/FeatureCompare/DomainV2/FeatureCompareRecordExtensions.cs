namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Uploads;
using RoadSegment;

public static class FeatureCompareRecordExtensions
{
    public static IEnumerable<T> NotRemoved<T>(this IEnumerable<T> records)
        where T : IFeatureCompareRecord
    {
        return records.ThrowIfNull().Where(x => x.RecordType != RecordType.Removed);
    }

    public static IEnumerable<RoadSegmentFeatureCompareRecord> OnlyGerealiseerd(this IEnumerable<RoadSegmentFeatureCompareRecord> records)
    {
        return records.ThrowIfNull().Where(x => x.Attributes.Status == RoadSegmentStatusV2.Gerealiseerd);
    }
}
