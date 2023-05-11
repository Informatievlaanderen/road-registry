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

    protected override void HandleIdenticalRoadSegment(RoadSegmentRecord wegsegment, List<Feature<NumberedRoadFeatureCompareAttributes>> leveringFeatures, List<Feature<NumberedRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn);
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);

        foreach (var leveringFeature in wegsegmentLeveringFeatures)
        {
            var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == leveringFeature.Attributes.RoadSegmentId
                                                                       && x.Attributes.Number == leveringFeature.Attributes.Number);
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(leveringFeature, RecordType.Added));
            }
            else
            {
                for (var i = 0; i < leveringExtractFeatures.Count; i++)
                {
                    var extractFeature = leveringExtractFeatures[i];

                    if (i == 0)
                    {
                        if (leveringFeature.Attributes.Ordinal != extractFeature.Attributes.Ordinal ||
                            leveringFeature.Attributes.Direction != extractFeature.Attributes.Direction)
                        {
                            processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                            processedRecords.Add(new Record(leveringFeature, RecordType.Added));
                        }
                        else
                        {
                            processedRecords.Add(new Record(leveringFeature, RecordType.Identical));
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
            var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId == extractFeature.Attributes.RoadSegmentId
                                                                        && x.Attributes.Number == extractFeature.Attributes.Number);
            if (!extractLeveringFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
            }
        }
    }

    protected override void HandleModifiedRoadSegment(RoadSegmentRecord wegsegment, List<Feature<NumberedRoadFeatureCompareAttributes>> leveringFeatures, List<Feature<NumberedRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn);
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);

        foreach (var leveringFeature in wegsegmentLeveringFeatures)
        {
            var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id && x.Attributes.Number == leveringFeature.Attributes.Number);
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(leveringFeature, RecordType.Added, wegsegment.Id));
            }
            else
            {
                for (var i = 0; i < leveringExtractFeatures.Count; i++)
                {
                    var extractFeature = leveringExtractFeatures[i];

                    if (i == 0)
                    {
                        if (leveringFeature.Attributes.Ordinal != extractFeature.Attributes.Ordinal ||
                            leveringFeature.Attributes.Direction != extractFeature.Attributes.Direction)
                        {
                            processedRecords.Add(new Record(extractFeature, RecordType.Removed));
                            processedRecords.Add(new Record(leveringFeature, RecordType.Added, wegsegment.Id));
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
            var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn
                                                                        && x.Attributes.Number == extractFeature.Attributes.Number);
            if (!extractLeveringFeatures.Any())
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
