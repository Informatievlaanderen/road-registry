namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class DbaseFileProblems
    {
        // file

        public static FileProblem HasNoDbaseRecords(this IFileProblemBuilder builder, bool treatAsWarning)
        {
            if (treatAsWarning)
                return builder.Warning(nameof(HasNoDbaseRecords)).Build();
            return builder.Error(nameof(HasNoDbaseRecords)).Build();
        }

        public static FileError HasDbaseHeaderFormatError(this IFileProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(HasDbaseHeaderFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError HasDbaseSchemaMismatch(this IFileProblemBuilder builder, DbaseSchema expectedSchema, DbaseSchema actualSchema)
        {
            if (expectedSchema == null) throw new ArgumentNullException(nameof(expectedSchema));
            if (actualSchema == null) throw new ArgumentNullException(nameof(actualSchema));

            return builder
                .Error(nameof(HasDbaseSchemaMismatch))
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

        // record

        public static FileError HasDbaseRecordFormatError(this IDbaseFileRecordProblemBuilder builder, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            return builder
                .Error(nameof(HasDbaseRecordFormatError))
                .WithParameter(new ProblemParameter("Exception", exception.ToString()))
                .Build();
        }

        public static FileError IdentifierZero(this IDbaseFileRecordProblemBuilder builder)
        {
            return builder.Error(nameof(IdentifierZero)).Build();
        }

        public static FileError RequiredFieldIsNull(this IDbaseFileRecordProblemBuilder builder, DbaseField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            return builder
                .Error(nameof(RequiredFieldIsNull))
                .WithParameter(new ProblemParameter("Field", field.Name.ToString()))
                .Build();
        }

        public static FileError RoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
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

        public static FileError FromPositionOutOfRange(this IDbaseFileRecordProblemBuilder builder, double value)
        {
            return builder
                .Error(nameof(FromPositionOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
                .Build();
        }

        public static FileError ToPositionOutOfRange(this IDbaseFileRecordProblemBuilder builder, double value)
        {
            return builder
                .Error(nameof(ToPositionOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString(Provider)))
                .Build();
        }

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
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

        // european road

        public static FileError NotEuropeanRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotEuropeanRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }

        // grade separated junction

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
            GradeSeparatedJunctionId identifier,
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

        public static FileError GradeSeparatedJunctionTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(GradeSeparatedJunctionTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", GradeSeparatedJunctionType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError UpperRoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(UpperRoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError LowerRoadSegmentIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(LowerRoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        // national road

        public static FileError NotNationalRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotNationalRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }

        // numbered road

        public static FileError NotNumberedRoadNumber(this IDbaseFileRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotNumberedRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }

        public static FileError NumberedRoadOrdinalOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(NumberedRoadOrdinalOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError NumberedRoadDirectionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(NumberedRoadDirectionMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentNumberedRoadDirection.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // record type

        public static FileError RecordTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RecordTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RecordType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // road node

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
            RoadNodeId identifier,
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

        public static FileError RoadNodeTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadNodeType))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadNodeType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // road segment

        public static FileError IdentifierNotUnique(this IDbaseFileRecordProblemBuilder builder,
            RoadSegmentId identifier,
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

        public static FileError RoadSegmentAccessRestrictionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentAccessRestrictionMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentAccessRestriction.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError RoadSegmentStatusMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentStatusMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentStatus.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError RoadSegmentCategoryMismatch(this IDbaseFileRecordProblemBuilder builder, string actual)
        {
            return builder
                .Error(nameof(RoadSegmentCategoryMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentCategory.ByIdentifier.Keys.Select(key => key))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual))
                .Build();
        }

        public static FileError RoadSegmentGeometryDrawMethodMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentGeometryDrawMethodMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentGeometryDrawMethod.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError RoadSegmentMorphologyMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(RoadSegmentMorphologyMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentMorphology.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError BeginRoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(BeginRoadNodeIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError EndRoadNodeIdOutOfRange(this IDbaseFileRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(EndRoadNodeIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        // lane

        public static FileError LaneCountOutOfRange(this IDbaseFileRecordProblemBuilder builder, int count)
        {
            return builder
                .Error(nameof(LaneCountOutOfRange))
                .WithParameter(new ProblemParameter("Actual", count.ToString()))
                .Build();
        }

        public static FileError LaneDirectionMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(LaneDirectionMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentLaneDirection.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // surface

        public static FileError SurfaceTypeMismatch(this IDbaseFileRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(SurfaceTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", RoadSegmentSurfaceType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        // width

        public static FileError WidthOutOfRange(this IDbaseFileRecordProblemBuilder builder, int count)
        {
            return builder
                .Error(nameof(WidthOutOfRange))
                .WithParameter(new ProblemParameter("Actual", count.ToString()))
                .Build();
        }
    }
}
