namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Errors;
using Events.V2;
using Extensions;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadNode
{
    public Problems VerifyTopologyAndDetectType(ScopedRoadNetworkContext context)
    {
        var problems = Problems.WithContext(context.IdTranslator.TranslateToTemporaryId(RoadNodeId));

        var segments = context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.StartNodeId == RoadNodeId || x.EndNodeId == RoadNodeId)
            .ToList();

        if (IsRemoved)
        {
            return problems;
        }

        var byOtherNode =
            context.RoadNetwork.GetNonRemovedRoadNodes().FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.Value.IsReasonablyEqualTo(Geometry.Value, context.Tolerances));
        if (byOtherNode is not null)
        {
            problems += new RoadNodeGeometryTaken(context.IdTranslator.TranslateToTemporaryId(byOtherNode.RoadNodeId));
        }

        problems += context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(s =>
                segments.All(x => x.RoadSegmentId != s.RoadSegmentId)
                && s.Geometry.Value.IsWithinDistance(Geometry.Value, Distances.RoadNodeTooClose)
            )
            .Aggregate(Problems.None, (current, segment) =>
                current.Add(new RoadNodeTooClose()
                    .WithContext(ProblemContext.For(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId)))));

        problems += ValidateTypeAndChangeIfNeeded(context, segments, context.Provenance);

        return problems;
    }

    private Problems ValidateTypeAndChangeIfNeeded(ScopedRoadNetworkContext context, List<RoadSegment> segments, Provenance provenance)
    {
        var problems = Problems.None;

        if (segments.Count == 0)
        {
            problems += new RoadNodeNotConnectedToAnySegment();
        }
        else if (segments.Count == 1 && Type != RoadNodeTypeV2.Eindknoop)
        {
            Apply(new RoadNodeTypeWasChanged
            {
                RoadNodeId = RoadNodeId,
                Type = RoadNodeTypeV2.Eindknoop,
                Provenance = new ProvenanceData(provenance)
            });
        }
        else if (segments.Count == 2)
        {
            var segment1 = segments[0];
            var segment2 = segments[1];

            if (Grensknoop || !segment1.Attributes.Equals(segment2.Attributes))
            {
                // Must be schijnknoop
                if (Type != RoadNodeTypeV2.Schijnknoop)
                {
                    Apply(new RoadNodeTypeWasChanged
                    {
                        RoadNodeId = RoadNodeId,
                        Type = RoadNodeTypeV2.Schijnknoop,
                        Provenance = new ProvenanceData(provenance)
                    });
                }
            }
            else
            {
                problems += new RoadNodeIsNotAllowed();
            }
        }
        else if (segments.Count > 2 && Type != RoadNodeTypeV2.EchteKnoop)
        {
            Apply(new RoadNodeTypeWasChanged
            {
                RoadNodeId = RoadNodeId,
                Type = RoadNodeTypeV2.EchteKnoop,
                Provenance = new ProvenanceData(provenance)
            });
        }

        return problems;
    }
}
