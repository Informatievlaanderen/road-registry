namespace RoadRegistry.BackOffice.FeatureCompare;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Translators;
using Uploads;

public class RoadSegmentFeatureCompareRecord
{
    public RoadSegmentFeatureCompareRecord(RecordNumber recordNumber, RoadSegmentFeatureCompareAttributes attributes, RoadSegmentId id, RecordType recordType)
    {
        RecordNumber = recordNumber;
        Attributes = attributes;
        Id = id;
        RecordType = recordType;
    }

    public RecordNumber RecordNumber { get; }
    public RoadSegmentFeatureCompareAttributes Attributes { get; set; }
    public RoadSegmentId Id { get; set; }
    public RecordType RecordType { get; }

    public bool GeometryChanged { get; init; }
    public FeatureType? FeatureType { get; init; }

    public RoadSegmentId GetActualId() => Id;
    public RoadSegmentId GetOriginalId() => Attributes.Id;
}
