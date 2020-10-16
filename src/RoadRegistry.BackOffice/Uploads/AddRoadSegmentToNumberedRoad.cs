namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddRoadSegmentToNumberedRoad : ITranslatedChange
    {
        public AddRoadSegmentToNumberedRoad(
            RecordNumber recordNumber,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            RecordNumber = recordNumber;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            Number = number;
            Direction = direction;
            Ordinal = ordinal;
        }

        public RecordNumber RecordNumber { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
            {
                TemporaryAttributeId = TemporaryAttributeId,
                Number = Number,
                Direction = Direction,
                Ordinal = Ordinal,
                SegmentId = SegmentId
            };
        }
    }
}
