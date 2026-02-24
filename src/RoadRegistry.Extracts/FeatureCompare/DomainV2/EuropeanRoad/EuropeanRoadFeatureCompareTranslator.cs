namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.EuropeanRoad;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadSegment.Changes;
using RoadSegment;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class EuropeanRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadFeatureCompareTranslator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttEuropweg)
    {
    }

    protected override void HandleIdenticalRoadSegment(
        RoadSegmentFeatureCompareRecord wegsegment,
        ILookup<RoadSegmentTempId, Feature<EuropeanRoadFeatureCompareAttributes>> changeFeatures,
        ILookup<RoadSegmentTempId, Feature<EuropeanRoadFeatureCompareAttributes>> extractFeatures,
        List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures[wegsegment.GetOriginalId()].ToList();
        var wegsegmentExtractFeatures = extractFeatures[wegsegment.GetOriginalId()].ToList();

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures[changeFeature.Attributes.RoadSegmentTempId]
                .Where(x => x.Attributes.Number == changeFeature.Attributes.Number)
                .ToList();
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Added));
            }
            else
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Identical));

                if (leveringExtractFeatures.Count > 1)
                {
                    foreach (var extractFeature in leveringExtractFeatures.Skip(1))
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                    }
                }
            }
        }

        foreach (var extractFeature in wegsegmentExtractFeatures)
        {
            var extractChangeFeatures = changeFeatures[extractFeature.Attributes.RoadSegmentTempId]
                .Where(x => x.Attributes.Number == extractFeature.Attributes.Number)
                .ToList();
            if (!extractChangeFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
            }
        }
    }

    protected override void HandleModifiedRoadSegment(
        RoadSegmentFeatureCompareRecord wegsegment,
        ILookup<RoadSegmentTempId, Feature<EuropeanRoadFeatureCompareAttributes>> changeFeatures,
        ILookup<RoadSegmentTempId, Feature<EuropeanRoadFeatureCompareAttributes>> extractFeatures,
        List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures[wegsegment.GetOriginalId()].ToList();
        var wegsegmentExtractFeatures = extractFeatures[wegsegment.GetOriginalId()].ToList();

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures[wegsegment.GetOriginalId()]
                .Where(x => x.Attributes.Number == changeFeature.Attributes.Number)
                .ToList();
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(changeFeature with
                {
                    Attributes = changeFeature.Attributes with
                    {
                        RoadSegmentTempId = wegsegment.GetActualId()
                    }
                }, RecordType.Added));
            }
            else
            {
                processedRecords.Add(new Record(leveringExtractFeatures.First(), RecordType.Identical));
            }
        }

        foreach (var extractFeature in wegsegmentExtractFeatures)
        {
            var extractChangeFeatures = changeFeatures[wegsegment.GetOriginalId()]
                .Where(x => x.Attributes.Number == extractFeature.Attributes.Number)
                .ToList();

            if (!extractChangeFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
            }
        }
    }

    protected override TranslatedChanges TranslateProcessedRecords(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    {
                        if (changes.TryFindRoadSegmentChange(record.Feature.Attributes.RoadSegmentTempId, out var roadSegmentChange) && roadSegmentChange is AddRoadSegmentChange addRoadSegmentChange)
                        {
                            changes = changes.ReplaceChange(addRoadSegmentChange, addRoadSegmentChange with
                            {
                                EuropeanRoadNumbers = addRoadSegmentChange.EuropeanRoadNumbers.Concat([record.Feature.Attributes.Number]).ToArray()
                            });
                        }
                        else
                        {
                            changes = changes.AppendChange(
                                new AddRoadSegmentToEuropeanRoadChange
                                {
                                    RoadSegmentId = record.Feature.Attributes.RoadSegmentTempId,
                                    Number = record.Feature.Attributes.Number
                                }
                            );
                        }
                    }
                    break;
                case RecordType.RemovedIdentifier:
                    {
                        if (changes.TryFindRoadSegmentChange(record.Feature.Attributes.RoadSegmentTempId, out var roadSegmentChange) && roadSegmentChange is RemoveRoadSegmentChange)
                        {
                            // Do not register removal of number
                        }
                        else
                        {
                            changes = changes.AppendChange(
                                new RemoveRoadSegmentFromEuropeanRoadChange
                                {
                                    RoadSegmentId = record.Feature.Attributes.RoadSegmentTempId,
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
