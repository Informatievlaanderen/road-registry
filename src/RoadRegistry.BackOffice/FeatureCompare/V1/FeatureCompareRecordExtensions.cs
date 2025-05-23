namespace RoadRegistry.BackOffice.FeatureCompare.V1;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.BackOffice.Uploads;

public static class FeatureCompareRecordExtensions
{
    public static IEnumerable<RoadSegmentFeatureCompareRecord> NotRemoved(this IEnumerable<RoadSegmentFeatureCompareRecord> records)
    {
        return records.ThrowIfNull().Where(x => x.RecordType != RecordType.Removed);
    }

    public static IEnumerable<RoadNodeFeatureCompareRecord> NotRemoved(this IEnumerable<RoadNodeFeatureCompareRecord> records)
    {
        return records.ThrowIfNull().Where(x => x.RecordType != RecordType.Removed);
    }

    public static IEnumerable<RoadSegmentFeatureCompareRecord> NotOutlined(this IEnumerable<RoadSegmentFeatureCompareRecord> records)
    {
        return records.ThrowIfNull().Where(x => x.Attributes.Method != RoadSegmentGeometryDrawMethod.Outlined);
    }
}
