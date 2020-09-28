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
            ImmutableDictionary<RecordNumber, RoadNodeId>.Empty,
            ImmutableDictionary<RecordNumber, RoadSegmentId>.Empty);

        private readonly ImmutableList<ITranslatedChange> _changes;
        private readonly ImmutableDictionary<RecordNumber, RoadNodeId> _mapToRoadNodeId;
        private readonly ImmutableDictionary<RecordNumber, RoadSegmentId> _mapToRoadSegmentId;

        private TranslatedChanges(
            Reason reason,
            OperatorName @operator,
            OrganizationId organization,
            ImmutableList<ITranslatedChange> changes,
            ImmutableDictionary<RecordNumber, RoadNodeId> mapToRoadNodeId,
            ImmutableDictionary<RecordNumber, RoadSegmentId> mapToRoadSegmentId)
        {
            Reason = reason;
            Operator = @operator;
            Organization = organization;
            _changes = changes ?? throw new ArgumentNullException(nameof(changes));
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
            return new TranslatedChanges(value, Operator, Organization, _changes, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges WithOperatorName(OperatorName value)
        {
            return new TranslatedChanges(Reason, value, Organization, _changes, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges WithOrganization(OrganizationId value)
        {
            return new TranslatedChanges(Reason, Operator, value, _changes, _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(AddRoadNode change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId.Add(change.RecordNumber, change.TemporaryId), _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(ModifyRoadNode change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId.Add(change.RecordNumber, change.Id), _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(RemoveRoadNode change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId.Add(change.RecordNumber, change.Id), _mapToRoadSegmentId);
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

        public bool TryFindAddRoadNode(RoadNodeId id, out AddRoadNode change)
        {
            change = this.OfType<AddRoadNode>().SingleOrDefault(_ => _.TemporaryId == id);
            return change != null;
        }

        public TranslatedChanges Replace(AddRoadNode before, AddRoadNode after)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public bool TryFindModifyRoadNode(RoadNodeId id, out ModifyRoadNode change)
        {
            change = this.OfType<ModifyRoadNode>().SingleOrDefault(_ => _.Id == id);
            return change != null;
        }

        public TranslatedChanges Replace(ModifyRoadNode before, ModifyRoadNode after)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public bool TryTranslateToRoadNodeId(RecordNumber number, out RoadNodeId translated)
        {
            return _mapToRoadNodeId.TryGetValue(number, out translated);
        }

        public TranslatedChanges Append(AddRoadSegment change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.TemporaryId));
        }

        public TranslatedChanges Append(ModifyRoadSegment change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.Id));
        }

        public TranslatedChanges Append(RemoveRoadSegment change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.Id));
        }

        public bool TryFindRoadSegmentChange(RoadSegmentId id, out object change)
        {
            change = new ITranslatedChange[]
            {
                this.OfType<AddRoadSegment>().SingleOrDefault(_ => _.TemporaryId == id),
                this.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.Id == id)
            }.Flatten();
            return change != null;
        }

        public bool TryFindAddRoadSegment(RoadSegmentId id, out AddRoadSegment change)
        {
            change = this.OfType<AddRoadSegment>().SingleOrDefault(_ => _.TemporaryId == id);
            return change != null;
        }

        public bool TryFindModifyRoadSegment(RoadSegmentId id, out ModifyRoadSegment change)
        {
            change = this.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.Id == id);
            return change != null;
        }

        public bool TryTranslateToRoadSegmentId(RecordNumber number, out RoadSegmentId translated)
        {
            return _mapToRoadSegmentId.TryGetValue(number, out translated);
        }

        public TranslatedChanges Replace(AddRoadSegment before, AddRoadSegment after)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Replace(ModifyRoadSegment before, ModifyRoadSegment after)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(AddRoadSegmentToEuropeanRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(RemoveRoadSegmentFromEuropeanRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(AddRoadSegmentToNationalRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(RemoveRoadSegmentFromNationalRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(AddRoadSegmentToNumberedRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(ModifyRoadSegmentOnNumberedRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(RemoveRoadSegmentFromNumberedRoad change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(AddGradeSeparatedJunction change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(ModifyGradeSeparatedJunction change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }

        public TranslatedChanges Append(RemoveGradeSeparatedJunction change)
        {
            return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId);
        }
    }
}
