namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public partial class ScopedRoadNetwork
{
    public RoadNetworkChangeResult Migrate(RoadNetworkChanges changes, DownloadId? downloadId, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;
        var summary = new RoadNetworkChangesSummary();
        var idTranslator = new IdentifierTranslator();

        if (!changes.Any())
        {
            problems += Problems.Single(new Error(ProblemCode.RoadNetwork.NoChanges.ToString()));
        }

        var roadSegmentRoadNumberChanges = GetRoadSegmentRoadNumberChanges(changes);
        var context = new ScopedRoadNetworkContext(this, idTranslator, changes.Provenance);

        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems += AddRoadNode(change, idGenerator, context, summary.RoadNodes);
                    break;
                case ModifyRoadNodeChange change:
                    problems += MigrateRoadNode(change, context, summary.RoadNodes);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNode(change, context, summary.RoadNodes);
                    break;

                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(change, idGenerator, context, summary.RoadSegments);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += MigrateRoadSegment(change, roadSegmentRoadNumberChanges, context, summary.RoadSegments);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RetireRoadSegmentBecauseOfMigration(change.RoadSegmentId, context, summary.RoadSegments);
                    break;

                case AddRoadSegmentToEuropeanRoadChange:
                case RemoveRoadSegmentFromEuropeanRoadChange:
                case AddRoadSegmentToNationalRoadChange:
                case RemoveRoadSegmentFromNationalRoadChange:
                    // Handled in MigrateRoadSegment
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(change, idGenerator, context, summary.GradeSeparatedJunctions);
                    break;
                case RemoveGradeSeparatedJunctionChange change:
                    problems += RemoveGradeSeparatedJunction(change, context, summary.GradeSeparatedJunctions);
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
                .Aggregate(problems, (p, x) => p + x!.VerifyTopologyAndDetectType(context));
            problems = _roadSegments.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopology(context));
            problems = _gradeSeparatedJunctions.Values
                .Where(x => x.HasChanges())
                .Aggregate(problems, (p, x) => p + x.VerifyTopology(context));
        }

        if (changes.Any())
        {
            Apply(new RoadNetworkWasChanged
            {
                RoadNetworkId = RoadNetworkId,
                ScopeGeometry = changes.BuildScopeGeometry()?.ToGeometryObject(),
                DownloadId = downloadId,
                Summary = new RoadNetworkChangedSummary(summary),
                Provenance = new ProvenanceData(changes.Provenance)
            });
        }

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

    private Problems MigrateRoadNode(ModifyRoadNodeChange change, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        var migrateChange = new MigrateRoadNodeChange
        {
            RoadNodeId = change.RoadNodeId,
            Geometry = change.Geometry!,
            Grensknoop = change.Grensknoop!.Value,
        };

        var (roadNode, problems) = RoadNode.Migrate(migrateChange, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        problems += context.IdTranslator.RegisterMapping(change.RoadNodeId, roadNode!.RoadNodeId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodes.Add(roadNode.RoadNodeId, roadNode);
        summary.Modified.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems MigrateRoadSegment(ModifyRoadSegmentChange change, ILookup<RoadSegmentId, IRoadNetworkChange> roadSegmentRoadNumberChanges, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        var originalId = change.OriginalId ?? change.RoadSegmentId;

        var europeanRoadNumbers = ReadEuropeanRoadNumbers(roadSegmentRoadNumberChanges[change.RoadSegmentId]);
        var nationalRoadNumbers = ReadNationalRoadNumbers(roadSegmentRoadNumberChanges[change.RoadSegmentId]);

        var migrateChange = new MigrateRoadSegmentChange
        {
            RoadSegmentId = change.RoadSegmentId,
            OriginalId = change.OriginalId,
            Geometry = change.Geometry!,
            GeometryDrawMethod = change.GeometryDrawMethod!,
            AccessRestriction = change.AccessRestriction!,
            Category = change.Category!,
            Morphology = change.Morphology!,
            Status = change.Status!,
            StreetNameId = change.StreetNameId!,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId!,
            SurfaceType = change.SurfaceType!,
            CarAccess = change.CarAccess!,
            BikeAccess = change.BikeAccess!,
            PedestrianAccess = change.PedestrianAccess!,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers
        };

        var (roadSegment, problems) = RoadSegment.Migrate(migrateChange, context);
        if (problems.HasError())
        {
            return problems;
        }

        problems += context.IdTranslator.RegisterMapping(originalId, roadSegment!.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);
        summary.Modified.Add(roadSegment.RoadSegmentId);

        return problems;
    }

    private IReadOnlyCollection<EuropeanRoadNumber> ReadEuropeanRoadNumbers(IEnumerable<IRoadNetworkChange> roadSegmentNumberChanges)
    {
        var list = new List<EuropeanRoadNumber>();

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

    private IReadOnlyCollection<NationalRoadNumber> ReadNationalRoadNumbers(IEnumerable<IRoadNetworkChange> roadSegmentNumberChanges)
    {
        var list = new List<NationalRoadNumber>();

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

    private Problems RetireRoadSegmentBecauseOfMigration(RoadSegmentId roadSegmentId, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        //TODO-pr indien segment deel is van een merge dan de nieuwe ID er aan toevoegen
        RoadSegmentId? mergedRoadSegmentId = null;

        var problems = roadSegment.RetireBecauseOfMigration(mergedRoadSegmentId, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(roadSegment.RoadSegmentId);
        return problems;
    }
}
