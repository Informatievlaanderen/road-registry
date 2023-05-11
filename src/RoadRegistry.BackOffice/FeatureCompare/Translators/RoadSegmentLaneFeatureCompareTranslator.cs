namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Uploads;

internal class RoadSegmentLaneFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentLaneFeatureCompareAttributes>
{
    public RoadSegmentLaneFeatureCompareTranslator(Encoding encoding)
        : base(encoding, "ATTRIJSTROKEN")
    {
    }

    protected override bool AttributesEquals(Feature<RoadSegmentLaneFeatureCompareAttributes> feature1, Feature<RoadSegmentLaneFeatureCompareAttributes> feature2)
    {
        return feature1.Attributes.FromPosition == feature2.Attributes.FromPosition
               && feature1.Attributes.ToPosition == feature2.Attributes.ToPosition
               && feature1.Attributes.Count == feature2.Attributes.Count
               && feature1.Attributes.Direction == feature2.Attributes.Direction;
    }

    protected override List<Feature<RoadSegmentLaneFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName)
    {
        var featureReader = new RoadSegmentLaneFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
    }

    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            var segmentId = new RoadSegmentId(record.Feature.Attributes.RoadSegmentId);
            var lane = new RoadSegmentLaneAttribute(
                new AttributeId(record.Feature.Attributes.Id),
                new RoadSegmentLaneCount(record.Feature.Attributes.Count),
                RoadSegmentLaneDirection.ByIdentifier[record.Feature.Attributes.Direction],
                RoadSegmentPosition.FromDouble(record.Feature.Attributes.FromPosition),
                RoadSegmentPosition.FromDouble(record.Feature.Attributes.ToPosition)
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
