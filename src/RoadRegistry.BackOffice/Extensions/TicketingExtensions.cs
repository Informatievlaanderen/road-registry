namespace RoadRegistry.BackOffice.Messages
{
    using RoadRegistry.Extracts.DutchTranslations;
    using RoadRegistry.Extracts.Messages;
    using TicketingService.Abstractions;

    public static class TicketingExtensions
    {
        public static TicketError ToTicketError(this FileProblem problem, FileProblemTranslator translator)
        {
            var translation = translator.Translate(problem);
            return new TicketError(translation.Message, $"{problem.File}_{problem.Severity}{translation.Code}");
        }
    }
}
