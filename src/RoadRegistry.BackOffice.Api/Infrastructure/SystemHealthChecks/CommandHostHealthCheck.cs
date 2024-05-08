namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthChecks;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RoadRegistry.BackOffice.Abstractions.Uploads;
using TicketingService.Abstractions;

internal class CommandHostHealthCheck : ISystemHealthCheck
{
    private readonly IMediator _mediator;
    private readonly ITicketing _ticketing;

    public CommandHostHealthCheck(IMediator mediator, ITicketing ticketing)
    {
        _mediator = mediator;
        _ticketing = ticketing;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        //TODO-rik aparte events starten voor de extracthost/eventhost te triggeren, via roadnetworkeventwriter

        var ticketId = await _ticketing.CreateTicket(new Dictionary<string, string>
            {
                { "Action", "HealthCheck"}
            }, cancellationToken);
        
        var request = new UploadHealthCheckRequest
        {
            TicketId = ticketId
        };

        await _mediator.Send(request, cancellationToken);

        return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, TimeSpan.FromMinutes(1), cancellationToken);
    }
}
