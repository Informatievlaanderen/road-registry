namespace RoadRegistry.BackOffice.Uploads
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    internal class DynamicRoadSegmentAttributeRecord
    {
        public DynamicRoadSegmentAttributeRecord(AttributeId identifier, RecordType recordType, RecordNumber recordNumber)
        {
            Identifier = identifier;
            RecordType = recordType;
            Number = recordNumber;
        }

        public AttributeId Identifier { get; }
        public RecordType RecordType { get; }
        public RecordNumber Number { get; }
    }
}
