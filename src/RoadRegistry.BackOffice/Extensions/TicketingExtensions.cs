namespace RoadRegistry.BackOffice.Messages
{
    using DutchTranslations;
    using TicketingService.Abstractions;

    public static class TicketingExtensions
    {
        public static TicketError ToTicketError(this FileProblem problem)
        {
            var translation = problem.TranslateToDutch();
            return new TicketError(translation.Message, $"{problem.File}_{problem.Severity}{translation.Code}");
        }
    }
}
