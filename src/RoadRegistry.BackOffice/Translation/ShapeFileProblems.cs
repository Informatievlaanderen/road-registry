namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class ShapeFileProblems
    {
        // file

        public static FileError ShapeHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(ShapeHeaderFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError HasNoShapeRecords(this IFileProblemBuilder builder)
        {
            return builder.Error(nameof(HasNoShapeRecords)).Build();
        }

        // record

        public static FileError HasShapeRecordFormatError(this IShapeFileRecordProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(HasShapeRecordFormatError))
                .WithParameters(
                    new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError ShapeRecordShapeTypeMismatch(this IShapeFileRecordProblemBuilder builder,
            ShapeType expectedShapeType, ShapeType actualShapeType)
        {
            return builder
                .Error(nameof(ShapeRecordShapeTypeMismatch))
                .WithParameters(
                    new ProblemParameter("ExpectedShapeType", expectedShapeType.ToString()),
                    new ProblemParameter("ActualShapeType", actualShapeType.ToString()))
                .Build();
        }

        public static FileError ShapeRecordGeometryMismatch(this IShapeFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(ShapeRecordGeometryMismatch)).Build();
        }

        public static FileError ShapeRecordGeometryLineCountMismatch(this IShapeFileRecordProblemBuilder builder,
            int expectedLineCount, int actualLineCount)
        {
            return builder
                .Error(nameof(ShapeRecordGeometryLineCountMismatch))
                .WithParameters(
                    new ProblemParameter("ExpectedLineCount", expectedLineCount.ToString()),
                    new ProblemParameter("ActualLineCount", actualLineCount.ToString()))
                .Build();
        }

        public static FileError ShapeRecordGeometrySelfOverlaps(this IShapeFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(ShapeRecordGeometrySelfOverlaps)).Build();
        }

        public static FileError ShapeRecordGeometrySelfIntersects(this IShapeFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(ShapeRecordGeometrySelfIntersects)).Build();
        }
    }
}
