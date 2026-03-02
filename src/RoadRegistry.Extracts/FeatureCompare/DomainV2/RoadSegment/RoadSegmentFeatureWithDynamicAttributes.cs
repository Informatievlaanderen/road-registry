namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentFeatureWithDynamicAttributes
{
    public RoadSegmentFeatureWithDynamicAttributes(
        RecordNumber recordNumber,
        RoadSegmentFeatureCompareWithDynamicAttributes attributes,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> flatFeatures
    )
    {
        RecordNumber = recordNumber;
        Attributes = attributes;
        FlatFeatures = flatFeatures;
    }

    public RecordNumber RecordNumber { get; }
    public RoadSegmentFeatureCompareWithDynamicAttributes Attributes { get; }
    public IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> FlatFeatures { get; }
}
