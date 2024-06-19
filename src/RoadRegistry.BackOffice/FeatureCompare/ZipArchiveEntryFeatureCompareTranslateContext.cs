namespace RoadRegistry.BackOffice.FeatureCompare;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Uploads;

public class ZipArchiveEntryFeatureCompareTranslateContext : ZipArchiveFeatureReaderContext
{
    public ZipArchive Archive { get; }
    public List<RoadNodeFeatureCompareRecord> RoadNodeRecords { get; }
    public List<RoadSegmentFeatureCompareRecord> RoadSegmentRecords { get; }

    public ZipArchiveEntryFeatureCompareTranslateContext(ZipArchive archive, ZipArchiveMetadata metadata)
        : base(metadata)
    {
        Archive = archive;
        RoadNodeRecords = [];
        RoadSegmentRecords = [];
    }

    public RoadNodeFeatureCompareRecord FindNotRemovedRoadNode(RoadNodeId id)
    {
        return RoadNodeRecords.NotRemoved().SingleOrDefault(x => x.GetActualId() == id)
            ?? RoadNodeRecords.NotRemoved().SingleOrDefault(x => x.GetOriginalId() == id);
    }

    public RoadSegmentFeatureCompareRecord FindRoadSegment(RoadSegmentId id)
    {
        return RoadSegmentRecords.SingleOrDefault(x => x.GetActualId() == id)
            ?? RoadSegmentRecords.SingleOrDefault(x => x.GetOriginalId() == id);
    }
}
