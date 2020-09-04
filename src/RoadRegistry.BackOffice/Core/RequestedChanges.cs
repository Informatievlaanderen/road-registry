namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class RequestedChanges : IReadOnlyCollection<IRequestedChange>, IRequestedChangeIdentityTranslator
    {
        public static RequestedChanges Start(TransactionId transactionId) =>
            new RequestedChanges(
                transactionId,
                ImmutableList<IRequestedChange>.Empty,
                ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
                ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty,
                ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>.Empty,
                ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId>.Empty);

        private readonly TransactionId _transactionId;
        private readonly ImmutableList<IRequestedChange> _changes;
        private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToPermanentNodeIdentifiers;
        private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToTemporaryNodeIdentifiers;
        private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToPermanentSegmentIdentifiers;
        private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToTemporarySegmentIdentifiers;
        private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapToPermanentGradeSeparatedJunctionIdentifiers;
        private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> _mapToTemporaryGradeSeparatedJunctionIdentifiers;

        private RequestedChanges(
            TransactionId transactionId,
            ImmutableList<IRequestedChange> changes,
            ImmutableDictionary<RoadNodeId, RoadNodeId> mapToPermanentNodeIdentifiers,
            ImmutableDictionary<RoadNodeId, RoadNodeId> mapToTemporaryNodeIdentifiers,
            ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToPermanentSegmentIdentifiers,
            ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToTemporarySegmentIdentifiers,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> mapToPermanentGradeSeparatedJunctionIdentifiers,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunctionId> mapToTemporaryGradeSeparatedJunctionIdentifiers)
        {
            _transactionId = transactionId;
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

        public RequestedChanges Append(AddRoadNode change)
        {
            if (change == null)
                throw new ArgumentNullException(nameof(change));

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
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

            return new RequestedChanges(_transactionId, _changes.Add(change),
                _mapToPermanentNodeIdentifiers,
                _mapToTemporaryNodeIdentifiers,
                _mapToPermanentSegmentIdentifiers,
                _mapToTemporarySegmentIdentifiers,
                _mapToPermanentGradeSeparatedJunctionIdentifiers,
                _mapToTemporaryGradeSeparatedJunctionIdentifiers);
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

        public VerifiedChanges VerifyWith(IRoadNetworkView view)
        {
            var context = new VerificationContext(view, this);
            return _changes.Aggregate(
                VerifiedChanges.Empty,
                (verifiedChanges, requestedChange) => verifiedChanges.Append(requestedChange.Verify(context)));
        }
    }
}
