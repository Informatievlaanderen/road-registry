namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;
using RoadNode = RoadNode.RoadNode;
using GradeSeparatedJunction = GradeSeparatedJunction.GradeSeparatedJunction;

public partial class ScopedRoadNetwork
{
    public RoadNetworkChangeResult Change(RoadNetworkChanges changes, DownloadId? downloadId, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;
        var summary = new RoadNetworkChangesSummary();
        var idTranslator = new IdentifierTranslator();

        var context = new ScopedRoadNetworkContext(this, idTranslator, changes.Provenance);

        foreach (var roadNetworkChange in changes)
        {
            switch (roadNetworkChange)
            {
                case AddRoadNodeChange change:
                    problems += AddRoadNode(change, idGenerator, context, summary.RoadNodes);
                    break;
                case ModifyRoadNodeChange change:
                    problems += ModifyRoadNode(change, context, summary.RoadNodes);
                    break;
                case RemoveRoadNodeChange change:
                    problems += RemoveRoadNode(change, context, summary.RoadNodes);
                    break;

                case AddRoadSegmentChange change:
                    problems += AddRoadSegment(change, idGenerator, context, summary.RoadSegments);
                    break;
                case ModifyRoadSegmentChange change:
                    problems += ModifyRoadSegment(change, context, summary.RoadSegments);
                    break;
                case RemoveRoadSegmentChange change:
                    problems += RemoveRoadSegment(change.RoadSegmentId, context, summary.RoadSegments);
                    break;
                case AddRoadSegmentToEuropeanRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment =>
                        segment.AddEuropeanRoad(change with
                        {
                            RoadSegmentId = idTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary.RoadSegments);
                    break;
                case RemoveRoadSegmentFromEuropeanRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment =>
                        segment.RemoveEuropeanRoad(change with
                        {
                            RoadSegmentId = idTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary.RoadSegments);
                    break;
                case AddRoadSegmentToNationalRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment =>
                        segment.AddNationalRoad(change with
                        {
                            RoadSegmentId = idTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary.RoadSegments);
                    break;
                case RemoveRoadSegmentFromNationalRoadChange change:
                    problems += ModifyRoadSegment(change.RoadSegmentId, segment =>
                        segment.RemoveNationalRoad(change with
                        {
                            RoadSegmentId = idTranslator.TranslateToPermanentId(change.RoadSegmentId)
                        }, changes.Provenance), summary.RoadSegments) ;
                    break;

                case AddGradeSeparatedJunctionChange change:
                    problems += AddGradeSeparatedJunction(change, idGenerator, context, summary.GradeSeparatedJunctions) ;
                    break;
                case ModifyGradeSeparatedJunctionChange change:
                    problems += ModifyGradeSeparatedJunction(change, context, summary.GradeSeparatedJunctions);
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

    private Problems VerifyAfterChange(ScopedRoadNetworkContext context)
    {
        return VerifyRoadNodesAfterChange(context)
            + VerifyRoadSegmentsAfterChange(context)
            + VerifyGradeSeparatedJunctionsAfterChange(context);
    }

    private Problems VerifyRoadSegmentsAfterChange(ScopedRoadNetworkContext context)
    {
        var problems = Problems.None;

        return _roadSegments.Values
            .Where(x => x.HasChanges())
            .Aggregate(problems, (p, x) => p + x.VerifyTopology(context));
    }

    private Problems VerifyRoadNodesAfterChange(ScopedRoadNetworkContext context)
    {
        var problems = Problems.None;

        return _roadNodes.Values.Where(x => x.HasChanges()).Select(x => x.RoadNodeId)
            .Concat(_roadSegments.Values.Where(x => x.HasChanges()).SelectMany(x => x.GetNodeIds()))
            .Distinct()
            .Select(x => _roadNodes.GetValueOrDefault(x))
            .Where(x => x is not null)
            .Aggregate(problems, (p, x) => p + x!.VerifyTopologyAndDetectType(context));
    }

    private Problems VerifyGradeSeparatedJunctionsAfterChange(ScopedRoadNetworkContext context)
    {
        var problems = Problems.None;

        var changedJunctions = _gradeSeparatedJunctions.Values
            .Where(x => x.HasChanges())
            .ToList();

        var identicalJunctions = context.RoadNetwork.GradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved)
            .GroupBy(x => new
            {
                Segment1 = Math.Min(x.LowerRoadSegmentId, x.UpperRoadSegmentId),
                Segment2 = Math.Max(x.LowerRoadSegmentId, x.UpperRoadSegmentId)
            })
            .Where(x => x.Count() > 1)
            .ToList();
        foreach (var identicalJunctionGrouping in identicalJunctions)
        {
            var junctionId = identicalJunctionGrouping.First().GradeSeparatedJunctionId;
            var otherIdenticalJunctionIds = identicalJunctionGrouping.Skip(1).Select(x => x.GradeSeparatedJunctionId).ToList();

            foreach (var otherIdenticalJunctionId in otherIdenticalJunctionIds)
            {
                problems += new GradeSeparatedJunctionNotUnique(junctionId, otherIdenticalJunctionId);
            }
        }

        return changedJunctions
            .Aggregate(problems, (p, x) => p + x.VerifyTopology(context));
    }

    private Problems AddRoadNode(AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
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
        summary.Added.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems ModifyRoadNode(ModifyRoadNodeChange change, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.RoadNodeId));
        }

        var problems = roadNode.Modify(change, context.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems RemoveRoadNode(RemoveRoadNodeChange change, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
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

        summary.Removed.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems AddRoadSegment(AddRoadSegmentChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
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
        summary.Added.Add(roadSegment.RoadSegmentId);

        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
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

        problems = roadSegment.Modify(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadSegment.RoadSegmentId);
        return problems;
    }

    private Problems ModifyRoadSegment(RoadSegmentId roadSegmentId, Func<RoadSegment, Problems> modify, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
    {
        var problems = Problems.WithContext(roadSegmentId);

        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment))
        {
            return problems + new RoadSegmentNotFound();
        }

        problems += modify(roadSegment);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadSegmentId);
        return problems;
    }

    private Problems RemoveRoadSegment(RoadSegmentId roadSegmentId, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<RoadSegmentId> summary)
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

        summary.Removed.Add(roadSegment.RoadSegmentId);
        return problems;
    }

    private Problems AddGradeSeparatedJunction(AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
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
        summary.Added.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);

        return problems;
    }

    private Problems ModifyGradeSeparatedJunction(ModifyGradeSeparatedJunctionChange change, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
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

        summary.Modified.Add(junction.GradeSeparatedJunctionId);
        return problems;
    }

    private Problems RemoveGradeSeparatedJunction(RemoveGradeSeparatedJunctionChange change, ScopedRoadNetworkContext context, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
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

        summary.Removed.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);
        return problems;
    }
}
