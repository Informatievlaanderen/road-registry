namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class AddRoadSegmentToNumberedRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public RoadSegmentId? TemporarySegmentId { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

        public AddRoadSegmentToNumberedRoad(AttributeId attributeId,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            RoadSegmentId? temporarySegmentId,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            AttributeId = attributeId;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            TemporarySegmentId = temporarySegmentId;
            Number = number;
            Direction = direction;
            Ordinal = ordinal;
        }

        public Messages.AcceptedChange Accept(IReadOnlyCollection<Problem> problems)
        {
            return new Messages.AcceptedChange
            {
                RoadSegmentAddedToNumberedRoad = new Messages.RoadSegmentAddedToNumberedRoad
                {
                    AttributeId = AttributeId,
                    Ident8 = Number,
                    Direction = Direction,
                    Ordinal = Ordinal,
                    SegmentId = SegmentId,
                    TemporaryAttributeId = TemporaryAttributeId
                },
                Warnings = problems.OfType<Warning>().Select(warning => warning.Translate()).ToArray()
            };
        }

        public Messages.RejectedChange Reject(IReadOnlyCollection<Problem> problems)
        {
            return new Messages.RejectedChange
            {
                AddRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
                {
                    TemporaryAttributeId = TemporaryAttributeId,
                    Ident8 = Number,
                    Direction = Direction,
                    Ordinal = Ordinal,
                    SegmentId = SegmentId
                },
                Errors = problems.OfType<Error>().Select(error => error.Translate()).ToArray(),
                Warnings = problems.OfType<Warning>().Select(warning => warning.Translate()).ToArray()
            };
        }
    }
}
