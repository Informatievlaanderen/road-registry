namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public class Problems : IReadOnlyCollection<Problem>
    {
        private readonly ImmutableList<Problem> _problems;

        public static readonly Problems None = new Problems(ImmutableList<Problem>.Empty);

        public static Problems Single(Problem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));

            return None.Add(problem);
        }

        public static Problems Many(params Problem[] problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            return None.AddRange(problems);
        }

        public static Problems Many(IEnumerable<Problem> problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            return None.AddRange(problems);
        }

        private Problems(ImmutableList<Problem> problems)
        {
            _problems = problems;
        }

        public IEnumerator<Problem> GetEnumerator() => _problems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _problems.Count;

        public Problems Add(Problem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));
            return new Problems(_problems.Add(problem));
        }

        public Problems AddRange(IEnumerable<Problem> problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));
            return new Problems(_problems.AddRange(problems));
        }

        public Problems AddRange(Problems problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));
            return new Problems(_problems.AddRange(problems));
        }

        public static Problems operator +(Problems left, Problem right)
            => left.Add(right);

        public static Problems operator +(Problems left, IEnumerable<Problem> right)
            => left.AddRange(right);

        public static Problems operator +(Problems left, Problems right)
            => left.AddRange(right);
    }
}
