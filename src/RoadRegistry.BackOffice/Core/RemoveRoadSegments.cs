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

    private RoadSegmentsRemoved _acceptedChange;
    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        // todo-pr: did we create islands?
        _acceptedChange = BuildAcceptedChange(context);

        return problems;
    }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RoadSegmentsRemoved = _acceptedChange;
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

    private RoadSegmentsRemoved BuildAcceptedChange(AfterVerificationContext context)
    {
        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            return new RoadSegmentsRemoved
            {
                GeometryDrawMethod = GeometryDrawMethod,
                RemovedRoadSegmentIds = Ids.Select(x => x.ToInt32()).ToArray(),
                RemovedRoadNodeIds = [],
                ChangedRoadNodes = [],
                RemovedGradeSeparatedJunctionIds = [],
                MergedRoadSegments = [],
            };
        }

        var segmentRoadNodeIds = GetSegmentRoadNodeIds(context);
        var removedRoadNodeIds = segmentRoadNodeIds.Where(x => !context.AfterView.Nodes.ContainsKey(x)).ToArray();
        var changedRoadNodes = GetRoadNodeTypeChanges(context, segmentRoadNodeIds.Except(removedRoadNodeIds));
        var removedJunctionIds = GetRemovedJunctionIds(context);
        var mergedRoadSegments = GetMergedRoadSegments(context);

        return new RoadSegmentsRemoved
        {
            GeometryDrawMethod = GeometryDrawMethod,
            RemovedRoadSegmentIds = Ids.Select(x => x.ToInt32()).ToArray(),
            RemovedRoadNodeIds = removedRoadNodeIds.Select(x => x.ToInt32()).ToArray(),
            ChangedRoadNodes = changedRoadNodes,
            RemovedGradeSeparatedJunctionIds = removedJunctionIds.Select(x => x.ToInt32()).ToArray(),
            MergedRoadSegments = mergedRoadSegments,
        };
    }

    private List<RoadNodeId> GetSegmentRoadNodeIds(AfterVerificationContext context)
    {
        var segmentRoadNodeIds = new List<RoadNodeId>();
        foreach (var id in Ids)
        {
            context.BeforeView.View.Segments.TryGetValue(id, out var segment);
            segmentRoadNodeIds.Add(segment!.Start);
            segmentRoadNodeIds.Add(segment!.End);
        }
        segmentRoadNodeIds = segmentRoadNodeIds.Distinct().ToList();
        return segmentRoadNodeIds;
    }

    private RoadNodeTypeChanged[] GetRoadNodeTypeChanges(AfterVerificationContext context, IEnumerable<RoadNodeId> changedRoadNodeIds)
    {
        List<RoadNodeTypeChanged> changedRoadNodes = [];
        foreach (var roadNodeId in changedRoadNodeIds)
        {
            context.BeforeView.Nodes.TryGetValue(roadNodeId, out var roadNodeBefore);
            context.AfterView.Nodes.TryGetValue(roadNodeId, out var roadNodeAfter);

            if (roadNodeBefore!.Type == roadNodeAfter!.Type)
            {
                continue;
            }

            changedRoadNodes.Add(new RoadNodeTypeChanged
            {
                Id = roadNodeId,
                Type = roadNodeAfter.Type,
                Version = _roadNetworkVersionProvider.NextRoadNodeVersion(roadNodeId)
            });
        }

        return changedRoadNodes.ToArray();
    }

    private static GradeSeparatedJunctionId[] GetRemovedJunctionIds(AfterVerificationContext context)
    {
        return context.BeforeView.GradeSeparatedJunctions.Keys
            .Where(x => !context.AfterView.GradeSeparatedJunctions.ContainsKey(x))
            .ToArray();
    }

    private RoadSegmentMerged[] GetMergedRoadSegments(AfterVerificationContext context)
    {
        return context.AfterView.Segments.Keys
            .Except(context.BeforeView.Segments.Keys)
            .Select(roadSegmentId =>
            {
                context.AfterView.View.Segments.TryGetValue(roadSegmentId, out var segment);

                return new RoadSegmentMerged
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
                };
            })
            .ToArray();
    }
}
