namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using RoadNode;
using RoadRegistry.Extracts.Uploads;
using RoadSegment;

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

    public RoadNodeFeatureCompareRecord? FindNotRemovedRoadNode(RoadNodeId id)
    {
        return RoadNodeRecords.NotRemoved().SingleOrDefault(x => x.GetActualId() == id)
            ?? RoadNodeRecords.NotRemoved().SingleOrDefault(x => x.GetOriginalId() == id);
    }

    public RoadSegmentFeatureCompareRecord? FindRoadSegment(RoadSegmentId id)
    {
        return RoadSegmentRecords.SingleOrDefault(x => x.GetActualId() == id)
            ?? RoadSegmentRecords.SingleOrDefault(x => x.GetOriginalId() == id);
    }

    public RoadSegmentFeatureCompareRecord? FindNotRemovedRoadSegmentByOriginalId(RoadSegmentId originalId)
    {
        var matchingFeatures = RoadSegmentRecords
            .NotRemoved()
            .Where(x => x.GetOriginalId() == originalId)
            .ToList();

        if (matchingFeatures.Count > 1)
        {
            var matchingFeaturesInfo = string.Join("\n", matchingFeatures.Select(feature => $"RoadSegment #{feature.RecordNumber}, ID: {feature.Id}, FeatureType: {feature.FeatureType}, RecordType: {feature.RecordType}"));
            throw new InvalidOperationException($"Found {matchingFeatures.Count} processed road segments with original ID {originalId} while only 1 is expected.\n{matchingFeaturesInfo}");
        }

        return matchingFeatures.SingleOrDefault();
    }
}
