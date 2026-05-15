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
    public ConcurrentDictionary<RoadNodeId, byte> TemporarySchijnknoopIds { get; }

    private readonly ConcurrentDictionary<FeatureType, List<RoadNodeFeatureCompareRecord>> _roadNodeRecords = [];
    private readonly ConcurrentDictionary<FeatureType, List<RoadSegmentFeatureCompareRecord>> _roadSegmentRecords = [];
    private readonly ConcurrentDictionary<(FeatureType, RoadSegmentTempId), (RoadSegmentId, RecordType)> _roadSegmentTempIdToActualIdMapping = new();
    // Index of non-removed road segment records by (FeatureType, actual RoadSegmentId).
    // Lets FindNotRemovedRoadSegmentByActualId run in O(1) instead of scanning every record per call,
    // which was making ValidateRoadSegmentTempIds and the GradeSeparatedJunction translator quadratic on big uploads.
    private readonly ConcurrentDictionary<(FeatureType, RoadSegmentId), RoadSegmentFeatureCompareRecord> _notRemovedRoadSegmentByActualId = new();

    public ZipArchiveEntryFeatureCompareTranslateContext(ZipArchive archive, ZipArchiveMetadata metadata)
        : base(metadata)
    {
        Archive = archive;
        TemporarySchijnknoopIds = [];
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

            if (record.RecordType != RecordType.Removed)
            {
                var actualIdKey = (record.FeatureType, record.GetActualId());
                if (!_notRemovedRoadSegmentByActualId.TryAdd(actualIdKey, record))
                {
                    var existing = _notRemovedRoadSegmentByActualId[actualIdKey];
                    throw new InvalidOperationException(
                        $"Found 2 processed road segments with actual ID {actualIdKey.Item2} while only 1 is expected.\n" +
                        $"RoadSegment #{existing.RecordNumber}, ID: {existing.RoadSegmentId}, FeatureType: {existing.FeatureType}, RecordType: {existing.RecordType}\n" +
                        $"RoadSegment #{record.RecordNumber}, ID: {record.RoadSegmentId}, FeatureType: {record.FeatureType}, RecordType: {record.RecordType}");
                }
            }

            foreach (var flatFeature in record.FlatFeatures)
            {
                var mappingKey = (record.FeatureType, flatFeature.Attributes.TempId);
                if (!_roadSegmentTempIdToActualIdMapping.TryAdd(mappingKey, (record.GetActualId(), record.RecordType)))
                {
                    if (record.RecordType == RecordType.Removed)
                    {
                        continue;
                    }

                    var existingMapping = _roadSegmentTempIdToActualIdMapping[mappingKey];
                    if (existingMapping.Item2 == RecordType.Removed)
                    {
                        _roadSegmentTempIdToActualIdMapping[mappingKey] = (record.GetActualId(), record.RecordType);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Duplicate road segment temp ID {flatFeature.Attributes.TempId} for feature type {record.FeatureType} and road segment ID {record.GetActualId()}");
                    }
                }
            }
        }
    }

    public RoadSegmentId MapToRoadSegmentId(FeatureType featureType, RoadSegmentTempId roadSegmentTempId) => _roadSegmentTempIdToActualIdMapping[(featureType, roadSegmentTempId)].Item1;

    public RoadSegmentId MapToRoadSegmentId(FeatureType[] featureTypes, RoadSegmentTempId roadSegmentTempId)
    {
        foreach (var featureType in featureTypes)
        {
            if (_roadSegmentTempIdToActualIdMapping.TryGetValue((featureType, roadSegmentTempId), out var mapping))
            {
                return mapping.Item1;
            }
        }

        throw new InvalidOperationException($"Could not find road segment ID for road segment temp ID {roadSegmentTempId}");
    }

    public IReadOnlyList<RoadSegmentFeatureCompareRecord> GetRoadSegmentRecords(FeatureType featureType)
    {
        return _roadSegmentRecords.TryGetValue(featureType, out var records)
            ? records.AsReadOnly()
            : [];
    }

    public RoadSegmentFeatureCompareRecord? FindNotRemovedRoadSegmentByTempId(FeatureType[] featureTypes, RoadSegmentTempId originalTempId)
    {
        foreach (var featureType in featureTypes)
        {
            if (!_roadSegmentTempIdToActualIdMapping.TryGetValue((featureType, originalTempId), out var actualId))
            {
                continue;
            }

            var roadSegment = _notRemovedRoadSegmentByActualId.GetValueOrDefault((featureType, actualId.Item1));
            if (roadSegment is not null)
            {
                return roadSegment;
            }
        }

        return null;
    }
}
