namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System.Collections.Generic;
using System.Linq;
using BackOffice.Core;
using BackOffice.Exceptions;
using CommandHandling;
using RoadRegistry.BackOffice.Extensions;
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

    public static TicketError ToTicketError(this RoadSegmentsProblemsException exception)
    {
        return new TicketError(exception.RoadSegmentsProblems
            .SelectMany(x =>
            {
                var errorContext = new Dictionary<string, object>
                {
                    { "WegsegmentId", x.Key.ToInt32() }
                };

                return x.Value.Select(problem => problem.ToTicketError(errorContext));
            })
            .ToArray()
        );
    }

    public static TicketError ToTicketError(this Problem problem, Dictionary<string, object> errorContext = null)
    {
        var translation = problem.TranslateToDutch();
        return new TicketError(translation.Message, translation.Code, errorContext ?? []);
    }
}
