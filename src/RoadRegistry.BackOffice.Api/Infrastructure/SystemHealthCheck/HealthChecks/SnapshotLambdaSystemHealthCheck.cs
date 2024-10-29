namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck.HealthChecks
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using RoadRegistry.Snapshot.Handlers.Sqs.Infrastructure;
    using TicketingService.Abstractions;

    internal class SnapshotLambdaSystemHealthCheck : ISystemHealthCheck
    {
        private readonly IMediator _mediator;
        private readonly ITicketing _ticketing;
        private readonly SystemHealthCheckOptions _options;

        public SnapshotLambdaSystemHealthCheck(
            IMediator mediator,
            ITicketing ticketing,
            SystemHealthCheckOptions options)
        {
            _mediator = mediator;
            _ticketing = ticketing;
            _options = options;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SnapshotLambdaHealthCheckSqsRequest
            {
                AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            }, cancellationToken);

            var ticketId = Guid.Parse(result.Location.ToString().Split('/').Last());

            return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, _options.CheckTimeout, cancellationToken);
        }
    }
}