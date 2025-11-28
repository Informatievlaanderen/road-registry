namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using RoadRegistry.RoadNetwork.ValueObjects;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class AddRoadSegmentToEuropeanRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "AddRoadSegmentToEuropeanRoad";

    public AddRoadSegmentToEuropeanRoad(AttributeId attributeId,
        AttributeId temporaryAttributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        RoadSegmentId? temporarySegmentId,
        EuropeanRoadNumber number,
        RoadSegmentVersion segmentVersion)
    {
        AttributeId = attributeId;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentGeometryDrawMethod = segmentGeometryDrawMethod;
        SegmentId = segmentId;
        TemporarySegmentId = temporarySegmentId;
        Number = number;
        SegmentVersion = segmentVersion;
    }

    public AttributeId AttributeId { get; }
    public EuropeanRoadNumber Number { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentId? TemporarySegmentId { get; }
    public RoadSegmentVersion SegmentVersion { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId,
                TemporaryAttributeId = TemporaryAttributeId,
                SegmentVersion = SegmentVersion
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
            SegmentId = SegmentId
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.AfterView.View.Segments.ContainsKey(SegmentId)) problems = problems.Add(new RoadSegmentMissing(TemporarySegmentId ?? SegmentId));

        return VerifyAfterResult.WithAcceptedChanges(problems, TranslateTo);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return Problems.None;
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
