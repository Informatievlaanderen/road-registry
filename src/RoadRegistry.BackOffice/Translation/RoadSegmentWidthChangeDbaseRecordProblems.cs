namespace RoadRegistry.BackOffice.Translation
{
    using Model;

    public static class RoadSegmentWidthChangeDbaseRecordProblems
    {
        public static FileError WidthOutOfRange(this IFileDbaseRecordProblemBuilder builder, int count)
        {
            return builder
                .Error(nameof(WidthOutOfRange))
                .WithParameter(new ProblemParameter("Actual", count.ToString()))
                .Build();
        }
    }
}
