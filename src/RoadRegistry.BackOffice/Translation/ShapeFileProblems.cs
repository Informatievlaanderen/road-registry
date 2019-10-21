namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class ShapeFileProblems
    {
        public static FileError ShapeHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(ShapeHeaderFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError NoShapeRecords(this IFileProblemBuilder builder)
        {
            return builder.Error(nameof(NoShapeRecords)).Build();
        }

    }
}
