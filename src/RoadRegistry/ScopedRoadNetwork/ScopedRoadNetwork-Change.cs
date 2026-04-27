namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Index.Strtree;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;

public partial class ScopedRoadNetwork
{
    public RoadNetworkChangeResult Change(RoadNetworkChanges changes, DownloadId? downloadId, IRoadNetworkIdGenerator idGenerator, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        // Ensure spatial indexes are built once at the start for optimal performance
        RebuildSpatialIndexes();

        var summary = new RoadNetworkChangesSummary();
        var idTranslator = new IdentifierTranslator();
        var context = new ScopedRoadNetworkContext(this, idTranslator, changes.Provenance, logger);

        var problems = ApplyChanges(changes, idGenerator, context, summary);

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

    private Problems ApplyChanges(RoadNetworkChanges changes, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        using var _ = context.Logger.TimeAction();

        var problems = Problems.None;

        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems += AddRoadNode(change, idGenerator, context, summary);
                    break;
                case ModifyRoadNodeChange change:
                    problems += ModifyRoadNode(change, context, summary);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNode(change, context, summary);
                    break;

                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(change, idGenerator, context, summary);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += ModifyRoadSegment(change, context, summary);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RemoveRoadSegment(change.RoadSegmentId, context, summary);
                    break;
                case AddRoadSegmentToEuropeanRoadChange change:
                    problems += ModifyRoadSegmentRoads(change.RoadSegmentId, segment =>
                        segment.AddEuropeanRoad(change with
                        {
                            RoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary);
                    break;
                case RemoveRoadSegmentFromEuropeanRoadChange change:
                    problems += ModifyRoadSegmentRoads(change.RoadSegmentId, segment =>
                        segment.RemoveEuropeanRoad(change with
                        {
                            RoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary);
                    break;
                case AddRoadSegmentToNationalRoadChange change:
                    problems += ModifyRoadSegmentRoads(change.RoadSegmentId, segment =>
                        segment.AddNationalRoad(change with
                        {
                            RoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary);
                    break;
                case RemoveRoadSegmentFromNationalRoadChange change:
                    problems += ModifyRoadSegmentRoads(change.RoadSegmentId, segment =>
                        segment.RemoveNationalRoad(change with
                        {
                            RoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary);
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(change, idGenerator, context, summary);
                    break;
                case ModifyGradeSeparatedJunctionChange change:
                    problems += ModifyGradeSeparatedJunction(change, context, summary);
                    break;
                case RemoveGradeSeparatedJunctionChange change:
                    problems += RemoveGradeSeparatedJunction(change, context, summary);
                    break;

                default:
                    throw new NotImplementedException($"{roadNetworkChange.GetType().Name} is not implemented.");
            }
        }

        return problems;
    }

    private Problems AfterChangesApplied(IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = VerifyRoadNodesAndUpdateTypeAfterChange(idGenerator, context);

        problems = problems
                   + VerifyRoadSegmentsAfterChange(context)
                   + VerifyGradeSeparatedJunctionsAfterChange(context);

        if (!problems.HasError())
        {
            problems += VerifyAndUpdateJunctions(idGenerator, context, summary);
        }

        return problems;
    }

    private Problems VerifyRoadNodesAndUpdateTypeAfterChange(IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context)
    {
        using var _ = context.Logger.TimeAction();

        var problems = Problems.None;

        var roadNodes = _roadNodes.Values.Where(x => x.HasChanges()).Select(x => x.RoadNodeId)
            .Concat(_roadSegments.Values.Where(x => x.HasChanges()).SelectMany(x => x.GetNodeIds()))
            .Distinct()
            .Select(x => _roadNodes[x])
            .ToArray();

        return roadNodes
            .Aggregate(problems, (p, x) => p + x.VerifyTopologyAndUpdateType(_roadSegmentsSpatialIndex, idGenerator, context));
    }

    private Problems VerifyRoadSegmentsAfterChange(ScopedRoadNetworkContext context)
    {
        using var _ = context.Logger.TimeAction();

        return _roadSegments.Values
            .Where(x => x.HasChanges())
            .AsParallel()
            .Aggregate(
                Problems.None,
                (p, x) => p + x.VerifyTopology(context),
                (p1, p2) => p1 + p2,
                finalResult => finalResult);
    }

    private Problems VerifyGradeSeparatedJunctionsAfterChange(ScopedRoadNetworkContext context)
    {
        using var _ = context.Logger.TimeAction();

        var problems = Problems.None;

        var changedJunctions = _gradeSeparatedJunctions.Values
            .Where(x => x.HasChanges())
            .ToList();

        // Optimize: Use Dictionary for O(n) duplicate detection instead of O(n log n) GroupBy
        var junctionsBySegmentPair = new Dictionary<(int Min, int Max), List<GradeSeparatedJunction>>();

        foreach (var junction in context.RoadNetwork.GradeSeparatedJunctions.Values.Where(x => !x.IsRemoved))
        {
            var key = (
                Math.Min(junction.LowerRoadSegmentId.ToInt32(), junction.UpperRoadSegmentId.ToInt32()),
                Math.Max(junction.LowerRoadSegmentId.ToInt32(), junction.UpperRoadSegmentId.ToInt32())
            );

            if (!junctionsBySegmentPair.TryGetValue(key, out var list))
            {
                list = [];
                junctionsBySegmentPair[key] = list;
            }
            list.Add(junction);
        }

        // Find duplicates
        foreach (var pair in junctionsBySegmentPair)
        {
            var junctionsList = pair.Value;
            if (junctionsList.Count <= 1)
            {
                continue;
            }

            var firstJunctionId = junctionsList[0].GradeSeparatedJunctionId;

            for (var i = 1; i < junctionsList.Count; i++)
            {
                problems += new GradeSeparatedJunctionNotUnique(firstJunctionId, junctionsList[i].GradeSeparatedJunctionId);
            }
        }

        return changedJunctions
            .AsParallel()
            .Aggregate(
                problems,
                (p, x) => p + x.VerifyTopology(context),
                (p1, p2) => p1 + p2,
                finalResult => finalResult);
    }

    private Problems AddRoadNode(AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var (roadNode, problems) = RoadNode.Add(change, context.Provenance, idGenerator);
        if (problems.HasError())
        {
            return problems;
        }

        problems += context.IdTranslator.RegisterMapping(change.OriginalId ?? change.TemporaryId, roadNode!.RoadNodeId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodes.Add(roadNode.RoadNodeId, roadNode);
        _roadNodesSpatialIndex.Insert(roadNode.Geometry.Value.EnvelopeInternal, roadNode);
        summary.RoadNodes.Added.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems ModifyRoadNode(ModifyRoadNodeChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.RoadNodeId));
        }

        var oldEnvelope = roadNode.Geometry.Value.EnvelopeInternal;
        var problems = roadNode.Modify(change, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodesSpatialIndex.Update(oldEnvelope, roadNode.Geometry.Value.EnvelopeInternal, roadNode);
        summary.RoadNodes.Modified.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems RemoveRoadNode(RemoveRoadNodeChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.None;
        }

        var problems = roadNode.Remove(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodesSpatialIndex.Remove(roadNode.Geometry.Value.EnvelopeInternal, roadNode);
        summary.RoadNodes.Removed.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems AddRoadSegment(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var (roadSegment, problems) = RoadSegment.Add(change, idGenerator, context);
        if (problems.HasError())
        {
            return problems;
        }

        problems += context.IdTranslator.RegisterMapping(change.RoadSegmentIdReference, roadSegment!.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);
        _roadSegmentsSpatialIndex.Insert(roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        summary.RoadSegments.Added.Add(roadSegment.RoadSegmentId);

        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.WithContext(change.RoadSegmentIdReference);

        if (!_roadSegments.TryGetValue(change.RoadSegmentIdReference.RoadSegmentId, out var roadSegment))
        {
            return problems + new RoadSegmentNotFound();
        }

        problems += context.IdTranslator.RegisterMapping(change.RoadSegmentIdReference, roadSegment.RoadSegmentId);
        if (problems.HasError())
        {
            return problems;
        }

        var oldEnvelope = roadSegment.Geometry.Value.EnvelopeInternal;
        problems = roadSegment.Modify(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegmentsSpatialIndex.Update(oldEnvelope, roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        summary.RoadSegments.Modified.Add(roadSegment.RoadSegmentId);

        problems += TryToRemoveLinkedGradeJunctions(roadSegment.RoadSegmentId, context, summary);

        return problems;
    }

    private Problems ModifyRoadSegmentRoads(RoadSegmentId roadSegmentId, Func<RoadSegment, Problems> modify, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.WithContext(roadSegmentId);

        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return problems + new RoadSegmentNotFound();
        }

        var oldEnvelope = roadSegment.Geometry.Value.EnvelopeInternal;
        problems += modify(roadSegment);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegmentsSpatialIndex.Update(oldEnvelope, roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        summary.RoadSegments.Modified.Add(roadSegmentId);
        return problems;
    }

    private Problems RemoveRoadSegment(RoadSegmentId roadSegmentId, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return Problems.None;
        }

        var problems = roadSegment.Remove(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        _roadSegmentsSpatialIndex.Remove(roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        summary.RoadSegments.Removed.Add(roadSegment.RoadSegmentId);

        problems += TryToRemoveLinkedGradeJunctions(roadSegmentId, context, summary);

        return problems;
    }

    private Problems TryToRemoveLinkedGradeJunctions(RoadSegmentId roadSegmentId, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.None;

        var roadSegment = _roadSegments[roadSegmentId];
        if (roadSegment.IsRemoved || roadSegment.Attributes?.Status != RoadSegmentStatusV2.Gerealiseerd)
        {
            var linkedGradeJunctions = _gradeJunctions
                .Where(x => !x.Value.IsRemoved && x.Value.IsConnectedTo(roadSegmentId))
                .Select(x => x.Value)
                .ToArray();
            foreach (var gradeJunction in linkedGradeJunctions)
            {
                problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context, summary);
            }
        }

        return problems;
    }

    private Problems AddGradeSeparatedJunction(AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        change = change with
        {
            LowerRoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.LowerRoadSegmentId),
            UpperRoadSegmentId = context.IdTranslator.TranslateToPermanentId(change.UpperRoadSegmentId)
        };

        var (gradeSeparatedJunction, problems) = GradeSeparatedJunction.Add(change, context.Provenance, idGenerator);
        if (problems.HasError())
        {
            return problems;
        }

        _gradeSeparatedJunctions.Add(gradeSeparatedJunction!.GradeSeparatedJunctionId, gradeSeparatedJunction);
        summary.GradeSeparatedJunctions.Added.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);

        return problems;
    }

    private Problems ModifyGradeSeparatedJunction(ModifyGradeSeparatedJunctionChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.WithContext(change.GradeSeparatedJunctionId);

        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var junction))
        {
            return problems + new GradeSeparatedJunctionNotFound();
        }

        change = change with
        {
            LowerRoadSegmentId = change.LowerRoadSegmentId is not null
                ? context.IdTranslator.TranslateToPermanentId(change.LowerRoadSegmentId.Value)
                : null,
            UpperRoadSegmentId = change.UpperRoadSegmentId is not null
                ? context.IdTranslator.TranslateToPermanentId(change.UpperRoadSegmentId.Value)
                : null
        };

        problems += junction.Modify(change, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.GradeSeparatedJunctions.Modified.Add(junction.GradeSeparatedJunctionId);
        return problems;
    }

    private Problems RemoveGradeSeparatedJunction(RemoveGradeSeparatedJunctionChange change, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var gradeSeparatedJunction))
        {
            return Problems.None;
        }

        var problems = gradeSeparatedJunction.Remove(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.GradeSeparatedJunctions.Removed.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);
        return problems;
    }

    private Problems AddGradeJunction(
        RoadSegmentId roadSegmentId1,
        RoadSegmentId roadSegmentId2,
        IRoadNetworkIdGenerator idGenerator,
        ScopedRoadNetworkContext context,
        RoadNetworkChangesSummary summary)
    {
        var gradeJunction = GradeJunction.Add(roadSegmentId1, roadSegmentId2, context.Provenance, idGenerator);

        _gradeJunctions.Add(gradeJunction.GradeJunctionId, gradeJunction);
        summary.GradeJunctions.Added.Add(gradeJunction.GradeJunctionId);

        return Problems.None;
    }

    private Problems RemoveGradeJunction(GradeJunctionId gradeJunctionId, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        if (!_gradeJunctions.TryGetValue(gradeJunctionId, out var gradeJunction))
        {
            return Problems.None;
        }

        var problems = gradeJunction.Remove(context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.GradeJunctions.Removed.Add(gradeJunction.GradeJunctionId);
        return problems;
    }
}
