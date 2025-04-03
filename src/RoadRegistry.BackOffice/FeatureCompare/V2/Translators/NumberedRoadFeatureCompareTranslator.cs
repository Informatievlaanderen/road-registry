namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using System.Linq;
using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;

public class NumberedRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NumberedRoadFeatureCompareAttributes>
{
    public NumberedRoadFeatureCompareTranslator(NumberedRoadFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttGenumweg)
    {
    }

    protected override void HandleIdenticalRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, List<Feature<NumberedRoadFeatureCompareAttributes>> changeFeatures, List<Feature<NumberedRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
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
                var unProcessedExtractFeatures = leveringExtractFeatures
                    .Where(x => processedRecords.All(processedRecord => processedRecord.Feature.Attributes.Id != x.Attributes.Id))
                    .ToList();

                for (var i = 0; i < unProcessedExtractFeatures.Count; i++)
                {
                    var extractFeature = unProcessedExtractFeatures[i];
                    if (i == 0)
                    {
                        if (changeFeature.Attributes.Ordinal != extractFeature.Attributes.Ordinal ||
                            changeFeature.Attributes.Direction != extractFeature.Attributes.Direction)
                        {
                            processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                            processedRecords.Add(new Record(changeFeature, RecordType.Added));
                        }
                        else
                        {
                            processedRecords.Add(new Record(changeFeature, RecordType.Identical));
                        }
                    }
                    else
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

    protected override void HandleModifiedRoadSegment(RoadSegmentFeatureCompareRecord wegsegment, List<Feature<NumberedRoadFeatureCompareAttributes>> changeFeatures, List<Feature<NumberedRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId());

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId() && x.Attributes.Number == changeFeature.Attributes.Number);
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
                var unProcessedExtractFeatures = leveringExtractFeatures
                    .Where(x => processedRecords.All(processedRecord => processedRecord.Feature.Attributes.Id != x.Attributes.Id))
                    .ToList();

                for (var i = 0; i < unProcessedExtractFeatures.Count; i++)
                {
                    var extractFeature = unProcessedExtractFeatures[i];

                    if (i == 0)
                    {
                        if (changeFeature.Attributes.Ordinal != extractFeature.Attributes.Ordinal ||
                            changeFeature.Attributes.Direction != extractFeature.Attributes.Direction)
                        {
                            processedRecords.Add(new Record(extractFeature, RecordType.Removed));
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
                            processedRecords.Add(new Record(extractFeature, RecordType.Identical));
                        }
                    }
                    else
                    {
                        processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                    }
                }
            }
        }

        foreach (var extractFeature in wegsegmentExtractFeatures)
        {
            var extractChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.GetOriginalId()
                                                                        && x.Attributes.Number == extractFeature.Attributes.Number);
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
                        new AddRoadSegmentToNumberedRoad(
                            record.Feature.RecordNumber,
                            record.Feature.Attributes.Id,
                            segment.Attributes.Method,
                            record.Feature.Attributes.RoadSegmentId,
                            record.Feature.Attributes.Number,
                            record.Feature.Attributes.Direction,
                            record.Feature.Attributes.Ordinal
                        )
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegmentFromNumberedRoad(
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
