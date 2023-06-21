namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using RoadRegistry.BackOffice.Extracts;
using Uploads;

internal class RoadSegmentWidthFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentWidthFeatureCompareAttributes>
{
    public RoadSegmentWidthFeatureCompareTranslator(Encoding encoding)
        : base(encoding, ExtractFileName.AttWegbreedte)
    {
    }

    protected override bool AttributesEquals(Feature<RoadSegmentWidthFeatureCompareAttributes> feature1, Feature<RoadSegmentWidthFeatureCompareAttributes> feature2)
    {
        return feature1.Attributes.FromPosition == feature2.Attributes.FromPosition
               && feature1.Attributes.ToPosition == feature2.Attributes.ToPosition
               && feature1.Attributes.Width == feature2.Attributes.Width;
    }

    protected override List<Feature<RoadSegmentWidthFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName)
    {
        var featureReader = new RoadSegmentWidthFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
    }

    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            var segmentId = new RoadSegmentId(record.Feature.Attributes.RoadSegmentId!.Value);
            var width = new RoadSegmentWidthAttribute(
                new AttributeId(record.Feature.Attributes.Id!.Value),
                new RoadSegmentWidth(record.Feature.Attributes.Width),
                RoadSegmentPosition.FromDouble(record.Feature.Attributes.FromPosition!.Value),
                RoadSegmentPosition.FromDouble(record.Feature.Attributes.ToPosition!.Value)
            );
            if (changes.TryFindRoadSegmentProvisionalChange(segmentId, out var provisionalChange))
            {
                switch (provisionalChange)
                {
                    case ModifyRoadSegment modifyRoadSegment:
                        switch (record.RecordType.Translation.Identifier)
                        {
                            case RecordType.IdenticalIdentifier:
                                changes = changes.ReplaceProvisionalChange(modifyRoadSegment,
                                    modifyRoadSegment.WithWidth(width));
                                break;
                            case RecordType.AddedIdentifier:
                            case RecordType.ModifiedIdentifier:
                                changes = changes.ReplaceChange(modifyRoadSegment,
                                    modifyRoadSegment.WithWidth(width));
                                break;
                            case RecordType.RemovedIdentifier:
                                changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment);
                                break;
                        }

                        break;
                }
            }
            else if (changes.TryFindRoadSegmentChange(segmentId, out var change))
            {
                switch (record.RecordType.Translation.Identifier)
                {
                    case RecordType.IdenticalIdentifier:
                    case RecordType.AddedIdentifier:
                        switch (change)
                        {
                            case AddRoadSegment addRoadSegment:
                                changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithWidth(width));
                                break;
                            case ModifyRoadSegment modifyRoadSegment:
                                changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithWidth(width));
                                break;
                        }

                        break;
                }
            }
        }

        return changes;
    }
}
