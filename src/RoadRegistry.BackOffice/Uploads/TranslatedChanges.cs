namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;

    public class TranslatedChanges : IReadOnlyCollection<ITranslatedChange>
    {
        public static readonly TranslatedChanges Empty = new TranslatedChanges(
            Reason.None,
            OperatorName.None,
            OrganizationId.Unknown,
            ImmutableList<ITranslatedChange>.Empty,
            ImmutableList<ITranslatedChange>.Empty,
            ImmutableDictionary<RecordNumber, RoadNodeId>.Empty,
            ImmutableDictionary<RecordNumber, RoadSegmentId>.Empty);

        private readonly ImmutableList<ITranslatedChange> _changes;
        private readonly ImmutableList<ITranslatedChange> _provisionalChanges;
        private readonly ImmutableDictionary<RecordNumber, RoadNodeId> _mapToRoadNodeId;
        private readonly ImmutableDictionary<RecordNumber, RoadSegmentId> _mapToRoadSegmentId;

        private TranslatedChanges(Reason reason,
            OperatorName @operator,
            OrganizationId organization,
            ImmutableList<ITranslatedChange> changes,
            ImmutableList<ITranslatedChange> provisionalChanges,
            ImmutableDictionary<RecordNumber, RoadNodeId> mapToRoadNodeId,
            ImmutableDictionary<RecordNumber, RoadSegmentId> mapToRoadSegmentId)
        {
            Reason = reason;
            Operator = @operator;
            Organization = organization;
            _changes = changes ?? throw new ArgumentNullException(nameof(changes));
            _provisionalChanges = provisionalChanges ?? throw new ArgumentNullException(nameof(provisionalChanges));
            _mapToRoadNodeId = mapToRoadNodeId ?? throw new ArgumentNullException(nameof(mapToRoadNodeId));
            _mapToRoadSegmentId = mapToRoadSegmentId ?? throw new ArgumentNullException(nameof(mapToRoadSegmentId));
        }

        public IEnumerator<ITranslatedChange> GetEnumerator() => _changes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _changes.Count;

        public Reason Reason { get; }
        public OperatorName Operator { get; }
        public OrganizationId Organization { get; }

        public TranslatedChanges WithReason(Reason value)
        {
            return new TranslatedChanges(value, Operator, Organization, _changes, _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges WithOperatorName(OperatorName value)
        {
            return new TranslatedChanges(Reason, value, Organization, _changes, _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges WithOrganization(OrganizationId value)
        {
            return new TranslatedChanges(Reason, Operator, value, _changes, _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(AddRoadNode change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId.Add(change.RecordNumber, change.TemporaryId), _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(ModifyRoadNode change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId.Add(change.RecordNumber, change.Id), _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(RemoveRoadNode change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId.Add(change.RecordNumber, change.Id), _mapToRoadSegmentId);
        }

        public bool TryFindRoadNodeChange(RoadNodeId id, out object change)
        {
            change = new ITranslatedChange[]
            {
                this.OfType<AddRoadNode>().SingleOrDefault(_ => _.TemporaryId == id),
                this.OfType<ModifyRoadNode>().SingleOrDefault(_ => _.Id == id),
                this.OfType<RemoveRoadNode>().SingleOrDefault(_ => _.Id == id)
            }.Flatten();
            return change != null;
        }

        public TranslatedChanges ReplaceChange(AddRoadNode before, AddRoadNode after)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges ReplaceChange(ModifyRoadNode before, ModifyRoadNode after)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public bool TryTranslateToRoadNodeId(RecordNumber number, out RoadNodeId translated)
        {
            return _mapToRoadNodeId.TryGetValue(number, out translated);
        }

        public TranslatedChanges AppendChange(AddRoadSegment change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.TemporaryId));
        }

        public TranslatedChanges AppendChange(ModifyRoadSegment change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.Id));
        }

        public TranslatedChanges AppendChange(RemoveRoadSegment change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.Id));
        }

        public TranslatedChanges AppendProvisionalChange(ModifyRoadSegment change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes, _provisionalChanges.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.Id));
        }

        public bool TryFindRoadSegmentChange(RoadSegmentId id, out object change)
        {
            change = new ITranslatedChange[]
            {
                _changes.OfType<AddRoadSegment>().SingleOrDefault(_ => _.TemporaryId == id),
                _changes.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.Id == id),
                _provisionalChanges.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.Id == id)
            }.Flatten();
            return change != null;
        }

        public bool TryTranslateToRoadSegmentId(RecordNumber number, out RoadSegmentId translated)
        {
            return _mapToRoadSegmentId.TryGetValue(number, out translated);
        }

        public TranslatedChanges ReplaceChange(AddRoadSegment before, AddRoadSegment after)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges ReplaceChange(ModifyRoadSegment before, ModifyRoadSegment after)
        {
            return _provisionalChanges.Contains(before)
                ? new TranslatedChanges(Reason, Operator, Organization, _changes.Add(after), _provisionalChanges.Remove(before), _mapToRoadNodeId, _mapToRoadSegmentId)
                : new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(AddRoadSegmentToEuropeanRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(RemoveRoadSegmentFromEuropeanRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(AddRoadSegmentToNationalRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(RemoveRoadSegmentFromNationalRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(AddRoadSegmentToNumberedRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(ModifyRoadSegmentOnNumberedRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(RemoveRoadSegmentFromNumberedRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(AddGradeSeparatedJunction change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(ModifyGradeSeparatedJunction change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges AppendChange(RemoveGradeSeparatedJunction change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges, _mapToRoadNodeId, _mapToRoadSegmentId);
        }
    }
}
