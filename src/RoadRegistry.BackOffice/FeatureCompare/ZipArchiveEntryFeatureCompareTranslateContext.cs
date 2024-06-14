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
        RoadNodeRecords = new List<RoadNodeFeatureCompareRecord>();
        RoadSegmentRecords = new List<RoadSegmentFeatureCompareRecord>();
    }

    public RoadNodeFeatureCompareRecord FindNotRemovedRoadNode(RoadNodeId id)
    {
        return RoadNodeRecords.NotRemoved().SingleOrDefault(x => x.Id == id)
            ?? RoadNodeRecords.NotRemoved().SingleOrDefault(x => x.Attributes.Id == id);
    }

    public RoadSegmentFeatureCompareRecord FindNotRemovedRoadSegment(RoadSegmentId id)
    {
        return RoadSegmentRecords.NotRemoved().SingleOrDefault(x => x.Id == id)
            ?? RoadSegmentRecords.NotRemoved().SingleOrDefault(x => x.Attributes.Id == id);
    }
}
