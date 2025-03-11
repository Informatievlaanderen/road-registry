namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;

public class RemoveRoadSegments : IRequestedChange
{
    private readonly RoadNetworkVersionProvider _roadNetworkVersionProvider;
    private readonly IRoadNetworkIdProvider _roadNetworkIdProvider;
    private readonly IOrganizations _organizations;

    public RemoveRoadSegments(IReadOnlyList<RoadSegmentId> ids,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadNetworkVersionProvider roadNetworkVersionProvider,
        IRoadNetworkIdProvider roadNetworkIdProvider,
        IOrganizations organizations)
    {
        _roadNetworkVersionProvider = roadNetworkVersionProvider;
        _roadNetworkIdProvider = roadNetworkIdProvider;
        _organizations = organizations;
        Ids = ids;
        GeometryDrawMethod = geometryDrawMethod;
    }

    public IReadOnlyList<RoadSegmentId> Ids { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        foreach (var id in Ids)
        {
            if (!context.BeforeView.View.Segments.ContainsKey(id))
            {
                problems += new RoadSegmentNotFound(id);
            }
        }

        return problems;
    }

    private readonly List<RoadNodeId> _removedRoadNodeIds = [];
    private readonly List<RoadNodeTypeChanged> _changedRoadNodes = [];
    private readonly List<GradeSeparatedJunctionId> _removedJunctionIds = [];
    private readonly List<RoadSegmentMerged> _mergedRoadSegments = [];

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _removedRoadNodeIds.Clear();
        _changedRoadNodes.Clear();
        _removedJunctionIds.Clear();
        _mergedRoadSegments.Clear();

        var problems = Problems.None;

        // todo-pr: did we create islands?

        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            return problems;
        }

        var relatedRoadNodes = new List<RoadNodeId>();
        foreach (var id in Ids)
        {
            context.BeforeView.View.Segments.TryGetValue(id, out var segment);
            relatedRoadNodes.Add(segment!.Start);
            relatedRoadNodes.Add(segment!.End);
        }

        relatedRoadNodes = relatedRoadNodes.Distinct().ToList();

        _removedRoadNodeIds.AddRange(relatedRoadNodes.Where(x => !context.AfterView.Nodes.ContainsKey(x)));

        foreach (var roadNodeId in relatedRoadNodes.Except(_removedRoadNodeIds))
        {
            context.BeforeView.Nodes.TryGetValue(roadNodeId, out var roadNodeBefore);
            context.AfterView.Nodes.TryGetValue(roadNodeId, out var roadNodeAfter);

            if (roadNodeBefore!.Type == roadNodeAfter!.Type)
            {
                continue;
            }

            _changedRoadNodes.Add(new RoadNodeTypeChanged
            {
                Id = roadNodeId,
                Type = roadNodeAfter.Type,
                Version = _roadNetworkVersionProvider.NextRoadNodeVersion(roadNodeId)
            });
        }

        var beforeJunctionIds = context.BeforeView.GradeSeparatedJunctions.Keys;
        _removedJunctionIds.AddRange(beforeJunctionIds.Where(x => !context.AfterView.GradeSeparatedJunctions.ContainsKey(x)));

        foreach (var roadSegmentId in context.AfterView.Segments.Keys
                     .Except(context.BeforeView.Segments.Keys))
        {
            context.AfterView.View.Segments.TryGetValue(roadSegmentId, out var segment);

            _mergedRoadSegments.Add(new RoadSegmentMerged
            {
                Id = roadSegmentId,
                Version = RoadSegmentVersion.Initial,
                AccessRestriction = segment!.AttributeHash.AccessRestriction,
                Category = segment.AttributeHash.Category,
                StartNodeId = segment.Start,
                EndNodeId = segment.End,
                Geometry = GeometryTranslator.Translate(segment.Geometry),
                GeometryDrawMethod = segment.AttributeHash.GeometryDrawMethod,
                GeometryVersion = GeometryVersion.Initial,
                Lanes = segment!.Lanes.Select(x => new Messages.RoadSegmentLaneAttributes
                {
                    AttributeId = _roadNetworkIdProvider.NextRoadSegmentLaneAttributeIdProvider(roadSegmentId)().GetAwaiter().GetResult(),
                    Count = x.Count,
                    Direction = x.Direction,
                    FromPosition = x.From,
                    ToPosition = x.To,
                    AsOfGeometryVersion = x.AsOfGeometryVersion
                }).ToArray(),
                LeftSide = new Messages.RoadSegmentSideAttributes { StreetNameId = segment.AttributeHash.LeftStreetNameId },
                RightSide = new Messages.RoadSegmentSideAttributes { StreetNameId = segment.AttributeHash.RightStreetNameId },
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = segment.AttributeHash.OrganizationId,
                    Name = _organizations.FindAsync(segment.AttributeHash.OrganizationId).GetAwaiter().GetResult()?.Translation.Name
                },
                Morphology = segment.AttributeHash.Category,
                Status = segment.AttributeHash.Category,
                Surfaces = segment.Surfaces.Select(x => new Messages.RoadSegmentSurfaceAttributes
                {
                    AttributeId = _roadNetworkIdProvider.NextRoadSegmentSurfaceAttributeIdProvider(roadSegmentId)().GetAwaiter().GetResult(),
                    Type = x.Type,
                    FromPosition = x.From,
                    ToPosition = x.To,
                    AsOfGeometryVersion = x.AsOfGeometryVersion
                })
                    .ToArray(),
                Widths = segment.Widths
                    .Select(x => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = _roadNetworkIdProvider.NextRoadSegmentWidthAttributeIdProvider(roadSegmentId)().GetAwaiter().GetResult(),
                        Width = x.Width,
                        FromPosition = x.From,
                        ToPosition = x.To,
                        AsOfGeometryVersion = x.AsOfGeometryVersion
                    })
                    .ToArray()
            });
        }

        return problems;
    }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RoadSegmentsRemoved = new RoadSegmentsRemoved
        {
            GeometryDrawMethod = GeometryDrawMethod,
            RemovedRoadSegmentIds = Ids.Select(x => x.ToInt32()).ToArray(),
            RemovedRoadNodeIds = _removedRoadNodeIds.Select(x => x.ToInt32()).ToArray(),
            ChangedRoadNodes = _changedRoadNodes.ToArray(),
            RemovedGradeSeparatedJunctionIds = _removedJunctionIds.Select(x => x.ToInt32()).ToArray(),
            MergedRoadSegments = _mergedRoadSegments.ToArray(),
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RemoveRoadSegments = new Messages.RemoveRoadSegments
        {
            Ids = Ids.Select(x => x.ToInt32()).ToArray(),
            GeometryDrawMethod = GeometryDrawMethod
        };
    }
}
