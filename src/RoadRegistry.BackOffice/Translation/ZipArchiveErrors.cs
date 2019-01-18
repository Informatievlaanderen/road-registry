namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Aiv.Vbr.Shaperon;
    using Model;

    public class ZipArchiveErrors: IReadOnlyCollection<Error>
    {
        private readonly ImmutableList<Error> _errors;

        public static readonly ZipArchiveErrors None = new ZipArchiveErrors(ImmutableList<Error>.Empty);

        private ZipArchiveErrors(ImmutableList<Error> errors)
        {
            _errors = errors;
        }

        public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _errors.Count;

        public ZipArchiveErrors CombineWith(IEnumerable<Error> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            return new ZipArchiveErrors(_errors.AddRange(errors));
        }

        public ZipArchiveErrors RequiredFileMissing(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(RequiredFileMissing),
                    new ProblemParameter("File",file)))
            );
        }

        public ZipArchiveErrors ShapeHeaderFormatError(string file, Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(ShapeHeaderFormatError),
                    new ProblemParameter("File", file),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }

        public ZipArchiveErrors ShapeRecordFormatError(string file, RecordNumber? afterRecordNumber, Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (afterRecordNumber.HasValue)
            {
                return new ZipArchiveErrors(_errors.Add(
                    new Error(
                        nameof(ShapeRecordFormatError),
                        new ProblemParameter("File", file),
                        new ProblemParameter("AfterRecordNumber", afterRecordNumber.Value.ToString()),
                        new ProblemParameter("Exception", exception.ToString())))
                );
            }

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(ShapeRecordFormatError),
                    new ProblemParameter("File", file),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }

        public ZipArchiveErrors NoShapeRecords(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(NoShapeRecords),
                    new ProblemParameter("File", file)))
            );
        }

        public ZipArchiveErrors ShapeRecordShapeTypeMismatch(string file, RecordNumber recordNumber, ShapeType expectedShapeType, ShapeType actualShapeType)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(ShapeRecordShapeTypeMismatch),
                    new ProblemParameter("File", file),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("ExpectedShapeType", expectedShapeType.ToString()),
                    new ProblemParameter("ActualShapeType", actualShapeType.ToString())))
            );
        }

        public ZipArchiveErrors ShapeRecordGeometryMismatch(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(ShapeRecordShapeTypeMismatch),
                    new ProblemParameter("File", file),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveErrors ShapeRecordGeometryLineCountMismatch(string file, RecordNumber recordNumber, int expectedLineCount, int actualLineCount)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(ShapeRecordGeometryLineCountMismatch),
                    new ProblemParameter("File", file),
                    new ProblemParameter("RecordNumber", recordNumber.ToString()),
                    new ProblemParameter("ExpectedLineCount", expectedLineCount.ToString()),
                    new ProblemParameter("ActualLineCount", actualLineCount.ToString())))
            );
        }

        public ZipArchiveErrors ShapeRecordGeometrySelfOverlaps(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(ShapeRecordGeometrySelfOverlaps),
                    new ProblemParameter("File", file),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveErrors ShapeRecordGeometrySelfIntersects(string file, RecordNumber recordNumber)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(ShapeRecordGeometrySelfIntersects),
                    new ProblemParameter("File", file),
                    new ProblemParameter("RecordNumber", recordNumber.ToString())))
            );
        }

        public ZipArchiveErrors DbaseHeaderFormatError(string file, Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(DbaseHeaderFormatError),
                    new ProblemParameter("File", file),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }

        public ZipArchiveErrors NoDbaseRecords(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(NoDbaseRecords),
                    new ProblemParameter("File", file)))
            );
        }

        public ZipArchiveErrors DbaseRecordFormatError(string file, int? afterRecordNumber, Exception exception)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (afterRecordNumber.HasValue)
            {
                return new ZipArchiveErrors(_errors.Add(
                    new Error(
                        nameof(DbaseRecordFormatError),
                        new ProblemParameter("File", file),
                        new ProblemParameter("AfterRecordNumber", afterRecordNumber.Value.ToString()),
                        new ProblemParameter("Exception", exception.ToString())))
                );
            }

            return new ZipArchiveErrors(_errors.Add(
                new Error(
                    nameof(DbaseRecordFormatError),
                    new ProblemParameter("File", file),
                    new ProblemParameter("Exception", exception.ToString())))
            );
        }
    }
}
