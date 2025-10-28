namespace RoadRegistry.RoadNetwork;

using BackOffice.Core;
using Changes;
using RoadSegment;
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
        var originalIdOrId = change.OriginalId ?? change.Id;

        if (!_roadSegments.TryGetValue(change.Id, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(originalIdOrId));
        }

        _identifierTranslator.RegisterMapping(originalIdOrId, roadSegment.RoadSegmentId);
        return roadSegment.Modify(change, context);
    }

    private Problems RemoveRoadSegment(RemoveRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        if (!_roadSegments.TryGetValue(change.Id, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(change.Id));
        }

        return roadSegment.Remove(change, context);
    }
}
