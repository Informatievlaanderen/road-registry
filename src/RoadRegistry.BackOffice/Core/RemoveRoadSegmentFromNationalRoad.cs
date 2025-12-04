namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class RemoveRoadSegmentFromNationalRoad : IRequestedChange, IHaveHash
{
    public const string EventName = "RemoveRoadSegmentFromNationalRoad";

    public RemoveRoadSegmentFromNationalRoad(
        AttributeId attributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        NationalRoadNumber number,
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
    public NationalRoadNumber Number { get; }
    public RoadSegmentId SegmentId { get; }
    public RoadSegmentVersion SegmentVersion { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentRemovedFromNationalRoad = new RoadSegmentRemovedFromNationalRoad
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

        message.RemoveRoadSegmentFromNationalRoad = new Messages.RemoveRoadSegmentFromNationalRoad
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
