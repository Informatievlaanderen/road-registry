namespace RoadRegistry.Infrastructure
{
    using DutchTranslations;
    using RoadRegistry.Infrastructure.Messages;
    using TicketingService.Abstractions;

    public static class TicketingExtensions
    {
        public static TicketError ToTicketError(this Problem problem, IProblemTranslator translator)
        {
            var translation = problem.TranslateToDutch(translator);
            return new TicketError(translation.Message, $"{problem.Severity}{translation.Code}");
        }
    }
}
