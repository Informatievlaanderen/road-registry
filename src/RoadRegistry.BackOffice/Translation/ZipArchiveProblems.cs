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

        public ZipArchiveProblems CombineWith(IEnumerable<FileProblem> problems)
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

        public ZipArchiveProblems ShapeHeaderFormatError(string file, Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(ShapeHeaderFormatError),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }

        public ZipArchiveProblems ShapeRecordFormatError(string file, RecordNumber? afterRecordNumber,
            Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (afterRecordNumber.HasValue)
            {
                return new ZipArchiveProblems(_problems.Add(
                    new FileError(
                        file.ToUpperInvariant(),
                        nameof(ShapeRecordFormatError),
                        new ProblemParameter("AfterRecordNumber", afterRecordNumber.Value.ToString()),
                        new ProblemParameter("Exception", exception.ToString())))
                );
            }

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(ShapeRecordFormatError),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }

        public ZipArchiveProblems NoShapeRecords(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(file.ToUpperInvariant(), nameof(NoShapeRecords)
                )));
        }

        public ZipArchiveProblems ShapeRecordShapeTypeMismatch(string file, RecordNumber recordNumber,
            ShapeType expectedShapeType, ShapeType actualShapeType)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(ShapeRecordShapeTypeMismatch),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("ExpectedShapeType", expectedShapeType.ToString()),
                    new ProblemParameter("ActualShapeType", actualShapeType.ToString())))
            );
        }

        public ZipArchiveProblems ShapeRecordGeometryMismatch(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(ShapeRecordShapeTypeMismatch),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveProblems ShapeRecordGeometryLineCountMismatch(string file, RecordNumber recordNumber,
            int expectedLineCount, int actualLineCount)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(ShapeRecordGeometryLineCountMismatch),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("ExpectedLineCount", expectedLineCount.ToString()),
                    new ProblemParameter("ActualLineCount", actualLineCount.ToString())))
            );
        }

        public ZipArchiveProblems ShapeRecordGeometrySelfOverlaps(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(ShapeRecordGeometrySelfOverlaps),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveProblems ShapeRecordGeometrySelfIntersects(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(ShapeRecordGeometrySelfIntersects),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveProblems DbaseHeaderFormatError(string file, Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(DbaseHeaderFormatError),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }

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

        public ZipArchiveProblems DbaseSchemaMismatch(string file, DbaseSchema expectedSchema, DbaseSchema actualSchema)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            return new ZipArchiveProblems(_problems.Add(
                new FileError(
                    file.ToUpperInvariant(),
                    nameof(DbaseSchemaMismatch),
                    new ProblemParameter("ExpectedSchema", Describe(expectedSchema)),
                    new ProblemParameter("ActualSchema", Describe(actualSchema))
                )
            ));
        }

        private static string Describe(DbaseSchema schema)
        {
            var builder = new StringBuilder();
            var index = 0;
            foreach (var field in schema.Fields)
            {
                if (index > 0) builder.Append(",");
                builder.Append(field.Name.ToString());
                builder.Append("[");
                builder.Append(field.FieldType.ToString());
                builder.Append("(");
                builder.Append(field.Length.ToString());
                builder.Append(",");
                builder.Append(field.DecimalCount.ToString());
                builder.Append(")");
                builder.Append("]");
                index++;
            }

            return builder.ToString();
        }
    }
}
