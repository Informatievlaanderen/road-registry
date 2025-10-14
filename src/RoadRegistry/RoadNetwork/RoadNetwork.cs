namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Changes;
using GradeSeparatedJunction;
using RoadNode;
using RoadSegment;

//TODO-pr current: verplaats strongly typed classes naar RoadRegistry (RoadSegmentId, RoadNodeId,...)
//TODO-pr current: verplaats Problems ook, en zie hieronder voor per change deze te returnen
//TODO-pr current: voorzien RoadNetworkChangeFactory voor in ChangeRoadNetworkCommandHandler te gebruiken

public partial class RoadNetwork
{
    public static RoadNetwork Empty => new RoadNetwork();

    public IDictionary<int, RoadNode> RoadNodes { get; } = [];
    public IDictionary<int, RoadSegment> RoadSegments { get; } = [];
    public IDictionary<int, GradeSeparatedJunction> GradeSeparatedJunctions { get; } = [];

    private RoadNetwork()
    {
    }

    public RoadNetwork(
        IReadOnlyCollection<RoadNode> roadNodes,
        IReadOnlyCollection<RoadSegment> roadSegments,
        IReadOnlyCollection<GradeSeparatedJunction> gradeSeparatedJunctions)
    {
        RoadNodes = roadNodes.ToDictionary(x => x.Id, x => x);
        RoadSegments = roadSegments.ToDictionary(x => x.Id, x => x);
        GradeSeparatedJunctions = gradeSeparatedJunctions.ToDictionary(x => x.Id, x => x);
    }

    public RoadNetworkChangeResult Change(ICollection<IRoadNetworkChange> changes)
    {
        // produce change started event?

        var problems = Problems.None;

        // dit vervangt the RequestedChangeTranslator
        foreach (var roadNetworkChange in changes)
        {
            switch(roadNetworkChange)
            {
                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(change);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += ModifyRoadSegment(change);
                    break;
                // other cases
            }
        }

        // produce change completed event

        // commit events to entities? (roadsegment,...)

        return new RoadNetworkChangeResult(problems);
    }
}

public sealed record RoadNetworkChangeResult(Problems problems);
