namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class VerifiedChanges : IReadOnlyCollection<IVerifiedChange>
    {
        private readonly ImmutableList<IVerifiedChange> _changes;

        public static readonly VerifiedChanges Empty = new VerifiedChanges(
            ImmutableList<IVerifiedChange>.Empty
        );

        private VerifiedChanges(ImmutableList<IVerifiedChange> changes)
        {
            _changes = changes;
        }

        public VerifiedChanges Append(IVerifiedChange change)
        {
            if (change == null) throw new ArgumentNullException(nameof(change));

            return new VerifiedChanges(_changes.Add(change));
        }

        public int Count => _changes.Count;

        public IEnumerator<IVerifiedChange> GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
