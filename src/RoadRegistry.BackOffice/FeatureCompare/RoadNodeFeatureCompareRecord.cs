namespace RoadRegistry.BackOffice.FeatureCompare;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using Translators;
using Uploads;

public class RoadNodeFeatureCompareRecord
{
    public RoadNodeFeatureCompareRecord(FeatureType featureType, RecordNumber recordNumber, RoadNodeFeatureCompareAttributes attributes, RoadNodeId id, RecordType recordType)
    {
        FeatureType = featureType;
        RecordNumber = recordNumber;
        Attributes = attributes;
        Id = id;
        RecordType = recordType;
    }

    public FeatureType FeatureType { get; }
    public RecordNumber RecordNumber { get; }
    public RoadNodeFeatureCompareAttributes Attributes { get; }
    public RoadNodeId Id { get; set; }
    public RecordType RecordType { get; }

    public RoadNodeId GetActualId() => Id;
    public RoadNodeId GetOriginalId() => Attributes.Id;
}
