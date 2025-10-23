namespace RoadRegistry.RoadNetwork;

using BackOffice.Core;
using Changes;
using RoadSegment;
using ValueObjects;

public partial class RoadNetwork
{
    private Problems AddRoadSegment(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var (roadSegment, problems) = RoadSegment.Register(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegments.Add(roadSegment.Id, roadSegment);
        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        if (!_roadSegments.TryGetValue(change.Id, out var segment))
        {
            return Problems.Single(new RoadSegmentNotFound(change.OriginalId ?? change.Id));
        }

        return segment.Modify(change, context);
    }

    private Problems RemoveRoadSegment(RemoveRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        if (!_roadSegments.TryGetValue(change.Id, out var segment))
        {
            return Problems.Single(new RoadSegmentNotFound(change.Id));
        }

        return segment.Remove(change, context);
    }
}
