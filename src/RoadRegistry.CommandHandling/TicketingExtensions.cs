namespace RoadRegistry.BackOffice.Messages
{
    using Extensions;
    using TicketingService.Abstractions;

    public static class TicketingExtensions
    {
        public static TicketError ToTicketError(this Messages.Problem problem)
        {
            var translation = problem.TranslateToDutch();
            return new TicketError(translation.Message, $"{problem.Severity}{translation.Code}");
        }
    }
}
