namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.Extracts.Uploads;

public class RoadSegmentFeatureCompareRecord : IFeatureCompareRecord
{
    public RoadSegmentFeatureCompareRecord(
        FeatureType featureType,
        RecordNumber recordNumber,
        RoadSegmentFeatureCompareWithDynamicAttributes attributes,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> flatFeatures,
        RoadSegmentId roadSegmentId,
        RecordType recordType,
        RoadSegmentFeatureCompareWithDynamicAttributes? deliveredAttributes = null
    )
    {
        FeatureType = featureType;
        RecordNumber = recordNumber;
        Attributes = attributes;
        DeliveredAttributes = deliveredAttributes ?? attributes;
        RoadSegmentId = roadSegmentId;
        RecordType = recordType;
        FlatFeatures = flatFeatures;
    }

    public FeatureType FeatureType { get; }
    public RecordNumber RecordNumber { get; }
    public RoadSegmentFeatureCompareWithDynamicAttributes Attributes { get; private set; }

    // The attributes as delivered. For a road segment that keeps its extract id, Attributes only holds what changed
    // compared to the extract; these are kept so the road segment can still be added in full should it lose that id.
    public RoadSegmentFeatureCompareWithDynamicAttributes DeliveredAttributes { get; }
    public RoadSegmentId RoadSegmentId { get; set; }
    public RecordType RecordType { get; set; }
    public IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> FlatFeatures { get; }

    public bool GeometryChanged { get; set; }

    // Another delivered road segment claimed the same extract id and got it: this one becomes a new road segment, and a
    // new road segment needs all of its attributes, not only how it differs from the extract road segment.
    public void BecomeAdded(RoadSegmentId newRoadSegmentId)
    {
        RoadSegmentId = newRoadSegmentId;
        RecordType = RecordType.Added;
        Attributes = DeliveredAttributes;
        GeometryChanged = false;
    }

    public RoadSegmentId GetActualId() => RoadSegmentId;
    public RoadSegmentId GetOriginalId() => Attributes.RoadSegmentId;
}
