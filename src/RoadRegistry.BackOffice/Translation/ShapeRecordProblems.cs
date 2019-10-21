namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class ShapeRecordProblems
    {
        public static FileError ShapeRecordFormatError(this IFileRecordProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(ShapeRecordFormatError))
                .WithParameters(
                    new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError ShapeRecordShapeTypeMismatch(this IFileRecordProblemBuilder builder,
            ShapeType expectedShapeType, ShapeType actualShapeType)
        {
            return builder
                .Error(nameof(ShapeRecordShapeTypeMismatch))
                .WithParameters(
                    new ProblemParameter("ExpectedShapeType", expectedShapeType.ToString()),
                    new ProblemParameter("ActualShapeType", actualShapeType.ToString()))
                .Build();
        }

        public static FileError ShapeRecordGeometryMismatch(this IFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(ShapeRecordGeometryMismatch)).Build();
        }

        public static FileError ShapeRecordGeometryLineCountMismatch(this IFileRecordProblemBuilder builder,
            int expectedLineCount, int actualLineCount)
        {
            return builder
                .Error(nameof(ShapeRecordGeometryLineCountMismatch))
                .WithParameters(
                    new ProblemParameter("ExpectedLineCount", expectedLineCount.ToString()),
                    new ProblemParameter("ActualLineCount", actualLineCount.ToString()))
                .Build();
        }

        public static FileError ShapeRecordGeometrySelfOverlaps(this IFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(ShapeRecordGeometrySelfOverlaps)).Build();
        }

        public static FileError ShapeRecordGeometrySelfIntersects(this IFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(ShapeRecordGeometrySelfIntersects)).Build();
        }
    }
}
