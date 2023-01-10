namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using RoadRegistry.BackOffice.Exceptions;
using TicketingService.Abstractions;

internal static class ExceptionExtensions
{
    public static TicketError ToTicketError(this RoadRegistryValidationException exception)
    {
        return new TicketError(exception.Message, exception.ErrorCode);
    }
}
