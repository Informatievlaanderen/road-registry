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

        //TODO-pr maak automatisch gelijkgrondsekruisingen aan/verwijder ze: (ook in roadnetwork.change)
        /*Voeg gelijkgrondse kruisingen toe op elk punt waar 2 wegsegmenten elkaar kruisen zonder dat er overlap is in de gecapteerde verkeerstypes*.
        Er is overlap in gecapteerde verkeerstypes tussen 2 wegsegmenten A en B wanneer
        (‘auto heen’='1' of ‘auto terug’='1') voor zowel A als B,
        (‘fiets heen’='1' of ‘fiets terug’='1') voor zowel A als B, of
        ‘voetganger’='1' voor zowel A als B.*/

        if (!problems.HasError())
        {
            problems += VerifyAfterChange(context);
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
        var problems = Problems.WithContext(change.RoadNodeId);

        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return problems + new RoadNodeNotFound();
        }

        var migrateChange = new MigrateRoadNodeChange
        {
            RoadNodeId = change.RoadNodeId,
            Geometry = change.Geometry!,
            Grensknoop = change.Grensknoop!.Value,
        };

        problems += roadNode.Migrate(migrateChange, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems MigrateRoadSegment(ModifyRoadSegmentChange change, ILookup<RoadSegmentId, IRoadNetworkChange> roadSegmentRoadNumberChanges, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        var problems = Problems.WithContext(change.RoadSegmentIdReference);

        if (!_roadSegments.TryGetValue(change.RoadSegmentIdReference.RoadSegmentId, out var roadSegment))
        {
            return problems + new RoadSegmentNotFound();
        }

        var europeanRoadNumbers = ReadEuropeanRoadNumbers(roadSegmentRoadNumberChanges[change.RoadSegmentIdReference.RoadSegmentId]);
        var nationalRoadNumbers = ReadNationalRoadNumbers(roadSegmentRoadNumberChanges[change.RoadSegmentIdReference.RoadSegmentId]);

        var migrateChange = new MigrateRoadSegmentChange
        {
            RoadSegmentIdReference = change.RoadSegmentIdReference,
            Geometry = change.Geometry!,
            GeometryDrawMethod = change.GeometryDrawMethod!,
            AccessRestriction = change.AccessRestriction!,
            Category = change.Category!,
            Morphology = change.Morphology!,
            Status = change.Status!,
            StreetNameId = change.StreetNameId!,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId!,
            SurfaceType = change.SurfaceType!,
            CarAccessForward = change.CarAccessForward!,
            CarAccessBackward = change.CarAccessBackward!,
            BikeAccessForward = change.BikeAccessForward!,
            BikeAccessBackward = change.BikeAccessBackward!,
            PedestrianAccess = change.PedestrianAccess!,
            EuropeanRoadNumbers = europeanRoadNumbers,
            NationalRoadNumbers = nationalRoadNumbers
        };

        problems += roadSegment.Migrate(migrateChange, context);
        if (problems.HasError())
        {
            return problems;
        }

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
        var problems = Problems.WithContext(roadSegmentId);

        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return problems + new RoadSegmentNotFound();
        }

        //TODO-pr indien segment deel is van een merge dan de nieuwe ID er aan toevoegen
        RoadSegmentId? mergedRoadSegmentId = null;

        problems += roadSegment.RetireBecauseOfMigration(mergedRoadSegmentId, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(roadSegment.RoadSegmentId);
        return problems;
    }
}
