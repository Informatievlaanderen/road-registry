namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.NationalRoad;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadSegment.Changes;
using RoadSegment;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class NationalRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadFeatureCompareTranslator(NationalRoadFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttNationweg)
    {
    }

    protected override void HandleIdenticalRoadSegment(
        RoadSegmentFeatureCompareRecord wegsegment,
        ILookup<RoadSegmentId, Feature<NationalRoadFeatureCompareAttributes>> changeFeatures,
        ILookup<RoadSegmentId, Feature<NationalRoadFeatureCompareAttributes>> extractFeatures,
        List<Record> processedRecords,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var wegsegmentChangeFeatures = changeFeatures[wegsegment.GetOriginalId()].ToList();
        var wegsegmentExtractFeatures = extractFeatures[wegsegment.GetOriginalId()].ToList();

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var roadSegmentId = context.MapToRoadSegmentId(FeatureType.Change, changeFeature.Attributes.RoadSegmentTempId);
            var leveringExtractFeatures = extractFeatures[roadSegmentId]
                .Where(x => x.Attributes.Number == changeFeature.Attributes.Number)
                .ToList();
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Added, roadSegmentId));
            }
            else
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Identical, roadSegmentId));

                if (leveringExtractFeatures.Count > 1)
                {
                    foreach (var extractFeature in leveringExtractFeatures.Skip(1))
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed, roadSegmentId));
                    }
                }
            }
        }

        foreach (var extractFeature in wegsegmentExtractFeatures)
        {
            var roadSegmentId = context.MapToRoadSegmentId(FeatureType.Extract, extractFeature.Attributes.RoadSegmentTempId);
            var extractChangeFeatures = changeFeatures[roadSegmentId]
                .Where(x => x.Attributes.Number == extractFeature.Attributes.Number)
                .ToList();
            if (!extractChangeFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed, roadSegmentId));
            }
        }
    }

    protected override void HandleModifiedRoadSegment(
        RoadSegmentFeatureCompareRecord wegsegment,
        ILookup<RoadSegmentId, Feature<NationalRoadFeatureCompareAttributes>> changeFeatures,
        ILookup<RoadSegmentId, Feature<NationalRoadFeatureCompareAttributes>> extractFeatures,
        List<Record> processedRecords,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var wegsegmentChangeFeatures = changeFeatures[wegsegment.GetOriginalId()].ToList();
        var wegsegmentExtractFeatures = extractFeatures[wegsegment.GetOriginalId()].ToList();

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures[wegsegment.GetOriginalId()]
                .Where(x => x.Attributes.Number == changeFeature.Attributes.Number)
                .ToList();
            processedRecords.Add(leveringExtractFeatures.Any()
                ? new Record(leveringExtractFeatures.First(), RecordType.Identical, wegsegment.GetActualId())
                : new Record(changeFeature, RecordType.Added, wegsegment.GetActualId()));
        }

        foreach (var extractFeature in wegsegmentExtractFeatures)
        {
            var extractChangeFeatures = changeFeatures[wegsegment.GetOriginalId()]
                .Where(x => x.Attributes.Number == extractFeature.Attributes.Number)
                .ToList();

            if (!extractChangeFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed, wegsegment.GetOriginalId()));
            }
        }
    }

    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                {
                    if (changes.TryFindRoadSegmentChange(record.RoadSegmentId, out var roadSegmentChange) && roadSegmentChange is AddRoadSegmentChange addRoadSegmentChange)
                    {
                        changes = changes.ReplaceChange(addRoadSegmentChange, addRoadSegmentChange with
                        {
                            NationalRoadNumbers = addRoadSegmentChange.NationalRoadNumbers.Concat([record.Feature.Attributes.Number]).ToArray()
                        });
                    }
                    else
                    {
                        changes = changes.AppendChange(
                            new AddRoadSegmentToNationalRoadChange
                            {
                                RoadSegmentId = record.RoadSegmentId,
                                Number = record.Feature.Attributes.Number
                            }
                        );
                    }
                }
                    break;
                case RecordType.RemovedIdentifier:
                {
                    if (changes.TryFindRoadSegmentChange(record.RoadSegmentId, out var roadSegmentChange) && roadSegmentChange is RemoveRoadSegmentChange)
                    {
                        // Do not register removal of number
                    }
                    else
                    {
                        changes = changes.AppendChange(
                            new RemoveRoadSegmentFromNationalRoadChange
                            {
                                RoadSegmentId = record.RoadSegmentId,
                                Number = record.Feature.Attributes.Number
                            }
                        );
                    }
                }
                    break;
            }
        }

        return changes;
    }
}
