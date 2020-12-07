namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class RequestedChanges : IReadOnlyCollection<IRequestedChange>, IRequestedChangeIdentityTranslator
    {
        public static RequestedChanges Start(TransactionId transactionId) =>
            new RequestedChanges(
                transactionId,
                new Envelope(0.0,0.0,0.0,0.0),
                ImmutableList<IRequestedChange>.Empty,
                ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
                ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty,
                ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>.Empty,
                ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>.Empty);

        private readonly TransactionId _transactionId;
        private readonly Envelope _envelope;
        private readonly ImmutableList<IRequestedChange> _changes;
        private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToPermanentNodeIdentifiers;
        private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToTemporaryNodeIdentifiers;
        private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToPermanentSegmentIdentifiers;
        private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToTemporarySegmentIdentifiers;
        private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapToPermanentGradeSeparatedJunctionIdentifiers;
        private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapToTemporaryGradeSeparatedJunctionIdentifiers;

        private RequestedChanges(
            TransactionId transactionId,
            Envelope envelope,
            ImmutableList<IRequestedChange> changes,
            ImmutableDictionary<RoadNodeId, RoadNodeId> mapToPermanentNodeIdentifiers,
            ImmutableDictionary<RoadNodeId, RoadNodeId> mapToTemporaryNodeIdentifiers,
            ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToPermanentSegmentIdentifiers,
            ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToTemporarySegmentIdentifiers,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> mapToPermanentGradeSeparatedJunctionIdentifiers,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> mapToTemporaryGradeSeparatedJunctionIdentifiers)
        {
            _transactionId = transactionId;
            _envelope = envelope;
            _changes = changes;
            _mapToPermanentNodeIdentifiers = mapToPermanentNodeIdentifiers;
            _mapToTemporaryNodeIdentifiers = mapToTemporaryNodeIdentifiers;
            _mapToPermanentSegmentIdentifiers = mapToPermanentSegmentIdentifiers;
            _mapToTemporarySegmentIdentifiers = mapToTemporarySegmentIdentifiers;
            _mapToPermanentGradeSeparatedJunctionIdentifiers = mapToPermanentGradeSeparatedJunctionIdentifiers;
            _mapToTemporaryGradeSeparatedJunctionIdentifiers = mapToTemporaryGradeSeparatedJunctionIdentifiers;
        }

        public IEnumerator<IRequestedChange> GetEnumerator() => _changes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _changes.Count;
        public TransactionId TransactionId => _transactionId;
        public Envelope Envelope => _envelope;

        public RequestedChanges Append(AddRoadNode change)
        {
            if (change == null)
                throw new ArgumentNullException(nameof(change));

            return new RequestedChanges(_transactionId,
                _envelope.ExpandWith(change.Geometry.EnvelopeInternal),
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

            return new RequestedChanges(_transactionId,
                _envelope.ExpandWith(change.Geometry.EnvelopeInternal),
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope.ExpandWith(change.Geometry.EnvelopeInternal),
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

            return new RequestedChanges(_transactionId,
                _envelope.ExpandWith(change.Geometry.EnvelopeInternal),
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
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

            return new RequestedChanges(_transactionId,
                _envelope,
                _changes.Add(change),
                _mapToPermanentNodeIdentifiers,
                _mapToTemporaryNodeIdentifiers,
                _mapToPermanentSegmentIdentifiers,
                _mapToTemporarySegmentIdentifiers,
                _mapToPermanentGradeSeparatedJunctionIdentifiers,
                _mapToTemporaryGradeSeparatedJunctionIdentifiers);
        }

        private static readonly HashSet<Type> RoadNodeChanges = new HashSet<Type>(new []
        {
            typeof(AddRoadNode), typeof(ModifyRoadNode), typeof(RemoveRoadNode)
        });

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

        private static readonly HashSet<Type> RoadSegmentChanges = new HashSet<Type>(new []
        {
            typeof(AddRoadSegment), typeof(ModifyRoadSegment), typeof(RemoveRoadSegment)
        });

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

        private static readonly HashSet<Type> GradeSeparatedJunctionChanges = new HashSet<Type>(new []
        {
            typeof(AddGradeSeparatedJunction), typeof(ModifyGradeSeparatedJunction), typeof(RemoveGradeSeparatedJunction)
        });

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

        public BeforeVerificationContext CreateBeforeVerificationContext(IRoadNetworkView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            var tolerances = new VerificationContextTolerances(
                DefaultTolerances.DynamicRoadSegmentAttributePositionTolerance,
                DefaultTolerances.MeasurementTolerance,
                DefaultTolerances.GeometryTolerance);
            return new BeforeVerificationContext(view, this, tolerances);
        }
    }
}
