namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;

public class RoadSegmentLaneFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentLaneFeatureCompareAttributes>
{
    public RoadSegmentLaneFeatureCompareTranslator(RoadSegmentLaneFeatureCompareFeatureReader featureReader)
        : base(featureReader, ExtractFileName.AttRijstroken)
    {
    }

    protected override bool AttributesEquals(Feature<RoadSegmentLaneFeatureCompareAttributes> feature1, Feature<RoadSegmentLaneFeatureCompareAttributes> feature2)
    {
        return feature1.Attributes.FromPosition == feature2.Attributes.FromPosition
               && feature1.Attributes.ToPosition == feature2.Attributes.ToPosition
               && feature1.Attributes.Count == feature2.Attributes.Count
               && feature1.Attributes.Direction == feature2.Attributes.Direction;
    }
    
    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            var segmentId = record.Feature.Attributes.RoadSegmentId;
            var lane = new RoadSegmentLaneAttribute(
                record.Feature.Attributes.Id,
                record.Feature.Attributes.Count,
                record.Feature.Attributes.Direction,
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
                                    modifyRoadSegment.WithLane(lane));
                                break;
                            case RecordType.AddedIdentifier:
                            case RecordType.ModifiedIdentifier:
                                changes = changes.ReplaceChange(modifyRoadSegment,
                                    modifyRoadSegment.WithLane(lane));
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
                                changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithLane(lane));
                                break;
                            case ModifyRoadSegment modifyRoadSegment:
                                changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithLane(lane));
                                break;
                        }

                        break;
                }
            }
        }

        return changes;
    }
}
