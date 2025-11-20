namespace RoadRegistry.BackOffice.FeatureCompare.V3.RoadSegment;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.RoadSegment.ValueObjects;

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
    public bool ConvertedFromOutlined { get; init; }

    public RoadSegmentId GetActualId() => Id;
    public RoadSegmentId GetOriginalId() => Attributes.Id;
}
