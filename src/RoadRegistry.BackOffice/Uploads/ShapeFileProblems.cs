namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;

public static class ShapeFileProblems
{
    public static FileError HasNoShapeRecords(this IFileProblemBuilder builder)
    {
        return builder.ThisError(nameof(HasNoShapeRecords)).Build();
    }

    // record

    public static FileError HasShapeRecordFormatError(this IShapeFileRecordProblemBuilder builder, Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        return builder
            .ThisError(nameof(HasShapeRecordFormatError))
            .WithParameters(
                new ProblemParameter("Exception", exception.ToString()))
            .Build();
    }
    // file

    public static FileError ShapeHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        return builder
            .ThisError(nameof(ShapeHeaderFormatError))
            .WithParameter(new ProblemParameter("Exception", exception.ToString()))
            .Build();
    }

    public static FileError ShapeRecordGeometryLineCountMismatch(this IShapeFileRecordProblemBuilder builder,
        int expectedLineCount, int actualLineCount)
    {
        return builder
            .ThisError(nameof(ShapeRecordGeometryLineCountMismatch))
            .WithParameters(
                new ProblemParameter("ExpectedLineCount", expectedLineCount.ToString()),
                new ProblemParameter("ActualLineCount", actualLineCount.ToString()))
            .Build();
    }

    public static FileError ShapeRecordGeometryMismatch(this IShapeFileRecordProblemBuilder builder)
    {
        return builder.ThisError(nameof(ShapeRecordGeometryMismatch)).Build();
    }

    public static FileError ShapeRecordGeometrySelfIntersects(this IShapeFileRecordProblemBuilder builder)
    {
        return builder.ThisError(nameof(ShapeRecordGeometrySelfIntersects)).Build();
    }

    public static FileError ShapeRecordGeometrySelfOverlaps(this IShapeFileRecordProblemBuilder builder)
    {
        return builder.ThisError(nameof(ShapeRecordGeometrySelfOverlaps)).Build();
    }

    public static FileError ShapeRecordShapeTypeMismatch(this IShapeFileRecordProblemBuilder builder,
        ShapeType expectedShapeType, ShapeType actualShapeType)
    {
        return builder
            .ThisError(nameof(ShapeRecordShapeTypeMismatch))
            .WithParameters(
                new ProblemParameter("ExpectedShapeType", expectedShapeType.ToString()),
                new ProblemParameter("ActualShapeType", actualShapeType.ToString()))
            .Build();
    }
}
