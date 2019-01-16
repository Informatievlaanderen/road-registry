namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    public class VerifiedChanges
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

        public void RecordUsing(Action<object> applier)
        {
            if (applier == null) throw new ArgumentNullException(nameof(applier));

            if (_changes.Count == 0) return;

            if (_changes.OfType<RejectedChange>().Any())
            {
                applier(new Messages.RoadNetworkChangesRejected
                {
                    Changes = _changes
                        .OfType<RejectedChange>()
                        .Select(change => change.Translate())
                        .ToArray()
                });
            }
            else
            {
                applier(new Messages.RoadNetworkChangesAccepted
                {
                    Changes = _changes
                        .OfType<AcceptedChange>()
                        .Select(change => change.Translate())
                        .ToArray()
                });
            }
        }
    }
}
