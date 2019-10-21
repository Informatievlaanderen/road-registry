namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;

    public static class NationalRoadChangeDbaseRecordProblems
    {
        public static FileError NotNationalRoadNumber(this IFileDbaseRecordProblemBuilder builder, string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            return builder
                .Error(nameof(NotNationalRoadNumber))
                .WithParameter(new ProblemParameter("Number", number.ToUpperInvariant()))
                .Build();
        }
    }
}
