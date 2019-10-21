namespace RoadRegistry.BackOffice.Translation
{
    using System.Linq;
    using Model;

    public static class RoadSegmentSurfaceChangeDbaseRecordProblems
    {
        public static FileError SurfaceTypeMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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
    }
}
