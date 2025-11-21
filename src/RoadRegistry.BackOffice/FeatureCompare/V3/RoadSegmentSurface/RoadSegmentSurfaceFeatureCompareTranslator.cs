namespace RoadRegistry.BackOffice.FeatureCompare.V3.RoadSegmentSurface;

using System.Collections.Generic;
using System.Linq;
using RoadNetwork;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using TranslatedChanges = TranslatedChanges;

public class RoadSegmentSurfaceFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentSurfaceFeatureCompareAttributes>
{
    public RoadSegmentSurfaceFeatureCompareTranslator(RoadSegmentSurfaceFeatureCompareFeatureReader featureReader)
        : base(featureReader)
    {
    }

    protected override bool AttributesEquals(Feature<RoadSegmentSurfaceFeatureCompareAttributes> feature1, Feature<RoadSegmentSurfaceFeatureCompareAttributes> feature2)
    {
        return feature1.Attributes.FromPosition == feature2.Attributes.FromPosition
               && feature1.Attributes.ToPosition == feature2.Attributes.ToPosition
               && feature1.Attributes.Type == feature2.Attributes.Type;
    }

    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var grouping in records
                     .GroupBy(x => x.Feature.Attributes.RoadSegmentId))
        {
            var segmentId = grouping.Key;
            var attributes = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>(grouping
                .Select(x => x.Feature.Attributes)
                .Select(x => (x.FromPosition, x.ToPosition, RoadSegmentAttributeSide.Both, x.Type))
            );

            if (changes.TryFindRoadSegmentChange(segmentId, out var roadSegmentChange))
            {
                changes = UpdateRoadSegmentChange(changes, roadSegmentChange, attributes);
            }
            else
            {
                var modifyRoadSegmentChange = new ModifyRoadSegmentChange
                {
                    RoadSegmentId = segmentId
                };

                changes = changes.AppendChange(modifyRoadSegmentChange);
                changes = UpdateRoadSegmentChange(changes, modifyRoadSegmentChange, attributes);
            }
        }

        return changes;
    }

    private static TranslatedChanges UpdateRoadSegmentChange(TranslatedChanges changes, IRoadNetworkChange change, RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> attributes)
    {
        switch (change)
        {
            case AddRoadSegmentChange addRoadSegment:
                changes = changes.ReplaceChange(addRoadSegment, addRoadSegment with
                {
                    SurfaceType = attributes
                });
                break;
            case ModifyRoadSegmentChange modifyRoadSegment:
                changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment with
                {
                    SurfaceType = attributes
                });
                break;
        }

        return changes;
    }
}
