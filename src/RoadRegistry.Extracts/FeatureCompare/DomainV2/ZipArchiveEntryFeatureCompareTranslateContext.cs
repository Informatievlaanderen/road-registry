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

    private readonly List<RoadSegmentFeatureCompareRecord> _roadSegmentRecords = [];
    private readonly Dictionary<(FeatureType, RoadSegmentTempId), RoadSegmentId> _roadSegmentTempIdToActualIdMapping = new();

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
                _roadSegmentTempIdToActualIdMapping.Add((roadSegmentRecord.FeatureType, flatFeature.Attributes.TempId), roadSegmentRecord.GetActualId());
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

    public RoadSegmentFeatureCompareRecord? FindNotRemovedRoadSegmentByTempId(FeatureType featureType, RoadSegmentTempId originalTempId)
    {
        if (!_roadSegmentTempIdToActualIdMapping.TryGetValue((featureType, originalTempId), out var actualId))
        {
            return null;
        }

        return FindNotRemovedRoadSegmentByActualId(featureType, actualId);
    }

    private RoadSegmentFeatureCompareRecord? FindNotRemovedRoadSegmentByActualId(FeatureType featureType, RoadSegmentId actualId)
    {
        var matchingFeatures = RoadSegmentRecords
            .Where(x => x.FeatureType == featureType)
            .NotRemoved()
            .Where(x => x.GetActualId() == actualId)
            .ToList();

        if (matchingFeatures.Count > 1)
        {
            var matchingFeaturesInfo = string.Join("\n", matchingFeatures.Select(feature => $"RoadSegment #{feature.RecordNumber}, ID: {feature.RoadSegmentId}, FeatureType: {feature.FeatureType}, RecordType: {feature.RecordType}"));
            throw new InvalidOperationException($"Found {matchingFeatures.Count} processed road segments with actual ID {actualId} while only 1 is expected.\n{matchingFeaturesInfo}");
        }

        return matchingFeatures.SingleOrDefault();
    }
}
