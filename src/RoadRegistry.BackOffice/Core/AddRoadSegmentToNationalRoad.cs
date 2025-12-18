namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;
using Problem = RoadRegistry.Infrastructure.Messages.Problem;

public class AddRoadSegmentToNationalRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "AddRoadSegmentToNationalRoad";

    public AddRoadSegmentToNationalRoad(AttributeId attributeId,
        AttributeId temporaryAttributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        RoadSegmentId? temporarySegmentId,
        NationalRoadNumber number,
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
    public NationalRoadNumber Number { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public RoadSegmentId? TemporarySegmentId { get; }
    public RoadSegmentVersion SegmentVersion { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
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

        message.AddRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
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
