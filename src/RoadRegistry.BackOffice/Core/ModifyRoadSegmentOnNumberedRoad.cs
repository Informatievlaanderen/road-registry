namespace RoadRegistry.BackOffice.Core;

using System;
using Messages;

public class ModifyRoadSegmentOnNumberedRoad : IRequestedChange
{
    public ModifyRoadSegmentOnNumberedRoad(
        AttributeId attributeId,
        RoadSegmentId segmentId,
        NumberedRoadNumber number,
        RoadSegmentNumberedRoadDirection direction,
        RoadSegmentNumberedRoadOrdinal ordinal)
    {
        AttributeId = attributeId;
        SegmentId = segmentId;
        Number = number;
        Direction = direction;
        Ordinal = ordinal;
    }

    public AttributeId AttributeId { get; }
    public RoadSegmentNumberedRoadDirection Direction { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; }
    public RoadSegmentId SegmentId { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentOnNumberedRoadModified = new RoadSegmentOnNumberedRoadModified
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentId = SegmentId,
            Direction = Direction,
            Ordinal = Ordinal
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.ModifyRoadSegmentOnNumberedRoad = new Messages.ModifyRoadSegmentOnNumberedRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentId = SegmentId,
            Direction = Direction,
            Ordinal = Ordinal
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        var segment = context.AfterView.View.Segments[SegmentId];
        if (!segment.PartOfNumberedRoads.Contains(Number)) problems = problems.Add(new NumberedRoadNumberNotFound(Number));

        return problems;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.BeforeView.View.Segments.ContainsKey(SegmentId)) problems = problems.Add(new RoadSegmentMissing(SegmentId));

        return problems;
    }
}
