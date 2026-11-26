namespace RoadRegistry.Extracts.FeatureCompare.Inwinning;

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
    // Temp ids that more than one record of the same feature type claims, and that therefore name no road segment
    // at all. See AddRoadSegmentRecords.
    private readonly ConcurrentDictionary<(FeatureType, RoadSegmentTempId), byte> _ambiguousRoadSegmentTempIds = new();
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
                if (_ambiguousRoadSegmentTempIds.ContainsKey(mappingKey))
                {
                    continue;
                }

                if (_roadSegmentTempIdToActualIdMapping.TryAdd(mappingKey, (record.GetActualId(), record.RecordType)))
                {
                    continue;
                }

                if (record.RecordType == RecordType.Removed)
                {
                    continue;
                }

                var existingMapping = _roadSegmentTempIdToActualIdMapping[mappingKey];
                if (existingMapping.Item2 == RecordType.Removed)
                {
                    _roadSegmentTempIdToActualIdMapping[mappingKey] = (record.GetActualId(), record.RecordType);
                    continue;
                }

                if (existingMapping.Item1 == record.GetActualId())
                {
                    continue;
                }

                // The change files are the uploader's own and their temp ids are checked for uniqueness before any
                // of this, so one claimed twice there is a bug rather than a file to deal with.
                if (record.FeatureType == FeatureType.Change)
                {
                    throw new InvalidOperationException($"Duplicate road segment temp ID {flatFeature.Attributes.TempId} for feature type {record.FeatureType} and road segment ID {record.GetActualId()}");
                }

                // The extract and the integration data are generated, and a temp id in them is no more than the key
                // the road numbers and the crossings point at a road segment with - nothing else reads it. One
                // claimed by two records says nothing about which of the two is meant, so it stops naming either:
                // whatever goes on to use it is reported where it is used, and a temp id nothing uses is no reason
                // to refuse an upload over.
                _ambiguousRoadSegmentTempIds.TryAdd(mappingKey, default);
                _roadSegmentTempIdToActualIdMapping.TryRemove(mappingKey, out _);
            }
        }
    }

    public RoadSegmentId MapToRoadSegmentId(FeatureType featureType, RoadSegmentTempId roadSegmentTempId)
    {
        var roadSegmentId = TryMapToRoadSegmentId(featureType, roadSegmentTempId);
        if (roadSegmentId is not null)
        {
            return roadSegmentId.Value;
        }

        throw new InvalidOperationException($"Could not find road segment ID for road segment temp ID {roadSegmentTempId} in feature type {featureType}");
    }

    public RoadSegmentId? TryMapToRoadSegmentId(FeatureType featureType, RoadSegmentTempId roadSegmentTempId)
    {
        return _roadSegmentTempIdToActualIdMapping.TryGetValue((featureType, roadSegmentTempId), out var mapping)
            ? mapping.Item1
            : null;
    }

    public RoadSegmentId MapToRoadSegmentId(FeatureType[] featureTypes, RoadSegmentTempId roadSegmentTempId)
    {
        var roadSegmentId = TryMapToRoadSegmentId(featureTypes, roadSegmentTempId);
        if (roadSegmentId is not null)
        {
            return roadSegmentId.Value;
        }

        throw new InvalidOperationException($"Could not find road segment ID for road segment temp ID {roadSegmentTempId}");
    }

    public RoadSegmentId? TryMapToRoadSegmentId(FeatureType[] featureTypes, RoadSegmentTempId roadSegmentTempId)
    {
        foreach (var featureType in featureTypes)
        {
            var roadSegmentId = TryMapToRoadSegmentId(featureType, roadSegmentTempId);
            if (roadSegmentId is not null)
            {
                return roadSegmentId;
            }
        }

        return null;
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
