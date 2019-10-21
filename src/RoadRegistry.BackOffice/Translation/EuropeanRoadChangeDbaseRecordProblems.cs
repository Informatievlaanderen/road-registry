namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;

    public static class EuropeanRoadChangeDbaseRecordProblems
    {
        public static FileError NotEuropeanRoadNumber(this IFileDbaseRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotEuropeanRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }
    }
}
