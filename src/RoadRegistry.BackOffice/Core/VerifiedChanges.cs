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

        //TODO: Is it normal for a rejected change to be able to turn into an accepted change this way?
        public VerifiedChanges Accept(IRequestedChange change, Problems problems)
        {
            var foundChange = _changes.Find(verifiedChange => verifiedChange.RequestedChange == change);
            if (foundChange != null)
            {
                return new VerifiedChanges(
                    _changes.Replace(foundChange, new AcceptedChange(change, foundChange.Problems.AddRange(problems)))
                );
            }

            return new VerifiedChanges(_changes.Add(new AcceptedChange(change, problems)));
        }

        public VerifiedChanges Reject(IRequestedChange change, Problems problems)
        {
            var foundChange = _changes.Find(verifiedChange => verifiedChange.RequestedChange == change);
            if (foundChange != null)
            {
                return new VerifiedChanges(
                    _changes.Replace(foundChange, new RejectedChange(change, foundChange.Problems.AddRange(problems)))
                );
            }

            return new VerifiedChanges(_changes.Add(new RejectedChange(change, problems)));
        }

        [Obsolete("Please use the Accept and Reject methods")]

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
