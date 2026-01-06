namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using GradeSeparatedJunction.Changes;
using RoadNode.Changes;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment.Changes;
using ValueObjects;

public partial class RoadNetwork
{
    public RoadNetworkChangeResult Migrate(RoadNetworkChanges changes, DownloadId? downloadId, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;
        var summary = new RoadNetworkChangesSummary();
        var idTranslator = new IdentifierTranslator();

        var roadSegmentRoadNumberChanges = GetRoadSegmentRoadNumberChanges(changes);

        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems += AddRoadNode(changes, change, idGenerator, idTranslator, summary.RoadNodes);
                    break;
                case ModifyRoadNodeChange change:
                    problems += MigrateRoadNode(changes, change, summary.RoadNodes);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNode(changes, change, summary.RoadNodes);
                    break;

                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(changes, change, idGenerator, idTranslator, summary.RoadSegments);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += MigrateRoadSegment(changes, change, roadSegmentRoadNumberChanges, idTranslator, summary.RoadSegments);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RetireRoadSegmentBecauseOfMigration(changes, change.RoadSegmentId, summary.RoadSegments);
                    break;

                case AddRoadSegmentToEuropeanRoadChange:
                case RemoveRoadSegmentFromEuropeanRoadChange:
                case AddRoadSegmentToNationalRoadChange:
                case RemoveRoadSegmentFromNationalRoadChange:
                    // Handled in MigrateRoadSegment
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(changes, change, idGenerator, idTranslator, summary.GradeSeparatedJunctions) ;
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
            Summary = new RoadNetworkChangedSummary(summary),
            Provenance = new ProvenanceData(changes.Provenance)
        });

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), summary);
    }

    private static ILookup<RoadSegmentId, IRoadNetworkChange> GetRoadSegmentRoadNumberChanges(RoadNetworkChanges changes)
    {
        return changes
            .Select(roadNetworkChange =>
            {
                switch (roadNetworkChange)
                {
                    case AddRoadSegmentToEuropeanRoadChange change:
                        return (change.RoadSegmentId, roadNetworkChange);
                    case RemoveRoadSegmentFromEuropeanRoadChange change:
                        return (change.RoadSegmentId, roadNetworkChange);
                    case AddRoadSegmentToNationalRoadChange change:
                        return (change.RoadSegmentId, roadNetworkChange);
                    case RemoveRoadSegmentFromNationalRoadChange change:
                        return (change.RoadSegmentId, roadNetworkChange);
                }

                return default;
            })
            .Where(x => x.RoadSegmentId > 0)
            .ToLookup(x => x.RoadSegmentId, x => x.roadNetworkChange);
    }

    private Problems MigrateRoadNode(RoadNetworkChanges changes, ModifyRoadNodeChange change, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.RoadNodeId));
        }

        var problems = roadNode.Migrate(change, changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems MigrateRoadSegment(RoadNetworkChanges changes, ModifyRoadSegmentChange change, ILookup<RoadSegmentId, IRoadNetworkChange> roadSegmentRoadNumberChanges, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
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

        var europeanRoadNumbers = MergeEuropeanRoadNumbers(roadSegment.Attributes.EuropeanRoadNumbers, roadSegmentRoadNumberChanges[change.RoadSegmentId]);
        var nationalRoadNumbers = MergeNationalRoadNumbers(roadSegment.Attributes.NationalRoadNumbers, roadSegmentRoadNumberChanges[change.RoadSegmentId]);

        problems = roadSegment.Migrate(change, europeanRoadNumbers, nationalRoadNumbers, changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadSegment.RoadSegmentId);
        return problems;
    }

    private ImmutableList<EuropeanRoadNumber> MergeEuropeanRoadNumbers(IEnumerable<EuropeanRoadNumber> roadSegmentNumbers, IEnumerable<IRoadNetworkChange> roadSegmentNumberChanges)
    {
        var list = roadSegmentNumbers.ToList();

        foreach (var roadNetworkChange in roadSegmentNumberChanges)
        {
            switch (roadNetworkChange)
            {
                case AddRoadSegmentToEuropeanRoadChange change:
                    list.Add(change.Number);
                    break;
                case RemoveRoadSegmentFromEuropeanRoadChange change:
                    list.Remove(change.Number);
                    break;
            }
        }

        return list.Distinct().ToImmutableList();
    }
    private ImmutableList<NationalRoadNumber> MergeNationalRoadNumbers(IEnumerable<NationalRoadNumber> roadSegmentNumbers, IEnumerable<IRoadNetworkChange> roadSegmentNumberChanges)
    {
        var list = roadSegmentNumbers.ToList();

        foreach (var roadNetworkChange in roadSegmentNumberChanges)
        {
            switch (roadNetworkChange)
            {
                case AddRoadSegmentToNationalRoadChange change:
                    list.Add(change.Number);
                    break;
                case RemoveRoadSegmentFromNationalRoadChange change:
                    list.Remove(change.Number);
                    break;
            }
        }

        return list.Distinct().ToImmutableList();
    }

    private Problems RetireRoadSegmentBecauseOfMigration(RoadNetworkChanges changes, RoadSegmentId roadSegmentId, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        //TODO-pr indien segment deel is van een merge dan de nieuwe ID er aan toevoegen
        RoadSegmentId? mergedRoadSegmentId = null;

        var problems = roadSegment.RetireBecauseOfMigration(mergedRoadSegmentId, changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(roadSegment.RoadSegmentId);
        return problems;
    }
}
