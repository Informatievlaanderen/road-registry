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

    public IReadOnlyDictionary<RoadNodeId, RoadNode> RoadNodes { get; }
    public IReadOnlyDictionary<RoadSegmentId, RoadSegment> RoadSegments { get; }
    public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions { get; }

    private readonly Dictionary<RoadNodeId, RoadNode> _roadNodes = [];
    private readonly Dictionary<RoadSegmentId, RoadSegment> _roadSegments = [];
    private readonly Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> _gradeSeparatedJunctions = [];

    private RoadNetwork()
    {
    }

    public RoadNetwork(
        IReadOnlyCollection<RoadNode> roadNodes,
        IReadOnlyCollection<RoadSegment> roadSegments,
        IReadOnlyCollection<GradeSeparatedJunction> gradeSeparatedJunctions)
    {
        _roadNodes = roadNodes.ToDictionary(x => x.RoadNodeId, x => x);
        RoadNodes = _roadNodes.AsReadOnly();

        _roadSegments = roadSegments.ToDictionary(x => x.RoadSegmentId, x => x);
        RoadSegments = _roadSegments.AsReadOnly();

        _gradeSeparatedJunctions = gradeSeparatedJunctions.ToDictionary(x => x.GradeSeparatedJunctionId, x => x);
        GradeSeparatedJunctions = _gradeSeparatedJunctions.AsReadOnly();
    }

    public RoadNetworkChangeResult Change(RoadNetworkChanges changes, IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        // produce change started event?

        var problems = Problems.None;

        // dit vervangt the RequestedChangeTranslator
        var context = new RoadNetworkChangeContext
        {
            RoadNetwork = this,
            Tolerances = VerificationContextTolerances.Default,
            IdGenerator = roadNetworkIdGenerator,
            Translator = changes
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
                case RemoveRoadSegmentChange change:
                    problems.AddRange(RemoveRoadSegment(change, context));
                    break;
                //TODO-pr other cases
                default:
                    throw new NotImplementedException($"{roadNetworkChange.GetType().Name} is not implemented.");
            }
        }

        if (!problems.HasError())
        {
            problems = _roadNodes.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopologyAfterChanges(context));
            problems = _roadSegments.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopologyAfterChanges(context));
            problems = _gradeSeparatedJunctions.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopologyAfterChanges(context));
        }

        // produce change completed event

        return new RoadNetworkChangeResult(problems);
    }
}

public sealed record RoadNetworkChangeResult(Problems Problems);
