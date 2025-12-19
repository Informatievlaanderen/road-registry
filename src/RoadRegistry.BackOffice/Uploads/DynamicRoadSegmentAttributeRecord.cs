namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.Extracts.Uploads;

internal class DynamicRoadSegmentAttributeRecord
{
    public DynamicRoadSegmentAttributeRecord(AttributeId identifier, RecordType recordType, RecordNumber recordNumber)
    {
        Identifier = identifier;
        RecordType = recordType;
        Number = recordNumber;
    }

    public AttributeId Identifier { get; }
    public RecordNumber Number { get; }
    public RecordType RecordType { get; }
}
