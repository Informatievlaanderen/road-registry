namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public class TranslatedChanges : IReadOnlyCollection<ITranslatedChange>
    {
        public static readonly TranslatedChanges Empty = new TranslatedChanges(
            ImmutableList<ITranslatedChange>.Empty,
            ImmutableDictionary<RecordNumber, RoadNodeId>.Empty,
            ImmutableDictionary<RecordNumber, RoadSegmentId>.Empty);

        private readonly ImmutableList<ITranslatedChange> _changes;
        private readonly ImmutableDictionary<RecordNumber, RoadNodeId> _mapToRoadNodeId;
        private readonly ImmutableDictionary<RecordNumber, RoadSegmentId> _mapToRoadSegmentId;

        private TranslatedChanges(
            ImmutableList<ITranslatedChange> changes,
            ImmutableDictionary<RecordNumber, RoadNodeId> mapToRoadNodeId,
            ImmutableDictionary<RecordNumber, RoadSegmentId> mapToRoadSegmentId)
        {
            _changes = changes ?? throw new ArgumentNullException(nameof(changes));
            _mapToRoadNodeId = mapToRoadNodeId ?? throw new ArgumentNullException(nameof(mapToRoadNodeId));
            _mapToRoadSegmentId = mapToRoadSegmentId ?? throw new ArgumentNullException(nameof(mapToRoadSegmentId));
        }

        public IEnumerator<ITranslatedChange> GetEnumerator() => _changes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _changes.Count;

        public TranslatedChanges Append(AddRoadNode change)
        {
            return new TranslatedChanges(
                _changes.Add(change), _mapToRoadNodeId.Add(change.RecordNumber, change.TemporaryId), _mapToRoadSegmentId
            );
        }

        public bool TryFindAddRoadNode(RoadNodeId id, out AddRoadNode change)
        {
            change = this.OfType<AddRoadNode>().SingleOrDefault(_ => _.TemporaryId == id);
            return change != null;
        }

        public TranslatedChanges Replace(AddRoadNode before, AddRoadNode after)
        {
            return new TranslatedChanges(
                _changes.Remove(before).Add(after), _mapToRoadNodeId, _mapToRoadSegmentId
            );
        }

        public bool TryTranslateToRoadNodeId(RecordNumber number, out RoadNodeId translated)
        {
            return _mapToRoadNodeId.TryGetValue(number, out translated);
        }

        public TranslatedChanges Append(AddRoadSegment change)
        {
            return new TranslatedChanges(
                _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId.Add(change.RecordNumber, change.TemporaryId)
            );
        }

        public bool TryFindAddRoadSegment(RoadSegmentId id, out AddRoadSegment change)
        {
            change = this.OfType<AddRoadSegment>().SingleOrDefault(_ => _.TemporaryId == id);
            return change != null;
        }

        public bool TryTranslateToRoadSegmentId(RecordNumber number, out RoadSegmentId translated)
        {
            return _mapToRoadSegmentId.TryGetValue(number, out translated);
        }

        public TranslatedChanges Replace(AddRoadSegment before, AddRoadSegment after)
        {
            return new TranslatedChanges(
                _changes.Remove(before).Add(after), _mapToRoadNodeId, _mapToRoadSegmentId
            );
        }

        public TranslatedChanges Append(AddRoadSegmentToEuropeanRoad change)
        {
            return new TranslatedChanges(
                _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId
            );
        }

        public TranslatedChanges Append(AddRoadSegmentToNationalRoad change)
        {
            return new TranslatedChanges(
                _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId
            );
        }

        public TranslatedChanges Append(AddRoadSegmentToNumberedRoad change)
        {
            return new TranslatedChanges(
                _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId
            );
        }

        public TranslatedChanges Append(AddGradeSeparatedJunction change)
        {
            return new TranslatedChanges(
                _changes.Add(change), _mapToRoadNodeId, _mapToRoadSegmentId
            );
        }
    }
}
