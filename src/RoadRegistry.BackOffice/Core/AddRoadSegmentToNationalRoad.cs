namespace RoadRegistry.BackOffice.Core;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;

public class AddRoadSegmentToNationalRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "AddRoadSegmentToNationalRoad";

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

    public AttributeId AttributeId { get; }
    public NationalRoadNumber Number { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentId? TemporarySegmentId { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentId = SegmentId,
            TemporaryAttributeId = TemporaryAttributeId
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
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

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
