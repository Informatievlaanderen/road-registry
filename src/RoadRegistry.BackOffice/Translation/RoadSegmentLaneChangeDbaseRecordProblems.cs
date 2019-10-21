namespace RoadRegistry.BackOffice.Translation
{
    using System.Linq;
    using Model;

    public static class RoadSegmentLaneChangeDbaseRecordProblems
    {
        public static FileError LaneCountOutOfRange(this IFileDbaseRecordProblemBuilder builder, int count)
        {
            return builder
                .Error(nameof(LaneCountOutOfRange))
                .WithParameter(new ProblemParameter("Actual", count.ToString()))
                .Build();
        }

        public static FileError LaneDirectionMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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
    }
}
