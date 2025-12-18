namespace RoadRegistry.Extracts.FeatureCompare.V3.RoadSegment;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Uploads;

public class RoadSegmentFeatureCompareRecord
{
    public RoadSegmentFeatureCompareRecord(
        FeatureType featureType,
        RecordNumber recordNumber,
        RoadSegmentFeatureCompareAttributes attributes,
        RoadSegmentId id,
        RecordType recordType
    )
    {
        FeatureType = featureType;
        RecordNumber = recordNumber;
        Attributes = attributes;
        Id = id;
        RecordType = recordType;
    }

    public FeatureType FeatureType { get; }
    public RecordNumber RecordNumber { get; }
    public RoadSegmentFeatureCompareAttributes Attributes { get; set; }
    public RoadSegmentId Id { get; set; }
    public RecordType RecordType { get; }

    public bool GeometryChanged { get; init; }

    public RoadSegmentId GetActualId() => Id;
    public RoadSegmentId GetOriginalId() => Attributes.Id;
}
