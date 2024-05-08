namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthChecks
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using RoadRegistry.Snapshot.Handlers.Sqs.Infrastructure;
    using TicketingService.Abstractions;

    internal class SnapshotLambdaHealthCheck : ISystemHealthCheck
    {
        private readonly IMediator _mediator;
        private readonly ITicketing _ticketing;

        public SnapshotLambdaHealthCheck(IMediator mediator, ITicketing ticketing)
        {
            _mediator = mediator;
            _ticketing = ticketing;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SnapshotLambdaHealthCheckSqsRequest(), cancellationToken);

            var ticketId = Guid.Parse(result.Location.ToString().Split('/').Last());

            var sw = Stopwatch.StartNew();
            var timeoutAt = TimeSpan.FromMinutes(1);

            while (true)
            {
                if (sw.Elapsed > timeoutAt)
                {
                    return HealthCheckResult.Unhealthy($"Timed out while waiting for ticket ({ticketId}) to complete at {sw.Elapsed}");
                }

                var ticket = await _ticketing.Get(ticketId, cancellationToken);

                if (ticket != null)
                {
                    if (ticket.Status == TicketStatus.Complete)
                    {
                        return HealthCheckResult.Healthy();
                    }

                    if (ticket.Status == TicketStatus.Error)
                    {
                        return HealthCheckResult.Unhealthy($"Ticket ({ticketId}) resulted in error: {ticket.Result?.ResultAsJson}");
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
