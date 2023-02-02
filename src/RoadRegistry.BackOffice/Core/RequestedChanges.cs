namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NetTopologySuite.Geometries;

public class RequestedChanges : IReadOnlyCollection<IRequestedChange>, IRequestedChangeIdentityTranslator
{
    private static readonly HashSet<Type> GradeSeparatedJunctionChanges = new(new[]
    {
        typeof(AddGradeSeparatedJunction), typeof(ModifyGradeSeparatedJunction), typeof(RemoveGradeSeparatedJunction)
    });

    private static readonly HashSet<Type> RoadNodeChanges = new(new[]
    {
        typeof(AddRoadNode), typeof(ModifyRoadNode), typeof(RemoveRoadNode)
    });

    private static readonly HashSet<Type> RoadSegmentChanges = new(new[]
    {
        typeof(AddRoadSegment), typeof(ModifyRoadSegment), typeof(RemoveRoadSegment)
    });

    private readonly ImmutableList<IRequestedChange> _changes;
    private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapToPermanentGradeSeparatedJunctionIdentifiers;
    private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToPermanentNodeIdentifiers;
    private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToPermanentSegmentIdentifiers;
    private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapToTemporaryGradeSeparatedJunctionIdentifiers;
    private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToTemporaryNodeIdentifiers;
    private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToTemporarySegmentIdentifiers;

    private RequestedChanges(
        TransactionId transactionId,
        ImmutableList<IRequestedChange> changes,
        ImmutableDictionary<RoadNodeId, RoadNodeId> mapToPermanentNodeIdentifiers,
        ImmutableDictionary<RoadNodeId, RoadNodeId> mapToTemporaryNodeIdentifiers,
        ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToPermanentSegmentIdentifiers,
        ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToTemporarySegmentIdentifiers,
        ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>
            mapToPermanentGradeSeparatedJunctionIdentifiers,
        ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>
            mapToTemporaryGradeSeparatedJunctionIdentifiers)
    {
        TransactionId = transactionId;
        _changes = changes;
        _mapToPermanentNodeIdentifiers = mapToPermanentNodeIdentifiers;
        _mapToTemporaryNodeIdentifiers = mapToTemporaryNodeIdentifiers;
        _mapToPermanentSegmentIdentifiers = mapToPermanentSegmentIdentifiers;
        _mapToTemporarySegmentIdentifiers = mapToTemporarySegmentIdentifiers;
        _mapToPermanentGradeSeparatedJunctionIdentifiers = mapToPermanentGradeSeparatedJunctionIdentifiers;
        _mapToTemporaryGradeSeparatedJunctionIdentifiers = mapToTemporaryGradeSeparatedJunctionIdentifiers;
    }

    public int Count => _changes.Count;
    public TransactionId TransactionId { get; }

    public IEnumerator<IRequestedChange> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public RoadNodeId TranslateToTemporaryOrId(RoadNodeId id)
    {
        return _mapToTemporaryNodeIdentifiers.TryGetValue(id, out var temporary)
            ? temporary
            : id;
    }

    public RoadSegmentId TranslateToTemporaryOrId(RoadSegmentId id)
    {
        return _mapToTemporarySegmentIdentifiers.TryGetValue(id, out var temporary)
            ? temporary
            : id;
    }

    public GradeSeparatedJunctionId TranslateToTemporaryOrId(GradeSeparatedJunctionId id)
    {
        return _mapToTemporaryGradeSeparatedJunctionIdentifiers.TryGetValue(id, out var temporary)
            ? temporary
            : id;
    }

