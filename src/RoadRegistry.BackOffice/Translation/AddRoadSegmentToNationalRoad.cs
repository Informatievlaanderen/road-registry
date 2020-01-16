namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;

    public class AddRoadSegmentToNationalRoad : ITranslatedChange
    {
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NationalRoadNumber Number { get; }

        public AddRoadSegmentToNationalRoad(
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            NationalRoadNumber number)
        {
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            Number = number;
        }
        
        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
            {
                TemporaryAttributeId = TemporaryAttributeId,
                Ident2 = Number,
                SegmentId = SegmentId
            };
        }
    }
}
