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

        var context = new ScopedRoadNetworkChangeContext(this, changes.Provenance, logger);

        RegisterOriginalRoadNodeIds(changes, context);

        var problems = ApplyMigrateChanges(changes, idGenerator, context);

        if (!problems.HasError())
        {
            problems += AfterChangesApplied(idGenerator, context, RoadSegmentMerging.AsInwinning);
        }

        if (!problems.HasError())
        {
            foreach (var gradeSeparatedJunction in _gradeSeparatedJunctions.Values.Where(x => x.HasChanges()))
            {
                gradeSeparatedJunction.EnsureGeometryFollowsMigration(context.Provenance);
            }
        }

        if (!problems.HasError() && changes.Any())
        {
            Apply(new RoadNetworkWasChanged
            {
                RoadNetworkId = RoadNetworkId,
                ScopeGeometry = changes.BuildScopeGeometry()?.ToGeometryObject(),
                DownloadId = downloadId,
                Summary = new RoadNetworkChangedSummary(context.Summary),
                Provenance = new ProvenanceData(changes.Provenance)
            });
        }

        return new RoadNetworkChangeResult(WithOriginalRoadNodeIds(Problems.None.AddRange(problems.Distinct()), context), context.Summary);
    }

    private Problems ApplyMigrateChanges(RoadNetworkChanges changes, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        using var _ = context.Logger.TimeAction();

        if (changes.Count == 0)
        {
            return Problems.Single(new Error(ProblemCode.RoadNetwork.NoChanges.ToString()));
        }

        // Ensure spatial indexes are built once at the start for optimal performance
        RebuildSpatialIndexes(context.Logger);

        var roadSegmentRoadNumberChanges = GetRoadSegmentRoadNumberChanges(changes);

        // Worked out up front: what a neighbouring inwinning already merged away decides how several of the changes
        // below are to be read, and the road nodes it concerns are handled before the road segments that run to them.
        var neighbourMerges = ResolveNeighbourMerges(changes, context);

        // And which road nodes a neighbouring inwinning removed outright rather than merged away, so they can be
        // collected again. Asked after the merges and given their answer: a road node those account for is not this.
        var removalCorrections = ResolveRemovalCorrections(changes, neighbourMerges, context);

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
                    problems += AddRoadNode(change, idGenerator, context);
                    break;
                case ModifyRoadNodeChange change:
                    problems += MigrateRoadNode(change, neighbourMerges, removalCorrections, context);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNodeBecauseOfMigration(change, neighbourMerges, context);
                    break;

                case AddRoadSegmentChange change:
                    problems += MigrateAddedRoadSegment(change, neighbourMerges, idGenerator, context);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += MigrateRoadSegment(change, roadSegmentRoadNumberChanges, neighbourMerges, context);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RemoveRoadSegmentBecauseOfMigration(change.RoadSegmentId, neighbourMerges, context);
                    break;

                case AddRoadSegmentToEuropeanRoadChange:
                case RemoveRoadSegmentFromEuropeanRoadChange:
                case AddRoadSegmentToNationalRoadChange:
                case RemoveRoadSegmentFromNationalRoadChange:
                    // Handled in MigrateRoadSegment
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(change, idGenerator, context);
                    break;
                case ModifyGradeSeparatedJunctionChange change:
                    problems += MigrateGradeSeparatedJunction(change, context);
                    break;
                case RemoveGradeSeparatedJunctionChange change:
                    problems += RemoveGradeSeparatedJunctionBecauseOfMigration(change, context);
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

                context.Logger.LogInformation(logMessage);
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
                    context.Logger.LogInformation(sb.ToString());
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

    private Problems MigrateRoadNode(ModifyRoadNodeChange change, NeighbourMergeResolutions neighbourMerges, IReadOnlyCollection<RoadNodeId> removalCorrections, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(change.RoadNodeId);

        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return problems + new RoadNodeNotFound();
        }

        // A road node that was removed is loaded along with the rest of the road network, so it is still here to be
        // named - which is a different thing to say than that it was never found.
        if (roadNode.IsRemoved)
        {
            // A road node a neighbouring inwinning merged away is not this upload's mistake: it was prepared against
            // an extract from before that merge. The roads that ran up to it are cut back to match.
            if (neighbourMerges.RoadNodeIds.Contains(change.RoadNodeId))
            {
                return Problems.None;
            }

            if (!removalCorrections.Contains(change.RoadNodeId))
            {
                return problems + new RoadNodeIsRemoved();
            }

            // A road node a neighbouring inwinning removed outright, and that this upload brings back together with a
            // road of its own that runs to it. It keeps its identifier and is collected from scratch below.
            problems += CorrectRoadNodeRemovalBecauseOfMigration(roadNode, context);
            if (problems.HasError())
            {
                return problems;
            }
        }

        if (roadNode.HasMigrated())
        {
            return ModifyRoadNode(change, context);
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
        context.Summary.RoadNodes.Modified.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems CorrectRoadNodeRemovalBecauseOfMigration(RoadNode.RoadNode roadNode, ScopedRoadNetworkChangeContext context)
    {
        var problems = roadNode.CorrectRemovalBecauseOfMigration(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        // Back into the spatial index by hand. The indexes are built from what is not removed, so a road node that
        // was is simply not in there, and the Update that follows its collection would not put it there either - it
        // does nothing at all when the geometry is unchanged, which for a road node brought back as it was is the
        // normal case. Without this the roads running to it would not find their start and end node.
        _roadNodesSpatialIndex.Insert(roadNode.Geometry.Value.EnvelopeInternal, roadNode);

        return problems;
    }

    private Problems RemoveRoadNodeBecauseOfMigration(RemoveRoadNodeChange change, NeighbourMergeResolutions neighbourMerges, ScopedRoadNetworkChangeContext context)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode) || roadNode.IsRemoved)
        {
            return Problems.None;
        }

        // This upload no longer needed the road node because it was merging the two roads that met there, but a
        // neighbouring inwinning had already collected one of them and the merge was cut back to what it left. What
        // is left runs up to this road node, so it stays.
        if (neighbourMerges.RoadNodeIdsToKeep.Contains(change.RoadNodeId))
        {
            return Problems.None;
        }

        var problems = roadNode.RemoveBecauseOfMigration(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodesSpatialIndex.Remove(roadNode.Geometry.Value.EnvelopeInternal, roadNode);
        context.Summary.RoadNodes.Removed.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems MigrateRoadSegment(ModifyRoadSegmentChange change, ILookup<RoadSegmentId, IRoadNetworkChange> roadSegmentRoadNumberChanges, NeighbourMergeResolutions neighbourMerges, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(change.RoadSegmentIdReference);

        if (neighbourMerges.RoadSegments.TryGetValue(change.RoadSegmentIdReference, out var neighbourMergeCuts))
        {
            // A neighbouring inwinning already collected this road, in whole or in part. What it took is not this
            // upload's to collect a second time. There can be more than one such stretch - a road running up to a
            // road node the neighbour merged away at either end is cut at both - and each cut was measured against
            // what the one before it left, so they are made in that same order.
            foreach (var neighbourMerge in neighbourMergeCuts)
            {
                if (neighbourMerge.TakenInFull)
                {
                    return Problems.None;
                }

                change = CutBackToRemainingStretch(change, neighbourMerge);
            }
        }

        if (!_roadSegments.TryGetValue(change.RoadSegmentIdReference.RoadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return problems + (roadSegment is null ? new RoadSegmentNotFound() : (Error)new RoadSegmentIsRemoved());
        }

        if (roadSegment.HasMigrated())
        {
            return ModifyRoadSegment(change, context);
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
            CarTrafficDirection = change.CarTrafficDirection!,
            BikeTrafficDirection = change.BikeTrafficDirection!,
            PedestrianTrafficDirection = change.PedestrianTrafficDirection!,
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
        context.Summary.RoadSegments.Modified.Add(roadSegment.RoadSegmentId);

        problems += TryToRemoveLinkedGradeJunctions(roadSegment.RoadSegmentId, context);

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

    // A road the uploader collected as one out of several the road network has arrives as a road drawn anew, with
    // the ones it replaces removed. Such a road runs up to a road node a neighbouring inwinning merged away just as
    // much as one this upload modifies, and is cut back to what the neighbour left in the same way (GAWR-7826).
    private Problems MigrateAddedRoadSegment(AddRoadSegmentChange change, NeighbourMergeResolutions neighbourMerges, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        if (neighbourMerges.RoadSegments.TryGetValue(change.RoadSegmentIdReference, out var neighbourMergeCuts))
        {
            foreach (var neighbourMerge in neighbourMergeCuts)
            {
                if (neighbourMerge.TakenInFull)
                {
                    // Collected by the neighbour from end to end: there is nothing here for this upload to draw.
                    return Problems.None;
                }

                change = CutBackToRemainingStretch(change, neighbourMerge);
            }
        }

        return AddRoadSegment(change, idGenerator, context);
    }

    private Problems RemoveRoadSegmentBecauseOfMigration(RoadSegmentId roadSegmentId, NeighbourMergeResolutions neighbourMerges, ScopedRoadNetworkChangeContext context)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return Problems.None;
        }

        // This upload is replacing the road segment with a road of its own, but a neighbouring inwinning had
        // already collected it and the replacement was cut back to what the neighbour left. What is on record is
        // the neighbour's work over a stretch this upload no longer covers, so there is nothing here to remove.
        if (neighbourMerges.RoadSegmentIdsToKeep.Contains(roadSegmentId))
        {
            return Problems.None;
        }

        var problems = roadSegment.RemoveBecauseOfMigration(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegmentsSpatialIndex.Remove(roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        context.Summary.RoadSegments.Removed.Add(roadSegment.RoadSegmentId);

        return problems;
    }

    private Problems MigrateGradeSeparatedJunction(ModifyGradeSeparatedJunctionChange change, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(change.GradeSeparatedJunctionId);

        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var gradeSeparatedJunction) || gradeSeparatedJunction.IsRemoved)
        {
            return problems + (gradeSeparatedJunction is null
                ? new GradeSeparatedJunctionNotFound()
                : (Error)new GradeSeparatedJunctionIsRemoved(change.GradeSeparatedJunctionId));
        }

        if (gradeSeparatedJunction.HasMigrated())
        {
            return problems;
        }

        var migrateChange = new MigrateGradeSeparatedJunctionChange
        {
            GradeSeparatedJunctionId = change.GradeSeparatedJunctionId,
            // FeatureCompare has validated the road segments and the type already; a migration without them is a bug
            LowerRoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.LowerRoadSegmentId ?? throw new InvalidOperationException($"{nameof(change.LowerRoadSegmentId)} is required to migrate grade separated junction {change.GradeSeparatedJunctionId}.")),
            UpperRoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.UpperRoadSegmentId ?? throw new InvalidOperationException($"{nameof(change.UpperRoadSegmentId)} is required to migrate grade separated junction {change.GradeSeparatedJunctionId}.")),
            Type = change.Type ?? throw new InvalidOperationException($"{nameof(change.Type)} is required to migrate grade separated junction {change.GradeSeparatedJunctionId}.")
        };

        problems += gradeSeparatedJunction.Migrate(migrateChange, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        context.Summary.GradeSeparatedJunctions.Modified.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);
        return problems;
    }

    private Problems RemoveGradeSeparatedJunctionBecauseOfMigration(RemoveGradeSeparatedJunctionChange change, ScopedRoadNetworkChangeContext context)
    {
        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var gradeSeparatedJunction) || gradeSeparatedJunction.IsRemoved)
        {
            return Problems.None;
        }

        var problems = gradeSeparatedJunction.RemoveBecauseOfMigration(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        context.Summary.GradeSeparatedJunctions.Removed.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);
        return problems;
    }
}
