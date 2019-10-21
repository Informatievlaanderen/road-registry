namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class DbaseFileProblems
    {
        public static FileError NoDbaseRecords(this IFileProblemBuilder builder)
        {
            return builder.Error(nameof(NoDbaseRecords)).Build();
        }

        public static FileError DbaseHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(DbaseHeaderFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError DbaseSchemaMismatch(this IFileProblemBuilder builder, DbaseSchema expectedSchema, DbaseSchema actualSchema)
        {
            if (expectedSchema == null) throw new ArgumentNullException(nameof(expectedSchema));
            if (actualSchema == null) throw new ArgumentNullException(nameof(actualSchema));

            return builder
                .Error(nameof(DbaseSchemaMismatch))
                .WithParameters(
                    new ProblemParameter("ExpectedSchema", Describe(expectedSchema)),
                    new ProblemParameter("ActualSchema", Describe(actualSchema))
                )
                .Build();
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
