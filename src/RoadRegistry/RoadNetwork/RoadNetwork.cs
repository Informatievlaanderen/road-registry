namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using Changes;
using GradeSeparatedJunction;
using RoadNode;
using RoadSegment;

public partial class RoadNetwork
{
    public static RoadNetwork Empty => new RoadNetwork();

    private Dictionary<RoadNodeId, RoadNode> RoadNodes { get; } = [];
    private Dictionary<RoadSegmentId, RoadSegment> RoadSegments { get; } = [];
    private Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; } = [];

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

    public RoadNetworkChangeResult Change(IReadOnlyCollection<IRoadNetworkChange> changes)
    {
        // produce change started event?

        var problems = new Dictionary<IRoadNetworkChange, Problems>();

        // dit vervangt the RequestedChangeTranslator
        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadSegmentChange change:
                    problems.Add(roadNetworkChange, AddRoadSegment(change));
                    break;
                case ModifyRoadSegmentChange change:
                    problems.Add(roadNetworkChange, ModifyRoadSegment(change));
                    break;
                // other cases
            }
        }

        // produce change completed event

        return new RoadNetworkChangeResult(problems);
    }
}

public sealed record RoadNetworkChangeResult(IDictionary<IRoadNetworkChange, Problems> ProblemsPerChange);
