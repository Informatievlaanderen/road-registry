namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Extracts;
using Uploads;

internal class NumberedRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NumberedRoadFeatureCompareAttributes>
{
    public NumberedRoadFeatureCompareTranslator(Encoding encoding)
        : base(encoding, ExtractFileName.AttGenumweg)
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
                for (var i = 0; i < leveringExtractFeatures.Count; i++)
                {
                    var extractFeature = leveringExtractFeatures[i];

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
                for (var i = 0; i < leveringExtractFeatures.Count; i++)
                {
                    var extractFeature = leveringExtractFeatures[i];

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

    protected override (List<Feature<NumberedRoadFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var featureReader = new NumberedRoadFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(archive, featureType, fileName, context);
    }

    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            switch (record.RecordType.Translation.Identifier)
            {
                case RecordType.AddedIdentifier:
                    changes = changes.AppendChange(
                        new AddRoadSegmentToNumberedRoad(
                            record.Feature.RecordNumber,
                            record.Feature.Attributes.Id,
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
