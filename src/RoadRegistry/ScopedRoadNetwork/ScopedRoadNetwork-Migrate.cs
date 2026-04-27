namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    public RoadNetworkChangeResult Migrate(RoadNetworkChanges changes, DownloadId? downloadId, IRoadNetworkIdGenerator idGenerator, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var summary = new RoadNetworkChangesSummary();
        var idTranslator = new IdentifierTranslator();
        var context = new ScopedRoadNetworkContext(this, idTranslator, changes.Provenance, logger);

        var problems = ApplyMigrateChanges(changes, idGenerator, context, summary);

        if (!problems.HasError())
        {
            problems += AfterChangesApplied(idGenerator, context, summary);
        }

        if (!problems.HasError() && changes.Any())
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

    private Problems ApplyMigrateChanges(RoadNetworkChanges changes, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        using var _ = context.Logger.TimeAction();

        if (changes.Count == 0)
        {
            return Problems.Single(new Error(ProblemCode.RoadNetwork.NoChanges.ToString()));
        }

        // Ensure spatial indexes are built once at the start for optimal performance
        RebuildSpatialIndexes(context.Logger);

        var roadSegmentRoadNumberChanges = GetRoadSegmentRoadNumberChanges(changes);

        var problems = Problems.None;

        // Track timing per action type
        var actionTimings = new Dictionary<string, long>();
        var actionCounts = new Dictionary<string, int>();

        var totalChanges = changes.Count;
        var processedChanges = 0;
        var lastLoggedProgress = 0;
        const int progressLogInterval = 10; // Log progress every 1000 changes

        foreach (var roadNetworkChange in changes)
        {
            var actionName = roadNetworkChange.GetType().Name;
            var stopwatch = Stopwatch.StartNew();

            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems += AddRoadNode(change, idGenerator, context, summary);
                    break;
                case ModifyRoadNodeChange change:
                    problems += MigrateRoadNode(change, context, summary);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNodeBecauseOfMigration(change, context, summary);
                    break;

                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(change, idGenerator, context, summary);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += MigrateRoadSegment(change, roadSegmentRoadNumberChanges, context, summary);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RemoveRoadSegmentBecauseOfMigration(change.RoadSegmentId, context, summary);
                    break;

                case AddRoadSegmentToEuropeanRoadChange:
                case RemoveRoadSegmentFromEuropeanRoadChange:
                case AddRoadSegmentToNationalRoadChange:
                case RemoveRoadSegmentFromNationalRoadChange:
                    // Handled in MigrateRoadSegment
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(change, idGenerator, context, summary);
                    break;
                case RemoveGradeSeparatedJunctionChange change:
                    problems += RemoveGradeSeparatedJunctionBecauseOfMigration(change, context, summary);
                    break;

                default:
                    throw new NotImplementedException($"{roadNetworkChange.GetType().Name} is not implemented.");
            }

            stopwatch.Stop();

            // Accumulate timing data
            actionTimings.TryAdd(actionName, 0);
            actionCounts.TryAdd(actionName, 0);
            actionTimings[actionName] += stopwatch.ElapsedMilliseconds;
            actionCounts[actionName]++;

            processedChanges++;

            // Log progress periodically to survive Lambda timeout
            if (processedChanges - lastLoggedProgress >= progressLogInterval)
            {
                var progressPercent = (processedChanges * 100.0) / totalChanges;
                var logMessage = $"Migration progress: {processedChanges}/{totalChanges} ({progressPercent:F1}%)";

                context.Logger.LogWarning(logMessage);
                Console.WriteLine(logMessage);

                lastLoggedProgress = processedChanges;

                {
                    // Log final timing summary
                    var sb = new StringBuilder();
                    sb.AppendLine("Action timings:");
                    foreach (var actionName2 in actionTimings.Keys.OrderByDescending(k => actionTimings[k]))
                    {
                        var totalMs = actionTimings[actionName2];
                        var count = actionCounts[actionName2];
                        var avgMs = count > 0 ? totalMs / (double)count : 0;
                        sb.AppendLine($"  {actionName2}: {count} actions, {totalMs}ms total, {avgMs:F2}ms avg");
                    }
                    context.Logger.LogWarning(sb.ToString());
                    Console.WriteLine(sb.ToString());
                }
            }
        }

        return problems;
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

    private Problems MigrateRoadNode(ModifyRoadNodeChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
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

        var oldEnvelope = roadNode.Geometry.Value.EnvelopeInternal;
        problems += roadNode.Migrate(migrateChange, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodesSpatialIndex.Update(oldEnvelope, roadNode.Geometry.Value.EnvelopeInternal, roadNode);
        summary.RoadNodes.Modified.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems RemoveRoadNodeBecauseOfMigration(RemoveRoadNodeChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.WithContext(change.RoadNodeId);

        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return problems + new RoadNodeNotFound();
        }

        problems += roadNode.RemoveBecauseOfMigration(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodesSpatialIndex.Remove(roadNode.Geometry.Value.EnvelopeInternal, roadNode);
        summary.RoadNodes.Removed.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems MigrateRoadSegment(ModifyRoadSegmentChange change, ILookup<RoadSegmentId, IRoadNetworkChange> roadSegmentRoadNumberChanges, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
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

        var oldEnvelope = roadSegment.Geometry.Value.EnvelopeInternal;
        problems += roadSegment.Migrate(migrateChange, context);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegmentsSpatialIndex.Update(oldEnvelope, roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        summary.RoadSegments.Modified.Add(roadSegment.RoadSegmentId);

        problems += TryToRemoveLinkedGradeJunctions(roadSegment.RoadSegmentId, context, summary);

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

    private Problems RemoveRoadSegmentBecauseOfMigration(RoadSegmentId roadSegmentId, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.WithContext(roadSegmentId);

        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return problems + new RoadSegmentNotFound();
        }

        problems += roadSegment.RemoveBecauseOfMigration(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegmentsSpatialIndex.Remove(roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        summary.RoadSegments.Removed.Add(roadSegment.RoadSegmentId);

        return problems;
    }

    private Problems RemoveGradeSeparatedJunctionBecauseOfMigration(RemoveGradeSeparatedJunctionChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.WithContext(change.GradeSeparatedJunctionId);

        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var gradeSeparatedJunction))
        {
            return problems + new GradeSeparatedJunctionNotFound();
        }

        problems += gradeSeparatedJunction.RemoveBecauseOfMigration(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.GradeSeparatedJunctions.Removed.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);
        return problems;
    }
}