    public bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent)
    {
        return _mapToPermanentNodeIdentifiers.TryGetValue(id, out permanent);
    }

    public bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent)
    {
        return _mapToPermanentSegmentIdentifiers.TryGetValue(id, out permanent);
    }

    public bool TryTranslateToPermanent(GradeSeparatedJunctionId id, out GradeSeparatedJunctionId temporary)
    {
        return _mapToPermanentGradeSeparatedJunctionIdentifiers.TryGetValue(id, out temporary);
    }

    public bool TryTranslateToTemporary(RoadNodeId id, out RoadNodeId temporary)
    {
        return _mapToTemporaryNodeIdentifiers.TryGetValue(id, out temporary);
    }

    public bool TryTranslateToTemporary(RoadSegmentId id, out RoadSegmentId temporary)
    {
        return _mapToTemporarySegmentIdentifiers.TryGetValue(id, out temporary);
    }

    public bool TryTranslateToTemporary(GradeSeparatedJunctionId id, out GradeSeparatedJunctionId temporary)
    {
        return _mapToTemporaryGradeSeparatedJunctionIdentifiers.TryGetValue(id, out temporary);
    }

    public RequestedChanges Append(AddRoadNode change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers.Add(change.TemporaryId, change.Id),
            _mapToTemporaryNodeIdentifiers.Add(change.Id, change.TemporaryId),
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(ModifyRoadNode change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(RemoveRoadNode change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(AddRoadSegment change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers.Add(change.TemporaryId, change.Id),
            _mapToTemporarySegmentIdentifiers.Add(change.Id, change.TemporaryId),
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(ModifyRoadSegment change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(RemoveRoadSegment change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(AddRoadSegmentToEuropeanRoad change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(RemoveRoadSegmentFromEuropeanRoad change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(AddRoadSegmentToNationalRoad change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(RemoveRoadSegmentFromNationalRoad change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(AddRoadSegmentToNumberedRoad change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(ModifyRoadSegmentOnNumberedRoad change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(RemoveRoadSegmentFromNumberedRoad change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(AddGradeSeparatedJunction change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers.Add(change.TemporaryId, change.Id),
            _mapToTemporaryGradeSeparatedJunctionIdentifiers.Add(change.Id, change.TemporaryId));
    }

    public RequestedChanges Append(ModifyGradeSeparatedJunction change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public RequestedChanges Append(RemoveGradeSeparatedJunction change)
    {
        if (change == null)
            throw new ArgumentNullException(nameof(change));

        return new RequestedChanges(TransactionId,
            _changes.Add(change),
            _mapToPermanentNodeIdentifiers,
            _mapToTemporaryNodeIdentifiers,
            _mapToPermanentSegmentIdentifiers,
            _mapToTemporarySegmentIdentifiers,
            _mapToPermanentGradeSeparatedJunctionIdentifiers,
            _mapToTemporaryGradeSeparatedJunctionIdentifiers);
    }

    public BeforeVerificationContext CreateBeforeVerificationContext(IRoadNetworkView view)
    {
        if (view == null) throw new ArgumentNullException(nameof(view));

        var tolerances = VerificationContextTolerances.Default;

        return new BeforeVerificationContext(
            view.CreateScopedView(DeriveScopeFromChanges(view)),
            this,
            tolerances);
    }

    private Envelope DeriveScopeFromChanges(IRoadNetworkView view)
    {
        var envelope = new Envelope();

        foreach (var change in this)
            switch (change)
            {
                case AddRoadNode addRoadNode:
                    // the geometry to add
                    envelope.ExpandToInclude(addRoadNode.Geometry.Coordinate);
                    break;
                case ModifyRoadNode modifyRoadNode:
                    // the geometry to modify it to
                    envelope.ExpandToInclude(modifyRoadNode.Geometry.Coordinate);
                    // if we still know this node, include the geometry as we know it now
                    if (view.Nodes.TryGetValue(modifyRoadNode.Id, out var nodeToModify)) envelope.ExpandToInclude(nodeToModify.Geometry.Coordinate);

                    break;
                case RemoveRoadNode removeRoadNode:
                    // if we still know this node, include the geometry as we know it now
                    if (view.Nodes.TryGetValue(removeRoadNode.Id, out var nodeToRemove)) envelope.ExpandToInclude(nodeToRemove.Geometry.Coordinate);

                    break;
                case AddRoadSegment addRoadSegment:
                    // the geometry to add
                    envelope.ExpandToInclude(addRoadSegment.Geometry.EnvelopeInternal);
                    break;
                case ModifyRoadSegment modifyRoadSegment:
                    // the geometry to modify it to
                    envelope.ExpandToInclude(modifyRoadSegment.Geometry.EnvelopeInternal);
                    // if we still know this segment, include the geometry as we know it now
                    if (view.Segments.TryGetValue(modifyRoadSegment.Id, out var segmentToModify)) envelope.ExpandToInclude(segmentToModify.Geometry.EnvelopeInternal);

                    break;
                case RemoveRoadSegment removeRoadSegment:
                    // if we still know this segment, include the geometry as we know it now
                    if (view.Segments.TryGetValue(removeRoadSegment.Id, out var segmentToRemove)) envelope.ExpandToInclude(segmentToRemove.Geometry.EnvelopeInternal);

                    break;
            }

        return envelope;
    }

    public IReadOnlyDictionary<GradeSeparatedJunctionId, IRequestedChange[]> FindConflictingGradeSeparatedJunctionChanges()
    {
        return this
            .Where(change => GradeSeparatedJunctionChanges.Contains(change.GetType()))
            .GroupBy(change =>
            {
                GradeSeparatedJunctionId id;
                switch (change)
                {
                    case AddGradeSeparatedJunction addGradeSeparatedJunction:
                        id = addGradeSeparatedJunction.Id;
                        break;
                    case ModifyGradeSeparatedJunction modifyGradeSeparatedJunction:
                        id = modifyGradeSeparatedJunction.Id;
                        break;
                    case RemoveGradeSeparatedJunction removeGradeSeparatedJunction:
                        id = removeGradeSeparatedJunction.Id;
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"The {change.GetType().Name} is not a grade separated junction change.");
                }

                return id;
            })
            .Where(changes => changes.Count() != 1)
            .ToDictionary(
                changes => changes.Key,
                changes => changes.ToArray());
    }

    public IReadOnlyDictionary<RoadNodeId, IRequestedChange[]> FindConflictingRoadNodeChanges()
    {
        return this
            .Where(change => RoadNodeChanges.Contains(change.GetType()))
            .GroupBy(change =>
            {
                RoadNodeId id;
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        id = addRoadNode.Id;
                        break;
                    case ModifyRoadNode modifyRoadNode:
                        id = modifyRoadNode.Id;
                        break;
                    case RemoveRoadNode removeRoadNode:
                        id = removeRoadNode.Id;
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"The {change.GetType().Name} is not a road node change.");
                }

                return id;
            })
            .Where(changes => changes.Count() != 1)
            .ToDictionary(
                changes => changes.Key,
                changes => changes.ToArray());
    }

    public IReadOnlyDictionary<RoadSegmentId, IRequestedChange[]> FindConflictingRoadSegmentChanges()
    {
        return this
            .Where(change => RoadSegmentChanges.Contains(change.GetType()))
            .GroupBy(change =>
            {
                RoadSegmentId id;
                switch (change)
                {
                    case AddRoadSegment addRoadSegment:
                        id = addRoadSegment.Id;
                        break;
                    case ModifyRoadSegment modifyRoadSegment:
                        id = modifyRoadSegment.Id;
                        break;
                    case RemoveRoadSegment removeRoadSegment:
                        id = removeRoadSegment.Id;
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"The {change.GetType().Name} is not a road segment change.");
                }

                return id;
            })
            .Where(changes => changes.Count() != 1)
            .ToDictionary(
                changes => changes.Key,
                changes => changes.ToArray());
    }

    public static RequestedChanges Start(TransactionId transactionId)
    {
        return new RequestedChanges(
            transactionId,
            ImmutableList<IRequestedChange>.Empty,
            ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
            ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
            ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty,
            ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>.Empty,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>.Empty);
    }
}
