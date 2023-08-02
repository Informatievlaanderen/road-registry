namespace RoadRegistry.BackOffice.FeatureCompare;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Translators;
using Uploads;

public class RoadNodeFeatureCompareRecord
{
    public RoadNodeFeatureCompareRecord(RecordNumber recordNumber, RoadNodeFeatureCompareAttributes attributes, RoadNodeId id, RecordType recordType)
    {
        RecordNumber = recordNumber;
        Attributes = attributes;
        Id = id;
        RecordType = recordType;
    }
    
    public RecordNumber RecordNumber { get; }
    public RoadNodeFeatureCompareAttributes Attributes { get; }
    public RoadNodeId Id { get; }
    public RecordType RecordType { get; }
}
