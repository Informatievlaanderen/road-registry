namespace RoadRegistry.Model
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class Warnings : IReadOnlyCollection<Warning>
    {
        private readonly ImmutableList<Warning> _warnings;

        public static readonly Warnings None = new Warnings(ImmutableList<Warning>.Empty);

        private Warnings(ImmutableList<Warning> warnings)
        {
            _warnings = warnings;
        }

        public IEnumerator<Warning> GetEnumerator() => _warnings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _warnings.Count;
    }
}
