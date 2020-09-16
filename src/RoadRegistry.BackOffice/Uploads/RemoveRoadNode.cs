namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RemoveRoadNode : ITranslatedChange
    {
        public RemoveRoadNode(RecordNumber recordNumber, RoadNodeId id)
        {
            RecordNumber = recordNumber;
            Id = id;
        }

        public RecordNumber RecordNumber { get; }
        public RoadNodeId Id { get; }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadNode = new Messages.RemoveRoadNode
            {
                Id = Id
            };
        }
    }
}
