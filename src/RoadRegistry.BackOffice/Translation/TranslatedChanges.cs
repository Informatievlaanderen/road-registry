namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class TranslatedChanges : IReadOnlyCollection<ITranslatedChange>
    {
        public static readonly TranslatedChanges Empty = new TranslatedChanges(
            ImmutableList<ITranslatedChange>.Empty);
        private readonly ImmutableList<ITranslatedChange> _changes;

        private TranslatedChanges(ImmutableList<ITranslatedChange> changes)
        {
            _changes = changes ?? throw new ArgumentNullException(nameof(changes));
        }

        public IEnumerator<ITranslatedChange> GetEnumerator() => _changes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _changes.Count;

        public TranslatedChanges Append(AddRoadNode change)
        {
            return new TranslatedChanges(
                _changes.Add(change)
            );
        }

        public TranslatedChanges Append(AddRoadSegment change)
        {
            return new TranslatedChanges(
                _changes.Add(change)
            );
        }

        public TranslatedChanges Replace(AddRoadSegment change)
        {
            return new TranslatedChanges(
                _changes.Add(change)
            );
        }

        public TranslatedChanges Append(AddRoadSegmentToEuropeanRoad change)
        {
            return new TranslatedChanges(
                _changes.Add(change)
            );
        }

        public TranslatedChanges Append(AddRoadSegmentToNationalRoad change)
        {
            return new TranslatedChanges(
                _changes.Add(change)
            );
        }

        public TranslatedChanges Append(AddRoadSegmentToNumberedRoad change)
        {
            return new TranslatedChanges(
                _changes.Add(change)
            );
        }

        public TranslatedChanges Append(AddGradeSeparatedJunction change)
        {
            return new TranslatedChanges(
                _changes.Add(change)
            );
        }
    }
}
