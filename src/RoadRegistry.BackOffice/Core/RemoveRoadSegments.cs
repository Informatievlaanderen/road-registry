namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;

public class RemoveRoadSegments : IRequestedChange, IHaveHash
{
    public const string EventName = "RemoveRoadSegments";
    private readonly RoadNetworkVersionProvider _roadNetworkVersionProvider;
    private readonly IRoadNetworkIdProvider _roadNetworkIdProvider;
    private readonly IOrganizations _organizations;

    public RemoveRoadSegments(IReadOnlyList<RoadSegmentId> ids,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadNetworkVersionProvider roadNetworkVersionProvider,
        IRoadNetworkIdProvider roadNetworkIdProvider,
        IOrganizations organizations)
    {
        Ids = ids;
        GeometryDrawMethod = geometryDrawMethod;
        _roadNetworkVersionProvider = roadNetworkVersionProvider;
        _roadNetworkIdProvider = roadNetworkIdProvider;
        _organizations = organizations;
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

    private IEnumerable<Messages.AcceptedChange> _acceptedChanges;

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        // todo-pr: did we create islands?

        _acceptedChanges = BuildAcceptedChanges(context);

        return problems;
    }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] _)
    {
        return _acceptedChanges;
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

    private IEnumerable<Messages.AcceptedChange> BuildAcceptedChanges(AfterVerificationContext context)
    {
        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            return Ids.Select(x => new Messages.AcceptedChange
            {
                RoadSegmentRemoved = new()
                {
                    GeometryDrawMethod = GeometryDrawMethod,
                    Id = x
                }
            });
        }

        var removedJunctionIds = GetRemovedJunctionIds(context);
        var removedRoadSegmentIds = GetRemovedSegmentIds(context);
        var segmentRoadNodeIds = GetSegmentRoadNodeIds(context);
        var removedRoadNodeIds = segmentRoadNodeIds.Where(x => !context.AfterView.Nodes.ContainsKey(x)).ToArray();
        var changedRoadNodes = GetModifiedRoadNodes(context, segmentRoadNodeIds.Except(removedRoadNodeIds));
        var mergedRoadSegmentsChanges = GetMergedRoadSegmentsChanges(context);

        var acceptedChanges = new List<Messages.AcceptedChange>();
        acceptedChanges.AddRange(removedJunctionIds.Select(x => new Messages.AcceptedChange
        {
            GradeSeparatedJunctionRemoved = new()
            {
                Id = x
            }
        }));
        acceptedChanges.AddRange(removedRoadSegmentIds.Select(x => new Messages.AcceptedChange
        {
            RoadSegmentRemoved = new()
            {
                GeometryDrawMethod = GeometryDrawMethod,
                Id = x
            }
        }));
        acceptedChanges.AddRange(removedRoadNodeIds.Select(x => new Messages.AcceptedChange
        {
            RoadNodeRemoved = new()
            {
                Id = x
            }
        }));
        acceptedChanges.AddRange(mergedRoadSegmentsChanges);
        acceptedChanges.AddRange(changedRoadNodes.Select(x => new Messages.AcceptedChange
        {
            RoadNodeModified = x
        }));

        return acceptedChanges;
    }

    private RoadNodeId[] GetSegmentRoadNodeIds(AfterVerificationContext context)
    {
        return Ids
            .SelectMany(id =>
            {
                context.BeforeView.View.Segments.TryGetValue(id, out var segment);
                return segment!.Nodes;
            })
            .Distinct()
            .ToArray();
    }

    private RoadNodeModified[] GetModifiedRoadNodes(AfterVerificationContext context, IEnumerable<RoadNodeId> changedRoadNodeIds)
    {
        List<RoadNodeModified> changedRoadNodes = [];

        foreach (var roadNodeId in changedRoadNodeIds)
        {
            context.BeforeView.Nodes.TryGetValue(roadNodeId, out var roadNodeBefore);
            context.AfterView.Nodes.TryGetValue(roadNodeId, out var roadNodeAfter);

            if (roadNodeBefore!.Type == roadNodeAfter!.Type)
            {
                continue;
            }

            changedRoadNodes.Add(new RoadNodeModified
            {
                Id = roadNodeId,
                Type = roadNodeAfter.Type,
                Version = _roadNetworkVersionProvider.NextRoadNodeVersion(roadNodeId),
                Geometry = GeometryTranslator.Translate(roadNodeAfter.Geometry),
            });
        }

        return changedRoadNodes.ToArray();
    }

    private static RoadSegmentId[] GetRemovedSegmentIds(AfterVerificationContext context)
    {
        return context.BeforeView.Segments.Keys
            .Except(context.AfterView.Segments.Keys)
            .ToArray();
    }

    private static GradeSeparatedJunctionId[] GetRemovedJunctionIds(AfterVerificationContext context)
    {
        return context.BeforeView.GradeSeparatedJunctions.Keys
            .Except(context.AfterView.GradeSeparatedJunctions.Keys)
            .ToArray();
    }

    private Messages.AcceptedChange[] GetMergedRoadSegmentsChanges(AfterVerificationContext context)
    {
        return context.AfterView.Segments.Keys
            .Except(context.BeforeView.Segments.Keys)
            .SelectMany(roadSegmentId =>
            {
                context.AfterView.View.Segments.TryGetValue(roadSegmentId, out var segment);

                //TODO-pr generate new ID na logica afterview, nieuwe ID gebruiken voor events (ook in junctions)

                var laneAttributeIdProvider = _roadNetworkIdProvider.NextRoadSegmentLaneAttributeIdProvider(roadSegmentId);
                var surfaceAttributeIdProvider = _roadNetworkIdProvider.NextRoadSegmentSurfaceAttributeIdProvider(roadSegmentId);
                var widthAttributeIdProvider = _roadNetworkIdProvider.NextRoadSegmentWidthAttributeIdProvider(roadSegmentId);

                var modifiedJunctions = context.AfterView.View.GradeSeparatedJunctions
                    .Where(x => x.Value.LowerSegment == roadSegmentId
                                || x.Value.UpperSegment == roadSegmentId)
                    .Select(x => x.Value);

                return Enumerable.Empty<Messages.AcceptedChange>()
                    .Concat([
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = new RoadSegmentAdded
                            {
                                Id = roadSegmentId,
                                TemporaryId = roadSegmentId,
                                Version = RoadSegmentVersion.Initial,
                                Geometry = GeometryTranslator.Translate(segment.Geometry),
                                GeometryVersion = GeometryVersion.Initial,
                                StartNodeId = segment.Start,
                                EndNodeId = segment.End,
                                GeometryDrawMethod = segment.AttributeHash.GeometryDrawMethod,
                                AccessRestriction = segment!.AttributeHash.AccessRestriction,
                                Category = segment.AttributeHash.Category,
                                Morphology = segment.AttributeHash.Category,
                                Status = segment.AttributeHash.Category,
                                MaintenanceAuthority = new MaintenanceAuthority
                                {
                                    Code = segment.AttributeHash.OrganizationId,
                                    Name = _organizations.FindAsync(segment.AttributeHash.OrganizationId).GetAwaiter().GetResult()?.Translation.Name
                                },
                                LeftSide = new Messages.RoadSegmentSideAttributes { StreetNameId = segment.AttributeHash.LeftStreetNameId },
                                RightSide = new Messages.RoadSegmentSideAttributes { StreetNameId = segment.AttributeHash.RightStreetNameId },
                                Lanes = segment!.Lanes.Select(x => new Messages.RoadSegmentLaneAttributes
                                {
                                    AttributeId = laneAttributeIdProvider().GetAwaiter().GetResult(),
                                    Count = x.Count,
                                    Direction = x.Direction,
                                    FromPosition = x.From,
                                    ToPosition = x.To,
                                    AsOfGeometryVersion = x.AsOfGeometryVersion
                                }).ToArray(),
                                Surfaces = segment.Surfaces.Select(x => new Messages.RoadSegmentSurfaceAttributes
                                    {
                                        AttributeId = surfaceAttributeIdProvider().GetAwaiter().GetResult(),
                                        Type = x.Type,
                                        FromPosition = x.From,
                                        ToPosition = x.To,
                                        AsOfGeometryVersion = x.AsOfGeometryVersion
                                    })
                                    .ToArray(),
                                Widths = segment.Widths
                                    .Select(x => new Messages.RoadSegmentWidthAttributes
                                    {
                                        AttributeId = widthAttributeIdProvider().GetAwaiter().GetResult(),
                                        Width = x.Width,
                                        FromPosition = x.From,
                                        ToPosition = x.To,
                                        AsOfGeometryVersion = x.AsOfGeometryVersion
                                    })
                                    .ToArray()
                            }
                        }
                    ])
                    .Concat(segment.EuropeanRoadAttributes.Select(road =>
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAddedToEuropeanRoad = new()
                            {
                                AttributeId = road.Value.AttributeId,
                                TemporaryAttributeId = road.Value.AttributeId,
                                SegmentId = roadSegmentId,
                                SegmentVersion = RoadSegmentVersion.Initial,
                                Number = road.Value.Number
                            }
                        }))
                    .Concat(segment.NationalRoadAttributes.Select(road =>
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAddedToNationalRoad = new()
                            {
                                AttributeId = road.Value.AttributeId,
                                TemporaryAttributeId = road.Value.AttributeId,
                                SegmentId = roadSegmentId,
                                SegmentVersion = RoadSegmentVersion.Initial,
                                Number = road.Value.Number
                            }
                        }))
                    .Concat(segment.NumberedRoadAttributes.Select(road =>
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAddedToNumberedRoad = new()
                            {
                                AttributeId = road.Value.AttributeId,
                                TemporaryAttributeId = road.Value.AttributeId,
                                SegmentId = roadSegmentId,
                                SegmentVersion = RoadSegmentVersion.Initial,
                                Number = road.Value.Number,
                                Direction = road.Value.Direction,
                                Ordinal = road.Value.Ordinal
                            }
                        }))
                    .Concat(modifiedJunctions.Select(junction =>
                        new Messages.AcceptedChange
                        {
                            GradeSeparatedJunctionModified = new()
                            {
                                Id = junction.Id,
                                Type = junction.Type,
                                LowerRoadSegmentId = junction.LowerSegment,
                                UpperRoadSegmentId = junction.UpperSegment
                            }
                        }));
            })
            .ToArray();
    }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
