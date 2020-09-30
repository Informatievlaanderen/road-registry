namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddRoadSegmentToEuropeanRoad : ITranslatedChange
    {
        public AddRoadSegmentToEuropeanRoad(
            RecordNumber recordNumber,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            EuropeanRoadNumber number)
        {
            RecordNumber = recordNumber;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            Number = number;
        }

        public RecordNumber RecordNumber { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public EuropeanRoadNumber Number { get; }


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
