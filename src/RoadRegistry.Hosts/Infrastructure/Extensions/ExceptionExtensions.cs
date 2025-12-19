namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System.Collections.Generic;
using System.Linq;
using BackOffice.Exceptions;
using RoadRegistry.Infrastructure;
using TicketingService.Abstractions;
using ValueObjects.Problems;

public static class ExceptionExtensions
{
    public static TicketError ToTicketError(this RoadRegistryValidationException exception)
    {
        return new TicketError(exception.Message, exception.ErrorCode);
    }

    public static TicketError ToTicketError(this RoadRegistryProblemsException exception)
    {
        return new TicketError(exception.Problems
            .Select(problem => problem.ToTicketError())
            .ToArray()
        );
    }

    public static TicketError ToTicketError(this Problem problem)
    {
        var translation = problem.TranslateToDutch();

        var errorContext = new Dictionary<string, object>();
        if (problem.Context is not null)
        {
            foreach (var parameter in problem.Context.Parameters)
            {
                errorContext.Add(parameter.Name, parameter.Value);
            }
        }

        return new TicketError(translation.Message, translation.Code, errorContext);
    }
}
