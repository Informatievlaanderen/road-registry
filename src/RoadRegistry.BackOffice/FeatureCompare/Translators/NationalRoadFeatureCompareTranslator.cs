namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Uploads;

internal class NationalRoadFeatureCompareTranslator : RoadNumberingFeatureCompareTranslatorBase<NationalRoadFeatureCompareAttributes>
{
    public NationalRoadFeatureCompareTranslator(Encoding encoding)
        : base(encoding, "ATTNATIONWEG")
    {
    }

    protected override void HandleIdenticalRoadSegment(RoadSegmentRecord wegsegment, List<Feature<NationalRoadFeatureCompareAttributes>> leveringFeatures, List<Feature<NationalRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
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
                processedRecords.Add(new Record(leveringFeature, RecordType.Identical));

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
            var extractLeveringFeature = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId == extractFeature.Attributes.RoadSegmentId
                                                                       && x.Attributes.Number == extractFeature.Attributes.Number);
            if (!extractLeveringFeature.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
            }
        }
    }

    protected override void HandleModifiedRoadSegment(RoadSegmentRecord wegsegment, List<Feature<NationalRoadFeatureCompareAttributes>> leveringFeatures, List<Feature<NationalRoadFeatureCompareAttributes>> extractFeatures, List<Record> processedRecords)
    {
        var wegsegmentLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId.ToString() == wegsegment.CompareIdn);
        var wegsegmentExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id);

        foreach (var leveringFeature in wegsegmentLeveringFeatures)
        {
            var leveringExtractFeatures = extractFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id
                                                                       && x.Attributes.Number == leveringFeature.Attributes.Number);
            if (!leveringExtractFeatures.Any())
            {
                processedRecords.Add(new Record(leveringFeature, RecordType.Added, wegsegment.Id));
            }
            else
            {
                processedRecords.Add(new Record(leveringExtractFeatures.First(), RecordType.Identical));
            }
        }

        foreach (var extractFeature in wegsegmentExtractFeatures)
        {
            var extractLeveringFeatures = leveringFeatures.FindAll(x => x.Attributes.RoadSegmentId == wegsegment.Id
                                                                        && x.Attributes.Number == extractFeature.Attributes.Number);
            if (!extractLeveringFeatures.Any())
            {
                processedRecords.Add(new Record(extractFeature, RecordType.Removed));
            }
        }
    }

    protected override List<Feature<NationalRoadFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName)
    {
        var featureReader = new NationalRoadFeatureCompareFeatureReader(Encoding);
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
                        new AddRoadSegmentToNationalRoad(
                            record.Feature.RecordNumber,
                            new AttributeId(record.Feature.Attributes.Id),
                            new RoadSegmentId(record.Feature.Attributes.RoadSegmentId),
                            NationalRoadNumber.Parse(record.Feature.Attributes.Number)
                        )
                    );
                    break;
                case RecordType.RemovedIdentifier:
                    changes = changes.AppendChange(
                        new RemoveRoadSegmentFromNationalRoad(
                            record.Feature.RecordNumber,
                            new AttributeId(record.Feature.Attributes.Id),
                            new RoadSegmentId(record.Feature.Attributes.RoadSegmentId),
                            NationalRoadNumber.Parse(record.Feature.Attributes.Number)
                        )
                    );
                    break;
            }
        }

        return changes;
    }
}
