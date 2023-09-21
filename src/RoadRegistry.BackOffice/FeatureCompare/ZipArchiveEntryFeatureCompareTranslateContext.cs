namespace RoadRegistry.BackOffice.FeatureCompare;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Uploads;

public class ZipArchiveEntryFeatureCompareTranslateContext: ZipArchiveFeatureReaderContext
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
        return RoadNodeRecords.SingleOrDefault(x => x.Id == id && x.RecordType != RecordType.Removed)
            ?? RoadNodeRecords.SingleOrDefault(x => x.Attributes.Id == id && x.RecordType != RecordType.Removed);
    }

    public IEnumerable<RoadSegmentFeatureCompareRecord> GetNonRemovedRoadSegmentRecords()
    {
        return RoadSegmentRecords.Where(x => x.RecordType != RecordType.Removed);
    }
}
