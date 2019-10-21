namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Globalization;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class DbaseRecordProblems
    {
        public static FileError DbaseRecordFormatError(this IFileDbaseRecordProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(DbaseRecordFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError IdentifierMissing(this IFileDbaseRecordProblemBuilder builder)
        {
            return builder.Error(nameof(IdentifierMissing)).Build();
        }

        public static FileError IdentifierZero(this IFileDbaseRecordProblemBuilder builder)
        {
            return builder.Error(nameof(IdentifierZero)).Build();
        }

        public static FileError FieldValueNull(this IFileDbaseRecordProblemBuilder builder, DbaseFieldValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return builder
                .Error(nameof(FieldValueNull))
                .WithParameter(new ProblemParameter("Field", value.Field.Name.ToString()))
                .Build();
        }

        public static FileError RoadSegmentIdOutOfRange(this IFileDbaseRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(RoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        private static readonly NumberFormatInfo Provider = new NumberFormatInfo
        {
            NumberDecimalSeparator = "."
        };

        public static FileError FromPositionOutOfRange(this IFileDbaseRecordProblemBuilder builder, double value)
        {
            return builder
                .Error(nameof(FromPositionOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
                .Build();
        }

        public static FileError ToPositionOutOfRange(this IFileDbaseRecordProblemBuilder builder, double value)
        {
            return builder
                .Error(nameof(ToPositionOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
                .Build();
        }

        public static FileError IdentifierNotUnique(this IFileDbaseRecordProblemBuilder builder,
            AttributeId identifier,
            RecordNumber takenByRecordNumber)
        {
            return builder
                .Error(nameof(IdentifierNotUnique))
                .WithParameters(
                    new ProblemParameter("Identifier", identifier.ToString()),
                    new ProblemParameter("TakenByRecordNumber", takenByRecordNumber.ToString())
                )
                .Build();
        }
    }
}
