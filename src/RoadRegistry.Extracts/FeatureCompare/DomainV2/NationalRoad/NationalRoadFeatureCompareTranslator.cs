namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.NationalRoad;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadSegment.Changes;
using RoadSegment;
using TranslatedChanges = DomainV2.TranslatedChanges;

public class NationalRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadFeatureCompareTranslator(NationalRoadFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttNationweg, nameof(RoadSegmentNationalRoadAttributeDbaseRecord.NW_OIDN))
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

        var changeFeaturesByNumber = wegsegmentChangeFeatures.ToLookup(x => x.Attributes.Number);
        var extractFeaturesByNumber = wegsegmentExtractFeatures.ToLookup(x => x.Attributes.Number);

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var roadSegmentId = context.MapToRoadSegmentId(FeatureType.Change, changeFeature.Attributes.RoadSegmentTempId);
            var leveringExtractFeatures = extractFeaturesByNumber[changeFeature.Attributes.Number].ToArray();
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Added, roadSegmentId));
            }
            else
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Identical, roadSegmentId));

                if (leveringExtractFeatures.Length > 1)
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
            var hasExtractChangeFeatures = changeFeaturesByNumber[extractFeature.Attributes.Number].Any();
            if (!hasExtractChangeFeatures)
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

        var changeFeaturesByNumber = wegsegmentChangeFeatures.ToLookup(x => x.Attributes.Number);
        var extractFeaturesByNumber = wegsegmentExtractFeatures.ToLookup(x => x.Attributes.Number);

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeaturesByNumber[changeFeature.Attributes.Number].ToArray();
            processedRecords.Add(leveringExtractFeatures.Any()
                ? new Record(leveringExtractFeatures.First(), RecordType.Identical, wegsegment.GetActualId())
                : new Record(changeFeature, RecordType.Added, wegsegment.GetActualId()));
        }

        foreach (var extractFeature in wegsegmentExtractFeatures)
        {
            var hasExtractChangeFeatures = changeFeaturesByNumber[extractFeature.Attributes.Number].Any();
            if (!hasExtractChangeFeatures)
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed, wegsegment.GetOriginalId()));
            }
        }
    }

    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var group in records.GroupBy(x => x.RoadSegmentId))
        {
            var roadSegmentId = group.Key;

            var removedNumbers = group.Where(x => x.RecordType == RecordType.Removed).Select(x => x.Feature.Attributes.Number).Distinct().ToArray();
            if (removedNumbers.Any())
            {
                if (changes.TryFindRoadSegmentChange(roadSegmentId, out var roadSegmentChange) && roadSegmentChange is RemoveRoadSegmentChange)
                {
                    // Do not register removal of numbers
                }
                else
                {
                    foreach (var number in removedNumbers)
                    {
                        changes = changes.AppendChange(
                            new RemoveRoadSegmentFromNationalRoadChange
                            {
                                RoadSegmentId = roadSegmentId,
                                Number = number
                            }
                        );
                    }
                }
            }

            var addedNumbers = group.Where(x => x.RecordType == RecordType.Added).Select(x => x.Feature.Attributes.Number).Distinct().ToArray();
            if (addedNumbers.Any())
            {
                if (changes.TryFindRoadSegmentChange(roadSegmentId, out var roadSegmentChange) && roadSegmentChange is AddRoadSegmentChange addRoadSegmentChange)
                {
                    changes = changes.ReplaceChange(addRoadSegmentChange, addRoadSegmentChange with
                    {
                        NationalRoadNumbers = addRoadSegmentChange.NationalRoadNumbers.Union(addedNumbers).ToArray()
                    });
                }
                else
                {
                    foreach (var number in addedNumbers)
                    {
                        changes = changes.AppendChange(
                            new AddRoadSegmentToNationalRoadChange
                            {
                                RoadSegmentId = roadSegmentId,
                                Number = number
                            }
                        );
                    }
                }
            }
        }

        return changes;
    }
}
