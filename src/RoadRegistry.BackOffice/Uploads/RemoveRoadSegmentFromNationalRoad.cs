namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RemoveRoadSegmentFromNationalRoad : ITranslatedChange
    {
        public RemoveRoadSegmentFromNationalRoad(
            RecordNumber recordNumber,
            AttributeId attributeId,
            RoadSegmentId segmentId,
            NationalRoadNumber number)
        {
            RecordNumber = recordNumber;
            AttributeId = attributeId;
            SegmentId = segmentId;
            Number = number;
        }

        public RecordNumber RecordNumber { get; }
        public AttributeId AttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NationalRoadNumber Number { get; }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegmentFromNationalRoad = new Messages.RemoveRoadSegmentFromNationalRoad
            {
                AttributeId = AttributeId,
                Ident2 = Number,
                SegmentId = SegmentId
            };
        }
    }
}
