namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;

    public class AddRoadSegmentToNumberedRoad : ITranslatedChange
    {
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

        public AddRoadSegmentToNumberedRoad(
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            Number = number;
            Direction = direction;
            Ordinal = ordinal;
        }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
            {
                TemporaryAttributeId = TemporaryAttributeId,
                Ident8 = Number,
                Direction = Direction,
                Ordinal = Ordinal,
                SegmentId = SegmentId
            };
        }
    }
}
