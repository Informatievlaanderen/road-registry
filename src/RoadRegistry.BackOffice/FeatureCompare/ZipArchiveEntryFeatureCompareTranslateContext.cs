namespace RoadRegistry.BackOffice.FeatureCompare;

using System.Collections.Generic;
using System.IO.Compression;
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
}
