namespace RoadRegistry.Infrastructure
{
    using RoadRegistry.Infrastructure.Messages;
    using TicketingService.Abstractions;

    public static class TicketingExtensions
    {
        public static TicketError ToTicketError(this Problem problem)
        {
            var translation = problem.TranslateToDutch();
            return new TicketError(translation.Message, $"{problem.Severity}{translation.Code}");
        }
    }
}
