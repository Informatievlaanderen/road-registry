namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using Changes;
using GradeSeparatedJunction;
using RoadNode;
using RoadSegment;
using RoadSegment.ValueObjects;
using ValueObjects;

public partial class RoadNetwork
{
    public static RoadNetwork Empty => new();

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

    public RoadNetworkChangeResult Change(IReadOnlyCollection<IRoadNetworkChange> changes, IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        // produce change started event?

        var problems = Problems.None;

        // dit vervangt the RequestedChangeTranslator
        var context = new RoadNetworkChangeContext
        {
            RoadNetwork = this,
            Tolerances = VerificationContextTolerances.Default,
            IdGenerator = roadNetworkIdGenerator
        };

        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadSegmentChange change:
                    problems.AddRange(AddRoadSegment(change, context));
                    break;
                case ModifyRoadSegmentChange change:
                    problems.AddRange(ModifyRoadSegment(change, context));
                    break;
                //TODO-pr other cases
                default:
                    throw new NotImplementedException($"{roadNetworkChange.GetType().Name} is not implemented.");
            }
        }

        if (problems.HasError())
        {
            //TODO-pr: verifywithinroadnetwork op alle gewijzigd/toegevoegde entiteiten, en degene die gelinkt zijn aan verwijderde
            problems = RoadNodes.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyWithinRoadNetwork(context));
            problems = RoadSegments.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyWithinRoadNetwork(context));
            problems = GradeSeparatedJunctions.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyWithinRoadNetwork(context));
        }

        // produce change completed event

        return new RoadNetworkChangeResult(problems);
    }
}

public sealed record RoadNetworkChangeResult(Problems Problems);
