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

    public RoadNetworkChangeResult Change(RoadNetworkChanges changes, DownloadId? downloadId, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;
        var summary = new RoadNetworkChangesSummary();
        var idTranslator = new IdentifierTranslator();

        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems += AddRoadNode(change, idGenerator, idTranslator, summary.RoadNodes);
                    break;
                case ModifyRoadNodeChange change:
                    problems += ModifyRoadNode(change, summary.RoadNodes);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNode(change, summary.RoadNodes);
                    break;

                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(change, idGenerator, idTranslator, summary.RoadSegments);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += ModifyRoadSegment(change, idTranslator, summary.RoadSegments);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RemoveRoadSegment(change.RoadSegmentId, summary.RoadSegments);
                    break;
                case AddRoadSegmentToEuropeanRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.AddEuropeanRoad(change), summary.RoadSegments);
                    break;
                case RemoveRoadSegmentFromEuropeanRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.RemoveEuropeanRoad(change), summary.RoadSegments);
                    break;
                case AddRoadSegmentToNationalRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.AddNationalRoad(change), summary.RoadSegments);
                    break;
                case RemoveRoadSegmentFromNationalRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.RemoveNationalRoad(change), summary.RoadSegments);
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(change, idGenerator, idTranslator, summary.GradeSeparatedJunctions);
                    break;
                case RemoveGradeSeparatedJunctionChange change:
                    problems += RemoveGradeSeparatedJunction(change, summary.GradeSeparatedJunctions);
                    break;

                default:
                    throw new NotImplementedException($"{roadNetworkChange.GetType().Name} is not implemented.");
            }
        }

        if (!problems.HasError())
        {
            var context = new RoadNetworkVerifyTopologyContext
            {
                RoadNetwork = this,
                IdTranslator = idTranslator
            };

            problems = _roadNodes.Values.Where(x => x.HasChanges()).Select(x => x.RoadNodeId)
                .Concat(_roadSegments.Values.Where(x => x.HasChanges()).SelectMany(x => x.Nodes))
                .Distinct()
                .Select(x => _roadNodes.GetValueOrDefault(x))
                .Where(x => x is not null)
                .Aggregate(problems, (p, x) => p + x!.VerifyTopology(context));
            problems = _roadSegments.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopology(context));
            problems = _gradeSeparatedJunctions.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopology(context));
        }

        UncommittedEvents.Add(new RoadNetworkChanged
        {
            ScopeGeometry = changes.BuildScopeGeometry().ToGeometryObject(),
            DownloadId = downloadId
        });

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), summary);
    }
}

public sealed record RoadNetworkChangeResult(Problems Problems, RoadNetworkChangesSummary Changes);

public sealed class RoadNetworkChangesSummary
{
    public RoadNetworkEntityChangesSummary<RoadNodeId> RoadNodes { get; } = new();
    public RoadNetworkEntityChangesSummary<RoadSegmentId> RoadSegments { get; } = new();
    public RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> GradeSeparatedJunctions { get; } = new();
}

public sealed class RoadNetworkEntityChangesSummary<TIdentifier>
{
    public UniqueList<TIdentifier> Added { get; } = [];
    public UniqueList<TIdentifier> Modified { get; } = [];
    public UniqueList<TIdentifier> Removed { get; } = [];

    public sealed class UniqueList<T> : List<T>
    {
        public new void Add(T item)
        {
            if (!Contains(item))
            {
                base.Add(item);
            }
        }
    }
}
