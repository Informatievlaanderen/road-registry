namespace RoadRegistry.RoadNetwork;

using System;
using BackOffice.Core;
using RoadSegment;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using ValueObjects;

public partial class RoadNetwork
{
    private Problems AddRoadSegment(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var (roadSegment, problems) = RoadSegment.Add(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        _identifierTranslator.RegisterMapping(change.OriginalId ?? change.TemporaryId, roadSegment!.RoadSegmentId);
        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);
        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var originalIdOrId = change.OriginalId ?? change.RoadSegmentId;

        if (!_roadSegments.TryGetValue(change.RoadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(originalIdOrId));
        }

        _identifierTranslator.RegisterMapping(originalIdOrId, roadSegment.RoadSegmentId);
        return roadSegment.Modify(change, context);
    }

    private Problems ExecuteOnRoadSegment(RoadSegmentId roadSegmentId, Func<RoadSegment, Problems> modify)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        return modify(roadSegment);
    }
}
