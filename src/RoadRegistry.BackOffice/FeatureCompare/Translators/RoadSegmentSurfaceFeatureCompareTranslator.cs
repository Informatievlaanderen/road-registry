namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Extracts;
using Uploads;

internal class RoadSegmentSurfaceFeatureCompareTranslator : RoadSegmentAttributeFeatureCompareTranslatorBase<RoadSegmentSurfaceFeatureCompareAttributes>
{
    public RoadSegmentSurfaceFeatureCompareTranslator(Encoding encoding)
        : base(encoding, ExtractFileName.AttWegverharding)
    {
    }

    protected override bool AttributesEquals(Feature<RoadSegmentSurfaceFeatureCompareAttributes> feature1, Feature<RoadSegmentSurfaceFeatureCompareAttributes> feature2)
    {
        return feature1.Attributes.FromPosition == feature2.Attributes.FromPosition
               && feature1.Attributes.ToPosition == feature2.Attributes.ToPosition
               && feature1.Attributes.Type == feature2.Attributes.Type;
    }

    protected override List<Feature<RoadSegmentSurfaceFeatureCompareAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName)
    {
        var featureReader = new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding);
        return featureReader.Read(entries, featureType, fileName);
    }

    protected override TranslatedChanges TranslateProcessedRecords(TranslatedChanges changes, List<Record> records)
    {
        foreach (var record in records)
        {
            var segmentId = new RoadSegmentId(record.Feature.Attributes.RoadSegmentId!.Value);
            var surface = new RoadSegmentSurfaceAttribute(
                new AttributeId(record.Feature.Attributes.Id!.Value),
                RoadSegmentSurfaceType.ByIdentifier[record.Feature.Attributes.Type],
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
                                    modifyRoadSegment.WithSurface(surface));
                                break;
                            case RecordType.AddedIdentifier:
                            case RecordType.ModifiedIdentifier:
                                changes = changes.ReplaceChange(modifyRoadSegment,
                                    modifyRoadSegment.WithSurface(surface));
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
                                changes = changes.ReplaceChange(addRoadSegment, addRoadSegment.WithSurface(surface));
                                break;
                            case ModifyRoadSegment modifyRoadSegment:
                                changes = changes.ReplaceChange(modifyRoadSegment, modifyRoadSegment.WithSurface(surface));
                                break;
                        }

                        break;
                }
            }
        }

        return changes;
    }
}
