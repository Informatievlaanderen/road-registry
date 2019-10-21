namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Linq;
    using Model;

    public static class NumberedRoadChangeDbaseRecordProblems
    {
        public static FileError NotNumberedRoadNumber(this IFileDbaseRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotNumberedRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }

        public static FileError NumberedRoadOrdinalOutOfRange(this IFileDbaseRecordProblemBuilder builder, int value)
        {
            return builder
                .Error(nameof(NumberedRoadOrdinalOutOfRange))
                .WithParameter(new ProblemParameter("Actual", value.ToString()))
                .Build();
        }

        public static FileError NumberedRoadDirectionMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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
    }
}
