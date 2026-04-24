namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using Extensions;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadNode
{
    public Problems VerifyTopologyAndUpdateType(RoadSegmentsSpatialIndex<RoadSegment> roadSegmentsSpatialIndex, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context)
    {
        var problems = Problems.WithContext(context.IdTranslator.TranslateToTemporaryId(RoadNodeId));

        var segments = context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.Attributes is not null && (x.StartNodeId == RoadNodeId || x.EndNodeId == RoadNodeId))
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

        problems += ValidateTypeAndChangeIfNeeded(segments, roadSegmentsSpatialIndex, idGenerator, context);

        return problems;
    }

    private Problems ValidateTypeAndChangeIfNeeded(List<RoadSegment> segments, RoadSegmentsSpatialIndex<RoadSegment> roadSegmentsSpatialIndex, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context)
    {
        var problems = Problems.None;

        if (segments.Count == 0)
        {
            problems += new RoadNodeNotConnectedToAnySegment();
        }
        else if (segments.Count == 1)
        {
            ChangeTypeTo(RoadNodeTypeV2.Eindknoop, context.Provenance);
        }
        else if (segments.Count == 2)
        {
            if (Grensknoop)
            {
                ChangeTypeTo(RoadNodeTypeV2.Validatieknoop, context.Provenance);
            }
            else
            {
                var segment1 = segments[0];
                var segment2 = segments[1];

                problems += MergeRoadSegmentsIfNodeIsNotNeeded(segment1, segment2, roadSegmentsSpatialIndex, idGenerator, context);
            }
        }
        else
        {
            ChangeTypeTo(RoadNodeTypeV2.EchteKnoop, context.Provenance);
        }

        return problems;
    }

    private Problems MergeRoadSegmentsIfNodeIsNotNeeded(RoadSegment segment1, RoadSegment segment2, RoadSegmentsSpatialIndex<RoadSegment> roadSegmentsSpatialIndex, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context)
    {
        var roadNodeIsNeeded = RoadNodePreventsInvalidRoadSegmentGeometry(segment1, segment2, roadSegmentsSpatialIndex, context);
        if (roadNodeIsNeeded)
        {
            ChangeTypeTo(RoadNodeTypeV2.Validatieknoop, context.Provenance);
            return Problems.None;
        }

        var (mergedSegment, problems) = context.RoadNetwork.MergeRoadSegments(segment1, segment2, idGenerator, context);
        if (mergedSegment is not null)
        {
            roadSegmentsSpatialIndex.Remove(segment1.Geometry.Value, segment1);
            roadSegmentsSpatialIndex.Remove(segment2.Geometry.Value, segment2);
            roadSegmentsSpatialIndex.Add(mergedSegment.Geometry.Value, mergedSegment);
        }

        return problems;
    }

    private bool RoadNodePreventsInvalidRoadSegmentGeometry(RoadSegment segment1, RoadSegment segment2, RoadSegmentsSpatialIndex<RoadSegment> roadSegmentsSpatialIndex, ScopedRoadNetworkContext context)
    {
        var mergedGeometry = RoadSegmentGeometryHelper.MergeGeometries(segment1, segment2, RoadNodeId, context);

        if (RoadSegmentGeometryHelper.GetSameStartEndNodeInvalidGeometrySection(mergedGeometry, context.Tolerances) is not null)
        {
            return true;
        }

        if (RoadSegmentGeometryHelper.GetSelfIntersectingInvalidGeometrySection(mergedGeometry, context.Tolerances) is not null)
        {
            return true;
        }

        var candidateSegments = roadSegmentsSpatialIndex.Query(mergedGeometry)
            .Where(x => x.RoadSegmentId != segment1.RoadSegmentId && x.RoadSegmentId != segment2.RoadSegmentId)
            .ToArray();
        foreach (var otherSegment in candidateSegments)
        {
            if (RoadSegmentGeometryHelper.GetFirstMultipleIntersectionsInvalidGeometrySection(mergedGeometry, otherSegment.Geometry.Value, context.Tolerances) is not null)
            {
                return true;
            }
        }

        return false;
    }

    private void ChangeTypeTo(RoadNodeTypeV2 type, Provenance provenance)
    {
        if (Type != type)
        {
            Apply(new RoadNodeTypeWasChanged
            {
                RoadNodeId = RoadNodeId,
                Type = type,
                Provenance = new ProvenanceData(provenance)
            });
        }
    }
}
