namespace RoadRegistry.BackOffice.Translation
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class GradeSeparatedJunctionChangeDbaseRecordProblems
    {
        public static FileError IdentifierNotUnique(this IFileDbaseRecordProblemBuilder builder,
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

        public static FileError GradeSeparatedJunctionTypeMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
        {
            return builder
                .Error(nameof(GradeSeparatedJunctionTypeMismatch))
                .WithParameter(
                    new ProblemParameter(
                        "ExpectedOneOf",
                        string.Join(",", GradeSeparatedJunctionType.ByIdentifier.Keys.Select(key => key.ToString()))
                    )
                )
                .WithParameter(new ProblemParameter("Actual", actual.ToString()))
                .Build();
        }

        public static FileError UpperRoadSegmentIdOutOfRange(this IFileDbaseRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(UpperRoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError LowerRoadSegmentIdOutOfRange(this IFileDbaseRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(LowerRoadSegmentIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }
    }
}
