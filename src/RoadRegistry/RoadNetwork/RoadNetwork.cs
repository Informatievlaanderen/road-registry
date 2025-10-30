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

    private readonly Dictionary<RoadNodeId, RoadNode> _roadNodes;
    private readonly Dictionary<RoadSegmentId, RoadSegment> _roadSegments;
    private readonly Dictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> _gradeSeparatedJunctions;
    private readonly IdentifierTranslator _identifierTranslator = new();

    private RoadNetwork()
        : this([], [], [])
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
        var problems = Problems.None;

        // dit vervangt the RequestedChangeTranslator
        var context = new RoadNetworkChangeContext
        {
            RoadNetwork = this,
            Tolerances = VerificationContextTolerances.Default,
            IdGenerator = roadNetworkIdGenerator,
            IdTranslator = _identifierTranslator
        };

        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems.AddRange(AddRoadNode(change, context));
                    break;
                // case ModifyRoadNodeChange change:
                //     problems.AddRange(ModifyRoadNode(change, context));
                //     break;
                // case RemoveRoadNodeChange change:
                //     problems.AddRange(RemoveRoadNode(change, context));
                //     break;

                case AddRoadSegmentChange change:
                    problems.AddRange(AddRoadSegment(change, context));
                    break;
                case ModifyRoadSegmentChange change:
                    problems.AddRange(ModifyRoadSegment(change, context));
                    break;
                case RemoveRoadSegmentChange change:
                    problems.AddRange(RemoveRoadSegment(change, context));
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems.AddRange(AddGradeSeparatedJunction(change, context));
                    break;
                // case ModifyGradeSeparatedJunctionChange change:
                //     problems.AddRange(ModifyGradeSeparatedJunction(change, context));
                //     break;
                // case RemoveGradeSeparatedJunctionChange change:
                //     problems.AddRange(RemoveGradeSeparatedJunction(change, context));
                //     break;
                //TODO-pr other cases
                default:
                    throw new NotImplementedException($"{roadNetworkChange.GetType().Name} is not implemented.");
            }
        }

        if (!problems.HasError())
        {
            problems = _roadNodes.Values.Where(x => x.HasChanges()).Select(x => x.RoadNodeId)
                .Concat(_roadSegments.Values.Where(x => x.HasChanges()).SelectMany(x => x.Nodes))
                .Distinct()
                .Select(x => _roadNodes.GetValueOrDefault(x))
                .Where(x => x is not null)
                .Aggregate(problems, (p, x) => p + x!.VerifyTopologyAfterChanges(context));
            problems = _roadSegments.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopologyAfterChanges(context));
            problems = _gradeSeparatedJunctions.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopologyAfterChanges(context));
        }

        return new RoadNetworkChangeResult(problems);
    }
}

public sealed record RoadNetworkChangeResult(Problems Problems);
