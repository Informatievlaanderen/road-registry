namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using RoadRegistry.RoadNetwork.ValueObjects;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class RemoveRoadSegmentFromNumberedRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "RemoveRoadSegmentFromNumberedRoad";

    public RemoveRoadSegmentFromNumberedRoad(
        AttributeId attributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        NumberedRoadNumber number,
        RoadSegmentVersion segmentVersion)
    {
        AttributeId = attributeId;
        SegmentGeometryDrawMethod = segmentGeometryDrawMethod;
        SegmentId = segmentId;
        Number = number;
        SegmentVersion = segmentVersion;
    }

    public AttributeId AttributeId { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentId SegmentId { get; }
    public RoadSegmentVersion SegmentVersion { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentRemovedFromNumberedRoad = new RoadSegmentRemovedFromNumberedRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId,
                SegmentVersion = SegmentVersion
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadSegmentFromNumberedRoad = new Messages.RemoveRoadSegmentFromNumberedRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
            SegmentId = SegmentId
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return VerifyAfterResult.WithAcceptedChanges(Problems.None, TranslateTo);
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
            if (segment.NumberedRoadAttributes.All(x => x.Value.Number != Number))
            {
                problems = problems.Add(new NumberedRoadNumberNotFound(Number));
            }
        }

        return problems;
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
