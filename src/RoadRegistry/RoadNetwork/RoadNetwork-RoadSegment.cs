namespace RoadRegistry.RoadNetwork;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BackOffice.Core;
using Changes;
using RoadSegment;
using RoadSegment.ValueObjects;
using ValueObjects;

public partial class RoadNetwork
{
    public bool TryGetRoadSegment(RoadSegmentId roadSegmentId, [MaybeNullWhen(false)] out RoadSegment segment)
    {
        return RoadSegments.TryGetValue(roadSegmentId, out segment);
    }

    public bool TryGetRoadSegment(Predicate<RoadSegment> match, [MaybeNullWhen(false)] out RoadSegment segment)
    {
        segment = RoadSegments
            .Where(x => match(x.Value))
            .Select(x => x.Value)
            .FirstOrDefault();
        return segment is not null;
    }

    private Problems AddRoadSegment(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var (roadSegment, problems) = RoadSegment.Register(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        RoadSegments.Add(roadSegment.Id, roadSegment);
        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        if (!TryGetRoadSegment(change.Id, out var segment))
        {
            return Problems.Single(new RoadSegmentNotFound(change.OriginalId ?? change.Id));
        }

        return segment.Modify(change, context);
    }
}
