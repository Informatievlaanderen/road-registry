namespace RoadRegistry.BackOffice.FeatureCompare.V1.Translators;

using System.Collections.Generic;
using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;

public class RoadSegmentWidthFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentWidthFeatureCompareAttributes>
{
    public RoadSegmentWidthFeatureCompareTranslator(RoadSegmentWidthFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttWegbreedte)
    {
    }

    protected override bool AttributesEquals(Feature<RoadSegmentWidthFeatureCompareAttributes> feature1, Feature<RoadSegmentWidthFeatureCompareAttributes> feature2)
    {
        return feature1.Attributes.FromPosition == feature2.Attributes.FromPosition
               && feature1.Attributes.ToPosition == feature2.Attributes.ToPosition
               && feature1.Attributes.Width == feature2.Attributes.Width;
    }
    
    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            var segmentId = record.Feature.Attributes.RoadSegmentId;
            var width = new RoadSegmentWidthAttribute(
                record.Feature.Attributes.Id,
                record.Feature.Attributes.Width,
                record.Feature.Attributes.FromPosition,
                record.Feature.Attributes.ToPosition
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
