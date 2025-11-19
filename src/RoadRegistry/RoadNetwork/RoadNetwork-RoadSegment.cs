namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using BackOffice.Core;
using RoadSegment;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;

public partial class RoadNetwork
{
    public IEnumerable<RoadSegment> GetNonRemovedRoadSegments()
    {
        return _roadSegments.Values.Where(x => !x.IsRemoved);
    }

    private Problems AddRoadSegment(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
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
        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, IIdentifierTranslator idTranslator)
    {
        var originalIdOrId = change.OriginalId ?? change.RoadSegmentId;

        if (!_roadSegments.TryGetValue(change.RoadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(originalIdOrId));
        }

        var problems = idTranslator.RegisterMapping(originalIdOrId, roadSegment.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        return roadSegment.Modify(change);
    }

    private Problems InvokeOnRoadSegment(RoadSegmentId roadSegmentId, Func<RoadSegment, Problems> modify)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        return modify(roadSegment);
    }
}
