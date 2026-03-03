namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<FeatureType, List<RoadNodeFeatureCompareRecord>> _roadNodeRecords = [];
    private readonly ConcurrentDictionary<FeatureType, List<RoadSegmentFeatureCompareRecord>> _roadSegmentRecords = [];
    private readonly ConcurrentDictionary<(FeatureType, RoadSegmentTempId), RoadSegmentId> _roadSegmentTempIdToActualIdMapping = new();

    public ZipArchiveEntryFeatureCompareTranslateContext(ZipArchive archive, ZipArchiveMetadata metadata)
        : base(metadata)
    {
        Archive = archive;
    }

    public void AddRoadNodeRecords(IReadOnlyCollection<RoadNodeFeatureCompareRecord> records)
    {
        if (records.Count == 0)
        {
            return;
        }

        var featureType = records.Select(x => x.FeatureType).Distinct().Single();

        foreach (var record in records)
        {
            _roadNodeRecords.TryAdd(featureType, new());
            _roadNodeRecords[featureType].Add(record);
        }
    }

    public IReadOnlyList<RoadNodeFeatureCompareRecord> GetRoadNodeRecords(FeatureType featureType)
    {
        return _roadNodeRecords.TryGetValue(featureType, out var records)
            ? records.AsReadOnly()
            : [];
    }

    public void AddRoadSegmentRecords(IReadOnlyCollection<RoadSegmentFeatureCompareRecord> records)
    {
        if (records.Count == 0)
        {
            return;
        }

        var featureType = records.Select(x => x.FeatureType).Distinct().Single();

        foreach (var record in records)
        {
            _roadSegmentRecords.TryAdd(featureType, new());
            _roadSegmentRecords[featureType].Add(record);

            foreach (var flatFeature in record.FlatFeatures)
            {
                if (!_roadSegmentTempIdToActualIdMapping.TryAdd((record.FeatureType, flatFeature.Attributes.TempId), record.GetActualId()))
                {
                    throw new InvalidOperationException($"Duplicate road segment temp ID {flatFeature.Attributes.TempId} for feature type {record.FeatureType} and road segment ID {record.GetActualId()}");
                }
            }
        }
    }

    public RoadSegmentId MapToRoadSegmentId(FeatureType featureType, RoadSegmentTempId roadSegmentTempId) => _roadSegmentTempIdToActualIdMapping[(featureType, roadSegmentTempId)];

    public IReadOnlyList<RoadSegmentFeatureCompareRecord> GetRoadSegmentRecords(FeatureType featureType)
    {
        return _roadSegmentRecords.TryGetValue(featureType, out var records)
            ? records.AsReadOnly()
            : [];
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
        var matchingFeatures = GetRoadSegmentRecords(featureType)
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
