namespace RoadRegistry.BackOffice.Translation
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class RoadSegmentChangeDbaseRecordProblems
    {
        public static FileError IdentifierNotUnique(this IFileDbaseRecordProblemBuilder builder,
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

        public static FileError RoadSegmentAccessRestrictionMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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

        public static FileError RoadSegmentStatusMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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

        public static FileError RoadSegmentCategoryMismatch(this IFileDbaseRecordProblemBuilder builder, string actual)
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

        public static FileError RoadSegmentGeometryDrawMethodMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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

        public static FileError RoadSegmentMorphologyMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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

        public static FileError BeginRoadNodeIdOutOfRange(this IFileDbaseRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(BeginRoadNodeIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError EndRoadNodeIdOutOfRange(this IFileDbaseRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(EndRoadNodeIdOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }
    }
}
