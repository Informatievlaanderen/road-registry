namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class AddRoadSegmentToNationalRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public RoadSegmentId? TemporarySegmentId { get; }
        public NationalRoadNumber Number { get; }

        public AddRoadSegmentToNationalRoad(AttributeId attributeId,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            RoadSegmentId? temporarySegmentId,
            NationalRoadNumber number)
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
                RoadSegmentAddedToNationalRoad = new Messages.RoadSegmentAddedToNationalRoad
                {
                    AttributeId = AttributeId,
                    Ident2 = Number,
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
                AddRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
                {
                    TemporaryAttributeId = TemporaryAttributeId,
                    Ident2 = Number,
                    SegmentId = SegmentId
                },
                Errors = problems.OfType<Error>().Select(error => error.Translate()).ToArray(),
                Warnings = problems.OfType<Warning>().Select(warning => warning.Translate()).ToArray()
            };
        }
    }
}
