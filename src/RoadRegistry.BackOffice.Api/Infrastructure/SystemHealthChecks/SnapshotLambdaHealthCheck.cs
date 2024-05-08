namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthChecks
{
    using System;
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

            return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}
