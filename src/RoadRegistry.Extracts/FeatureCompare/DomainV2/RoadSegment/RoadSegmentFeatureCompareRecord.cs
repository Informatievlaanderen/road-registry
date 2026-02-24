namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.Extracts.Uploads;

public class RoadSegmentFeatureCompareRecord
{
    public RoadSegmentFeatureCompareRecord(
        FeatureType featureType,
        RecordNumber recordNumber,
        RoadSegmentFeatureCompareWithDynamicAttributes attributes,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> flatFeatures,
        RoadSegmentId roadSegmentId,
        RecordType recordType
    )
    {
        FeatureType = featureType;
        RecordNumber = recordNumber;
        Attributes = attributes;
        RoadSegmentId = roadSegmentId;
        RecordType = recordType;
        FlatFeatures = flatFeatures;
    }

    public FeatureType FeatureType { get; }
    public RecordNumber RecordNumber { get; }
    public RoadSegmentFeatureCompareWithDynamicAttributes Attributes { get; }
    public RoadSegmentId RoadSegmentId { get; set; }
    public RecordType RecordType { get; }
    public IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> FlatFeatures { get; }

    public bool GeometryChanged { get; init; }

    public RoadSegmentId GetActualId() => RoadSegmentId;
    public RoadSegmentId GetOriginalId() => Attributes.RoadSegmentId;
}
