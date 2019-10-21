namespace RoadRegistry.BackOffice.Translation
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class RoadNodeChangeDbaseRecordProblems
    {
        public static FileError IdentifierNotUnique(this IFileDbaseRecordProblemBuilder builder,
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

        public static FileError RoadNodeTypeMismatch(this IFileDbaseRecordProblemBuilder builder, int actual)
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
    }
}
