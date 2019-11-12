namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("Problems = {" + nameof(_problems) + "}")]
    public class ZipArchiveProblems : IReadOnlyCollection<FileProblem>
    {
        private readonly ImmutableList<FileProblem> _problems;

        public static readonly ZipArchiveProblems None = new ZipArchiveProblems(ImmutableList<FileProblem>.Empty);

        public static ZipArchiveProblems Single(FileProblem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));

            return None.Add(problem);
        }

        public static ZipArchiveProblems Many(params FileProblem[] problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            return None.AddRange(problems);
        }

        public static ZipArchiveProblems Many(IEnumerable<FileProblem> problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            return None.AddRange(problems);
        }

        private ZipArchiveProblems(ImmutableList<FileProblem> problems)
        {
            _problems = problems;
        }

        public bool Equals(ZipArchiveProblems other) => other != null && _problems.SequenceEqual(other._problems);
        public override bool Equals(object obj) => obj is ZipArchiveProblems other && Equals(other);
        public override int GetHashCode() => _problems.Aggregate(0, (current, error) => current ^ error.GetHashCode());

        public IEnumerator<FileProblem> GetEnumerator() => _problems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _problems.Count;

        public ZipArchiveProblems Add(FileProblem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));
            return new ZipArchiveProblems(_problems.Add(problem));
        }

        public ZipArchiveProblems AddRange(IEnumerable<FileProblem> problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));
            return new ZipArchiveProblems(_problems.AddRange(problems));
        }

        public static ZipArchiveProblems operator +(ZipArchiveProblems left, FileProblem right)
            => left.Add(right);

        public static ZipArchiveProblems operator +(ZipArchiveProblems left, IEnumerable<FileProblem> right)
            => left.AddRange(right);

        public static ZipArchiveProblems operator +(ZipArchiveProblems left, ZipArchiveProblems right)
            => left.AddRange(right);

        public ZipArchiveProblems RequiredFileMissing(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(file.ToUpperInvariant(), nameof(RequiredFileMissing)))
            );
        }
    }
}
