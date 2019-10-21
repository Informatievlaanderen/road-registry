namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    [DebuggerDisplay("Problems = {" + nameof(_problems) + "}")]
    public class ZipArchiveProblems : IReadOnlyCollection<FileProblem>
    {
        private readonly ImmutableList<FileProblem> _problems;

        public static readonly ZipArchiveProblems None = new ZipArchiveProblems(ImmutableList<FileProblem>.Empty);

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

        public ZipArchiveProblems RequiredFileMissing(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(file.ToUpperInvariant(), nameof(RequiredFileMissing)))
            );
        }

        public static ZipArchiveProblems operator +(ZipArchiveProblems left, FileProblem right)
            => left.Add(right);

        public static ZipArchiveProblems operator +(ZipArchiveProblems left, IEnumerable<FileProblem> right)
            => left.AddRange(right);

        public static ZipArchiveProblems operator +(ZipArchiveProblems left, ZipArchiveProblems right)
            => left.AddRange(right);



        public ZipArchiveProblems NoDbaseRecords(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(NoDbaseRecords)))
            );
        }

        public ZipArchiveProblems DbaseRecordFormatError(string file, RecordNumber recordNumber, Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(DbaseRecordFormatError),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }

        public ZipArchiveProblems IdentifierZero(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(IdentifierZero),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveProblems IdentifierMissing(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(IdentifierMissing),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveProblems NotEuropeanRoadNumber(string file, string number, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(NotEuropeanRoadNumber),
                    new ProblemParameter("Number", number?.ToUpperInvariant() ?? "<null>"),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveProblems IdentifierNotUnique(string file, AttributeId identifier, RecordNumber recordNumber,
            RecordNumber takenByRecordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(IdentifierNotUnique),
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
            ));
        }

        public ZipArchiveProblems IdentifierNotUnique(string file, GradeSeparatedJunctionId identifier,
            RecordNumber recordNumber, RecordNumber takenByRecordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(IdentifierNotUnique),
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
            ));
        }

        public ZipArchiveProblems IdentifierNotUnique(string file, RoadNodeId identifier, RecordNumber recordNumber,
            RecordNumber takenByRecordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(IdentifierNotUnique),
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
            ));
        }

        public ZipArchiveProblems IdentifierNotUnique(string file, RoadSegmentId identifier, RecordNumber recordNumber,
            RecordNumber takenByRecordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(IdentifierNotUnique),
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
            ));
        }


    }
}
