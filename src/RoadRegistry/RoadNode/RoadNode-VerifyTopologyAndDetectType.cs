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
    // mayMergeRoadSegments: whether a node that turns out to be unnecessary may take the two segments hanging off it
    // with it, merging them into one. That belongs to the inwinning/change upload, which reshapes the network as a
    // whole. An editing action names one segment and must leave the others as they are, so it turns this off.
    public Problems VerifyTopologyAndUpdateType(LazyQuadtree<RoadSegment> roadSegmentsSpatialIndex, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context, bool mayMergeRoadSegments = true)
    {
        var problems = Problems.WithContext(context.IdTranslator.TranslateToTemporaryId(RoadNodeId));

        var segments = context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.StartNodeId == RoadNodeId || x.EndNodeId == RoadNodeId)
            .ToList();

        var byOtherNode =
            context.RoadNetwork.GetNonRemovedRoadNodes().FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.Value.EqualsExact(Geometry.Value));
        if (byOtherNode is not null)
        {
            problems += new RoadNodeGeometryTaken(context.IdTranslator.TranslateToTemporaryId(byOtherNode.RoadNodeId));
        }

        problems += ValidateTypeAndChangeIfNeeded(segments, roadSegmentsSpatialIndex, idGenerator, context, mayMergeRoadSegments);

        return problems;
    }

    private Problems ValidateTypeAndChangeIfNeeded(List<RoadSegment> segments, LazyQuadtree<RoadSegment> roadSegmentsSpatialIndex, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context, bool mayMergeRoadSegments)
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

                problems += MergeRoadSegmentsIfNodeIsNotNeeded(segment1, segment2, roadSegmentsSpatialIndex, idGenerator, context, mayMergeRoadSegments);
            }
        }
        else
        {
            ChangeTypeTo(RoadNodeTypeV2.EchteKnoop, context.Provenance);
        }

        return problems;
    }

    private Problems MergeRoadSegmentsIfNodeIsNotNeeded(RoadSegment segment1, RoadSegment segment2, LazyQuadtree<RoadSegment> roadSegmentsSpatialIndex, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context, bool mayMergeRoadSegments)
    {
        var roadNodeIsNeeded = !mayMergeRoadSegments
                               || segment1.Attributes is null
                               || segment2.Attributes is null
                               || RoadNodePreventsInvalidRoadSegmentGeometry(segment1, segment2, roadSegmentsSpatialIndex, context);
        if (roadNodeIsNeeded)
        {
            ChangeTypeTo(RoadNodeTypeV2.Validatieknoop, context.Provenance);
            return Problems.None;
        }

        var problems = context.RoadNetwork.MergeRoadSegments(segment1, segment2, idGenerator, context);
        return problems;
    }

    private bool RoadNodePreventsInvalidRoadSegmentGeometry(RoadSegment segment1, RoadSegment segment2, LazyQuadtree<RoadSegment> roadSegmentGridSpatialIndex, ScopedRoadNetworkChangeContext context)
    {
        var mergedGeometry = RoadSegmentGeometryHelper.MergeGeometries(segment1, segment2, RoadNodeId, context);

        if (mergedGeometry.GetSingleLineString().SelfOverlaps())
        {
            return true;
        }

        if (RoadSegmentGeometryHelper.GetSameStartEndNodeInvalidGeometrySection(mergedGeometry, context.Tolerances) is not null)
        {
            return true;
        }

        if (RoadSegmentGeometryHelper.GetSelfIntersectingInvalidGeometrySection(mergedGeometry, context.Tolerances) is not null)
        {
            return true;
        }

        var candidateSegments = roadSegmentGridSpatialIndex.Query(mergedGeometry.EnvelopeInternal)
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
