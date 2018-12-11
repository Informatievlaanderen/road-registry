namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class AddRoadSegmentToEuropeanRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public RoadSegmentId? TemporarySegmentId { get; }
        public EuropeanRoadNumber Number { get; }

        public AddRoadSegmentToEuropeanRoad(AttributeId attributeId,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            RoadSegmentId? temporarySegmentId,
            EuropeanRoadNumber number)
        {
            AttributeId = attributeId;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            TemporarySegmentId = temporarySegmentId;
            Number = number;
        }

        public Messages.AcceptedChange Accept(IReadOnlyCollection<Problem> problems)
        {
            return new Messages.AcceptedChange
            {
                RoadSegmentAddedToEuropeanRoad = new Messages.RoadSegmentAddedToEuropeanRoad
                {
                    AttributeId = AttributeId,
                    RoadNumber = Number,
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
                AddRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
                {
                    TemporaryAttributeId = TemporaryAttributeId,
                    RoadNumber = Number,
                    SegmentId = SegmentId
                },
                Errors = problems.OfType<Error>().Select(error => error.Translate()).ToArray(),
                Warnings = problems.OfType<Warning>().Select(warning => warning.Translate()).ToArray()
            };
        }
    }
}
