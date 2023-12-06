namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Extracts;
using System.Collections.Generic;
using System.Linq;
using Uploads;

public class EuropeanRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<EuropeanRoadFeatureCompareAttributes>
{
    public EuropeanRoadFeatureCompareTranslator(EuropeanRoadFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttEuropweg)
    {
    }

    protected override void HandleIdenticalRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, List<Feature<EuropeanRoadFeatureCompareAttributes>> changeFeatures, List<Feature<EuropeanRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == changeFeature.Attributes.RoadSegmentId
                                                                       && x.Attributes.Number == changeFeature.Attributes.Number);
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
            var extractChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == extractFeature.Attributes.RoadSegmentId
                                                                        && x.Attributes.Number == extractFeature.Attributes.Number);
            if (!extractChangeFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
            }
        }
    }

    protected override void HandleModifiedRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, List<Feature<EuropeanRoadFeatureCompareAttributes>> changeFeatures, List<Feature<EuropeanRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId()
                                                                       && x.Attributes.Number == changeFeature.Attributes.Number);
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
            {
                var extractChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId()
                                                                            && x.Attributes.Number == extractFeature.Attributes.Number);

                if (!extractChangeFeatures.Any())
                {
                    processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                }
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
                    changes = changes.AppendChange(
                        new AddRoadSegmentToEuropeanRoad(
                            record.Feature.RecordNumber,
                            record.Feature.Attributes.Id,
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
