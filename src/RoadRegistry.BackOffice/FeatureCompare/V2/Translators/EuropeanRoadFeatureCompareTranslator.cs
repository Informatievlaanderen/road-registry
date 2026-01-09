namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using System.Linq;
using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Uploads;

public class EuropeanRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadFeatureCompareTranslator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttEuropweg)
    {
    }

    protected override void HandleIdenticalRoadSegment(
        RoadSegmentFeatureCompareRecord wegsegment,
        ILookup<RoadSegmentId, Feature<EuropeanRoadFeatureCompareAttributes>> changeFeatures,
        ILookup<RoadSegmentId, Feature<EuropeanRoadFeatureCompareAttributes>> extractFeatures,
        List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures[wegsegment.GetOriginalId()].ToList();
        var wegsegmentExtractFeatures = extractFeatures[wegsegment.GetOriginalId()].ToList();

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures[changeFeature.Attributes.RoadSegmentId]
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
            var extractChangeFeatures = changeFeatures[extractFeature.Attributes.RoadSegmentId]
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
        ILookup<RoadSegmentId, Feature<EuropeanRoadFeatureCompareAttributes>> changeFeatures,
        ILookup<RoadSegmentId, Feature<EuropeanRoadFeatureCompareAttributes>> extractFeatures,
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
                        RoadSegmentId = wegsegment.GetActualId()
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
            var segment = context.FindRoadSegment(record.Feature.Attributes.RoadSegmentId);

            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadSegmentToEuropeanRoad(
                            record.Feature.RecordNumber,
                            record.Feature.Attributes.Id,
                            segment.Attributes.Method,
                            record.Feature.Attributes.RoadSegmentId,
                            record.Feature.Attributes.Number
                        )
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegmentFromEuropeanRoad(
                            record.Feature.RecordNumber,
                            record.Feature.Attributes.Id,
                            segment.Attributes.Method,
                            record.Feature.Attributes.RoadSegmentId,
                            record.Feature.Attributes.Number
                        )
                    );
                    break;
            }
        }

        return changes;
    }
}
