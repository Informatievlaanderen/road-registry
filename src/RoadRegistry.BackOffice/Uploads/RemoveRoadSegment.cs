namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RemoveRoadSegment : ITranslatedChange
    {
        public RemoveRoadSegment(RecordNumber recordNumber, RoadSegmentId id)
        {
            RecordNumber = recordNumber;
            Id = id;
        }

        public RecordNumber RecordNumber { get; }
        public RoadSegmentId Id { get; }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegment = new Messages.RemoveRoadSegment
            {
                Id = Id
            };
        }
    }
}
