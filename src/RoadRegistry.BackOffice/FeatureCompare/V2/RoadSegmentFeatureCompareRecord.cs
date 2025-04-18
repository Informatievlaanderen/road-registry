namespace RoadRegistry.BackOffice.FeatureCompare.V2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Models;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureCompare.V2.Translators;
using RoadRegistry.BackOffice.Uploads;

public class RoadSegmentFeatureCompareRecord
{
    public RoadSegmentFeatureCompareRecord(FeatureType featureType, RecordNumber recordNumber, RoadSegmentFeatureCompareAttributes attributes, RoadSegmentId id, RecordType recordType)
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
    public bool CategoryModified { get; init; }

    public RoadSegmentId GetActualId() => Id;
    public RoadSegmentId GetOriginalId() => Attributes.Id;
}
