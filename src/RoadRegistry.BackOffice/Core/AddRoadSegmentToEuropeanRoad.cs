namespace RoadRegistry.BackOffice.Core;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;

public class AddRoadSegmentToEuropeanRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "AddRoadSegmentToEuropeanRoad";

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

    public AttributeId AttributeId { get; }
    public EuropeanRoadNumber Number { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentId? TemporarySegmentId { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
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

        message.AddRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
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
