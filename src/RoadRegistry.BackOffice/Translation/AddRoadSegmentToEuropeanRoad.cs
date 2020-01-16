namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;

    public class AddRoadSegmentToEuropeanRoad : ITranslatedChange
    {
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public EuropeanRoadNumber Number { get; }

        public AddRoadSegmentToEuropeanRoad(
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            EuropeanRoadNumber number)
        {
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            Number = number;
        }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
            {
                TemporaryAttributeId = TemporaryAttributeId,
                Number = Number,
                SegmentId = SegmentId
            };
        }
    }
}
