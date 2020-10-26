namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RemoveRoadSegmentFromNumberedRoad : ITranslatedChange
    {
        public RemoveRoadSegmentFromNumberedRoad(
            RecordNumber recordNumber,
            AttributeId attributeId,
            RoadSegmentId segmentId,
            NumberedRoadNumber number)
        {
            RecordNumber = recordNumber;
            AttributeId = attributeId;
            SegmentId = segmentId;
            Number = number;
        }

        public RecordNumber RecordNumber { get; }
        public AttributeId AttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NumberedRoadNumber Number { get; }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegmentFromNumberedRoad = new Messages.RemoveRoadSegmentFromNumberedRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId
            };
        }
    }
}
