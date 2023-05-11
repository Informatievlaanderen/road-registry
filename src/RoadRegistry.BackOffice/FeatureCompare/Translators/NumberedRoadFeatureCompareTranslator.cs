namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Uploads;

internal class NumberedRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NumberedRoadFeatureCompareAttributes>
{
    public NumberedRoadFeatureCompareTranslator(Encoding encoding)
        : base(encoding, "ATTGENUMWEG")
    {
    }

    protected override void HandleIdenticalRoadSegment(RoadSegmentRecord wegsegment, List<Feature<NumberedRoadFeatureCompareAttributes>> changeFeatures, List<Feature<NumberedRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn);
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);

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

    protected override void HandleModifiedRoadSegment(RoadSegmentRecord wegsegment, List<Feature<NumberedRoadFeatureCompareAttributes>> changeFeatures, List<Feature<NumberedRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn);
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);

        foreach (var changeFeature in wegsegmentChangeFeatures)
        {
            var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id && x.Attributes.Number == changeFeature.Attributes.Number);
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(changeFeature, RecordType.Added, wegsegment.Id));
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
                            processedRecords.Add(new Record(changeFeature, RecordType.Added, wegsegment.Id));
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
            var extractChangeFeatures = changeFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn
                                                                        && x.Attributes.Number == extractFeature.Attributes.Number);
            if (!extractChangeFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
            }
        }
    }

    protected override List<Feature<NumberedRoadFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName)
    {
        var featureReader = new NumberedRoadFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
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
                            new AttributeId(record.Feature.Attributes.Id),
                            new RoadSegmentId(record.Feature.Attributes.RoadSegmentId),
                            NumberedRoadNumber.Parse(record.Feature.Attributes.Number),
                            RoadSegmentNumberedRoadDirection.ByIdentifier[record.Feature.Attributes.Direction],
                            new RoadSegmentNumberedRoadOrdinal(record.Feature.Attributes.Ordinal)
                        )
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegmentFromNumberedRoad(
                            record.Feature.RecordNumber,
                            new AttributeId(record.Feature.Attributes.Id),
                            new RoadSegmentId(record.Feature.Attributes.RoadSegmentId),
                            NumberedRoadNumber.Parse(record.Feature.Attributes.Number)
                        )
                    );
                    break;
            }
        }

        return changes;
    }
}
