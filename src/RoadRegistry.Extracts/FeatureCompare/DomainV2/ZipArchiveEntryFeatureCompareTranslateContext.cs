namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using RoadNode;
using RoadRegistry.Extracts.Uploads;
using RoadSegment;
using TransactionZone;

public class ZipArchiveEntryFeatureCompareTranslateContext : ZipArchiveFeatureReaderContext
{
    public ZipArchive Archive { get; }
    public TransactionZoneFeatureCompareAttributes TransactionZone { get; set; }
    public List<RoadNodeFeatureCompareRecord> RoadNodeRecords { get; }
    public IReadOnlyList<RoadSegmentFeatureCompareRecord> RoadSegmentRecords { get; }

    private List<RoadSegmentFeatureCompareRecord> _roadSegmentRecords = [];
    private Dictionary<(FeatureType, RoadSegmentTempId), RoadSegmentId> _roadSegmentTempIdToActualIdMapping = new();

    public ZipArchiveEntryFeatureCompareTranslateContext(ZipArchive archive, ZipArchiveMetadata metadata)
        : base(metadata)
    {
        Archive = archive;
        RoadNodeRecords = [];
        RoadSegmentRecords = _roadSegmentRecords.AsReadOnly();
    }

    public void AddRoadSegments(IEnumerable<RoadSegmentFeatureCompareRecord> roadSegmentRecords)
    {
        foreach (var roadSegmentRecord in roadSegmentRecords)
        {
            _roadSegmentRecords.Add(roadSegmentRecord);
            foreach (var flatFeature in roadSegmentRecord.FlatFeatures)
            {
                _roadSegmentTempIdToActualIdMapping.Add((roadSegmentRecord.FeatureType, flatFeature.Attributes.TempId), roadSegmentRecord.RoadSegmentId);
            }
        }
    }

    public RoadSegmentId MapToRoadSegmentId(FeatureType featureType, RoadSegmentTempId roadSegmentTempId) => _roadSegmentTempIdToActualIdMapping[(featureType, roadSegmentTempId)];

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

    public RoadSegmentFeatureCompareRecord? FindNotRemovedRoadSegmentByOriginalId(FeatureType featureType, RoadSegmentTempId originalTempId)
    {
        if (!_roadSegmentTempIdToActualIdMapping.TryGetValue((featureType, originalTempId), out var actualId))
        {
            return null;
        }

        return FindNotRemovedRoadSegmentByOriginalId(featureType, actualId);
    }

    public RoadSegmentFeatureCompareRecord? FindNotRemovedRoadSegmentByOriginalId(FeatureType featureType, RoadSegmentId originalId)
    {
        var matchingFeatures = RoadSegmentRecords
            .Where(x => x.FeatureType == featureType)
            .NotRemoved()
            .Where(x => x.GetOriginalId() == originalId)
            .ToList();

        if (matchingFeatures.Count > 1)
        {
            var matchingFeaturesInfo = string.Join("\n", matchingFeatures.Select(feature => $"RoadSegment #{feature.RecordNumber}, ID: {feature.RoadSegmentId}, FeatureType: {feature.FeatureType}, RecordType: {feature.RecordType}"));
            throw new InvalidOperationException($"Found {matchingFeatures.Count} processed road segments with original ID {originalId} while only 1 is expected.\n{matchingFeaturesInfo}");
        }

        return matchingFeatures.SingleOrDefault();
    }
}
