namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using Events;
using GradeSeparatedJunction;
using GradeSeparatedJunction.Changes;
using RoadNode;
using RoadNode.Changes;
using RoadSegment;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using ValueObjects;

public partial class RoadNetwork : MartenAggregateRootEntity<string>
{
    public static RoadNetwork Empty => new();
    public const string GlobalIdentifier = "0";

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
        : base(GlobalIdentifier)
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

        var context = new RoadNetworkChangeContext
        {
            RoadNetwork = this,
            Tolerances = VerificationContextTolerances.Default,
            IdGenerator = roadNetworkIdGenerator,
            IdTranslator = _identifierTranslator
        };

        foreach (var roadNetworkChange in changes
                     .Select((change, ordinal) => new SortableChange(change, ordinal))
                     .OrderBy(x => x, new SortableChangeComparer())
                     .Select(x => x.Change))
        {
            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems = problems.AddRange(AddRoadNode(change, context));
                    break;
                case ModifyRoadNodeChange change:
                    problems.AddRange(ModifyRoadNode(change));
                    break;
                case RemoveRoadNodeChange change:
                    problems.AddRange(RemoveRoadNode(change));
                    break;

                case AddRoadSegmentChange change:
                    problems = problems.AddRange(AddRoadSegment(change, context));
                    break;
                case ModifyRoadSegmentChange change:
                    problems = problems.AddRange(ModifyRoadSegment(change, context));
                    break;
                case RemoveRoadSegmentChange change:
                    problems = problems.AddRange(ExecuteOnRoadSegment(change.RoadSegmentId, segment => segment.Remove()));
                    break;
                case AddRoadSegmentToEuropeanRoadChange change:
                    problems = problems.AddRange(ExecuteOnRoadSegment(change.RoadSegmentId, segment => segment.AddEuropeanRoad(change)));
                    break;
                case RemoveRoadSegmentFromEuropeanRoadChange change:
                    problems = problems.AddRange(ExecuteOnRoadSegment(change.RoadSegmentId, segment => segment.RemoveEuropeanRoad(change)));
                    break;
                case AddRoadSegmentToNationalRoadChange change:
                    problems = problems.AddRange(ExecuteOnRoadSegment(change.RoadSegmentId, segment => segment.AddNationalRoad(change)));
                    break;
                case RemoveRoadSegmentFromNationalRoadChange change:
                    problems = problems.AddRange(ExecuteOnRoadSegment(change.RoadSegmentId, segment => segment.RemoveNationalRoad(change)));
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems = problems.AddRange(AddGradeSeparatedJunction(change, context));
                    break;
                case RemoveGradeSeparatedJunctionChange change:
                    problems.AddRange(RemoveGradeSeparatedJunction(change));
                    break;

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

        UncommittedEvents.Add(new RoadNetworkChanged());

        return new RoadNetworkChangeResult(problems);
    }

    private sealed class SortableChangeComparer : IComparer<SortableChange>
    {
        private static readonly Type[] SequenceByTypeOfChange =
        {
            typeof(AddRoadNodeChange),
            typeof(AddRoadSegmentChange),
            typeof(AddRoadSegmentToEuropeanRoadChange),
            typeof(AddRoadSegmentToNationalRoadChange),
            typeof(AddGradeSeparatedJunctionChange),
            typeof(ModifyRoadNodeChange),
            typeof(ModifyRoadSegmentChange),
            typeof(RemoveRoadSegmentFromEuropeanRoadChange),
            typeof(RemoveRoadSegmentFromNationalRoadChange),
            typeof(RemoveGradeSeparatedJunctionChange),
            typeof(RemoveRoadSegmentChange),
            typeof(RemoveRoadNodeChange)
        };

        public int Compare(SortableChange? left, SortableChange? right)
        {
            if (left is null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            var leftRank = Array.IndexOf(SequenceByTypeOfChange, left.Change.GetType());
            if (leftRank == -1)
            {
                throw new InvalidOperationException($"Change of type {left.Change.GetType().Name} is not configured in '{nameof(SequenceByTypeOfChange)}.");
            }

            var rightRank = Array.IndexOf(SequenceByTypeOfChange, right.Change.GetType());
            if (rightRank == -1)
            {
                throw new InvalidOperationException($"Change of type {right.Change.GetType().Name} is not configured in '{nameof(SequenceByTypeOfChange)}.");
            }

            var comparison = leftRank.CompareTo(rightRank);
            return comparison != 0
                ? comparison
                : left.Ordinal.CompareTo(right.Ordinal);
        }
    }

    private sealed class SortableChange
    {
        public SortableChange(object change, int ordinal)
        {
            Ordinal = ordinal;
            Change = change;
        }

        public object Change { get; }
        public int Ordinal { get; }
    }
}

public sealed record RoadNetworkChangeResult(Problems Problems);
