namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using BackOffice.Core;
using BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extensions;
using TicketingService.Abstractions;

public static class ExceptionExtensions
{
    public static TicketError ToTicketError(this RoadRegistryValidationException exception)
    {
        return new TicketError(exception.Message, exception.ErrorCode);
    }

    public static TicketError ToTicketError(this Problem problem)
    {
        var translation = problem.TranslateToDutch();
        return new TicketError(translation.Message, translation.Code);
    }
}
