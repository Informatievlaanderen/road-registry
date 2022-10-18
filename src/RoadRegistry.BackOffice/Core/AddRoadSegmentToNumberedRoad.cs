namespace RoadRegistry.BackOffice.Core;

using System;
using Messages;

public class AddRoadSegmentToNumberedRoad : IRequestedChange
{
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

    public AttributeId AttributeId { get; }
    public RoadSegmentNumberedRoadDirection Direction { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentId? TemporarySegmentId { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            Direction = Direction,
            Ordinal = Ordinal,
            SegmentId = SegmentId,
            TemporaryAttributeId = TemporaryAttributeId
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
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

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.AfterView.View.Segments.ContainsKey(SegmentId)) problems = problems.Add(new RoadSegmentMissing(TemporarySegmentId ?? SegmentId));

        return problems;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return Problems.None;
    }
}
