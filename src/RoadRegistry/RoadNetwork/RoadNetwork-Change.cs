namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events;
using GradeSeparatedJunction.Changes;
using RoadNode.Changes;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment;
using RoadSegment.Changes;
using ValueObjects;
using RoadNode = RoadNode.RoadNode;
using GradeSeparatedJunction = GradeSeparatedJunction.GradeSeparatedJunction;

public partial class RoadNetwork
{
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
                    problems += AddRoadNode(changes, change, idGenerator, idTranslator, summary.RoadNodes);
                    break;
                case ModifyRoadNodeChange change:
                    problems += ModifyRoadNode(changes, change, summary.RoadNodes);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNode(changes, change, summary.RoadNodes);
                    break;

                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(changes, change, idGenerator, idTranslator, summary.RoadSegments);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += ModifyRoadSegment(changes, change, idTranslator, summary.RoadSegments);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RemoveRoadSegment(changes, change.RoadSegmentId, summary.RoadSegments);
                    break;
                case AddRoadSegmentToEuropeanRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.AddEuropeanRoad(change, changes.Provenance), summary.RoadSegments);
                    break;
                case RemoveRoadSegmentFromEuropeanRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.RemoveEuropeanRoad(change, changes.Provenance), summary.RoadSegments);
                    break;
                case AddRoadSegmentToNationalRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.AddNationalRoad(change, changes.Provenance), summary.RoadSegments);
                    break;
                case RemoveRoadSegmentFromNationalRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment => segment.RemoveNationalRoad(change, changes.Provenance), summary.RoadSegments);
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(changes, change, idGenerator, idTranslator, summary.GradeSeparatedJunctions);
                    break;
                case ModifyGradeSeparatedJunctionChange change:
                    problems += ModifyGradeSeparatedJunction(changes, change, summary.GradeSeparatedJunctions);
                    break;
                case RemoveGradeSeparatedJunctionChange change:
                    problems += RemoveGradeSeparatedJunction(changes, change, summary.GradeSeparatedJunctions);
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

        Apply(new RoadNetworkChanged
        {
            ScopeGeometry = changes.BuildScopeGeometry().ToGeometryObject(),
            DownloadId = downloadId,
            Provenance = new ProvenanceData(changes.Provenance)
        });

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), summary);
    }

    private Problems AddRoadNode(RoadNetworkChanges changes, AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        var (roadNode, problems) = RoadNode.Add(change, changes.Provenance, idGenerator);
        if (problems.HasError())
        {
            return problems;
        }

        problems += idTranslator.RegisterMapping(change.TemporaryId, roadNode!.RoadNodeId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodes.Add(roadNode.RoadNodeId, roadNode);
        summary.Added.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems ModifyRoadNode(RoadNetworkChanges changes, ModifyRoadNodeChange change, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.RoadNodeId));
        }

        var problems = roadNode.Modify(change, changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems RemoveRoadNode(RoadNetworkChanges changes, RemoveRoadNodeChange change, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.RoadNodeId));
        }

        var problems = roadNode.Remove(changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems AddRoadSegment(RoadNetworkChanges changes, AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        var (roadSegment, problems) = RoadSegment.Add(change, changes.Provenance, idGenerator, idTranslator);
        if (problems.HasError())
        {
            return problems;
        }

        problems += idTranslator.RegisterMapping(change.OriginalId ?? change.TemporaryId, roadSegment!.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);
        summary.Added.Add(roadSegment.RoadSegmentId);

        return problems;
    }

    private Problems ModifyRoadSegment(RoadNetworkChanges changes, ModifyRoadSegmentChange change, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;

        if (!_roadSegments.TryGetValue(change.RoadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(originalId));
        }

        var problems = idTranslator.RegisterMapping(originalId, roadSegment.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        problems = roadSegment.Modify(change, changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadSegment.RoadSegmentId);
        return problems;
    }

    private Problems ModifyRoadSegment(RoadSegmentId roadSegmentId, Func<RoadSegment, Problems> modify, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        var problems = modify(roadSegment);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadSegmentId);
        return problems;
    }

    private Problems RemoveRoadSegment(RoadNetworkChanges changes, RoadSegmentId roadSegmentId, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        var problems = roadSegment.Remove(changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(roadSegment.RoadSegmentId);
        return problems;
    }

    private Problems AddGradeSeparatedJunction(RoadNetworkChanges changes, AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
    {
        var (gradeSeparatedJunction, problems) = GradeSeparatedJunction.Add(change, changes.Provenance, idGenerator, idTranslator);
        if (problems.HasError())
        {
            return problems;
        }

        _gradeSeparatedJunctions.Add(gradeSeparatedJunction!.GradeSeparatedJunctionId, gradeSeparatedJunction);
        summary.Added.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);

        return problems;
    }

    private Problems ModifyGradeSeparatedJunction(RoadNetworkChanges changes, ModifyGradeSeparatedJunctionChange change, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
    {
        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var junction))
        {
            return Problems.Single(new GradeSeparatedJunctionNotFound(change.GradeSeparatedJunctionId));
        }

        var problems = junction.Modify(change, changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(junction.GradeSeparatedJunctionId);
        return problems;
    }

    private Problems RemoveGradeSeparatedJunction(RoadNetworkChanges changes, RemoveGradeSeparatedJunctionChange change, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
    {
        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var gradeSeparatedJunction))
        {
            return Problems.Single(new GradeSeparatedJunctionNotFound(change.GradeSeparatedJunctionId));
        }

        var problems = gradeSeparatedJunction.Remove(changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);
        return problems;
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
}

public sealed class UniqueList<T> : IReadOnlyCollection<T>
{
    private readonly HashSet<T> _set = [];
    private readonly List<T> _list = [];

    public void Add(T item)
    {
        if (_set.Add(item))
        {
            _list.Add(item);
        }
    }

    public int Count => _list.Count;
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
