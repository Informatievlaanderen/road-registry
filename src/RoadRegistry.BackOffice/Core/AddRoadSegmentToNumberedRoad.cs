namespace RoadRegistry.BackOffice.Core;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;

public class AddRoadSegmentToNumberedRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "AddRoadSegmentToNumberedRoad";

    public AddRoadSegmentToNumberedRoad(AttributeId attributeId,
        AttributeId temporaryAttributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        RoadSegmentId? temporarySegmentId,
        NumberedRoadNumber number,
        RoadSegmentNumberedRoadDirection direction,
        RoadSegmentNumberedRoadOrdinal ordinal,
        RoadSegmentVersion segmentVersion)
    {
        AttributeId = attributeId;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentGeometryDrawMethod = segmentGeometryDrawMethod;
        SegmentId = segmentId;
        TemporarySegmentId = temporarySegmentId;
        Number = number;
        Direction = direction;
        Ordinal = ordinal;
        SegmentVersion = segmentVersion;
    }

    public AttributeId AttributeId { get; }
    public RoadSegmentNumberedRoadDirection Direction { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public RoadSegmentId? TemporarySegmentId { get; }
    public RoadSegmentVersion SegmentVersion { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            Direction = Direction,
            Ordinal = Ordinal,
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
            SegmentId = SegmentId,
            TemporaryAttributeId = TemporaryAttributeId,
            SegmentVersion = SegmentVersion
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
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
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
