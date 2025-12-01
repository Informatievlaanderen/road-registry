namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment;
using RoadSegment.Changes;

public partial class RoadNetwork
{
    public IEnumerable<RoadSegment> GetNonRemovedRoadSegments()
    {
        return _roadSegments.Values.Where(x => !x.IsRemoved);
    }

    private Problems AddRoadSegment(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        var (roadSegment, problems) = RoadSegment.Add(change, idGenerator, idTranslator);
        if (problems.HasError())
        {
            return problems;
        }

        problems += idTranslator.RegisterMapping(change.OriginalId ?? change.TemporaryId, roadSegment!.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);
        summary.Added.Add(roadSegment.RoadSegmentId);

        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;

        if (!_roadSegments.TryGetValue(change.RoadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(originalId));
        }

        var problems = idTranslator.RegisterMapping(originalId, roadSegment.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        problems = roadSegment.Modify(change);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadSegment.RoadSegmentId);
        return problems;
    }

    private Problems ModifyRoadSegment(RoadSegmentId roadSegmentId, Func<RoadSegment, Problems> modify, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        var problems = modify(roadSegment);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadSegmentId);
        return problems;
    }

    private Problems RemoveRoadSegment(RoadSegmentId roadSegmentId, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        var problems = roadSegment.Remove();
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(roadSegment.RoadSegmentId);
        return problems;
    }
}
