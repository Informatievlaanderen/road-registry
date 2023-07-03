namespace RoadRegistry.BackOffice.Core;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;

public class RemoveRoadSegmentFromNationalRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "RemoveRoadSegmentFromNationalRoad";

    public RemoveRoadSegmentFromNationalRoad(
        AttributeId attributeId,
        RoadSegmentId segmentId,
        NationalRoadNumber number)
    {
        AttributeId = attributeId;
        SegmentId = segmentId;
        Number = number;
    }

    public AttributeId AttributeId { get; }
    public NationalRoadNumber Number { get; }
    public RoadSegmentId SegmentId { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentRemovedFromNationalRoad = new RoadSegmentRemovedFromNationalRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentId = SegmentId
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadSegmentFromNationalRoad = new Messages.RemoveRoadSegmentFromNationalRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentId = SegmentId
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return Problems.None;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.BeforeView.View.Segments.TryGetValue(SegmentId, out var segment))
        {
            problems = problems.Add(new RoadSegmentMissing(SegmentId));
        }
        else
        {
            if (segment.NationalRoadAttributes.All(x => x.Value.Number != Number))
            {
                problems = problems.Add(new NationalRoadNumberNotFound(Number));
            }
        }

        return problems;
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
