namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck.HealthChecks;

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Uploads;
using Framework;
using Messages;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TicketingService.Abstractions;

internal class CommandHostSystemHealthCheck : ISystemHealthCheck
{
    private readonly ITicketing _ticketing;
    private readonly RoadNetworkUploadsBlobClient _uploadsBlobClient;
    private readonly IHealthCommandQueue _healthCommandQueue;
    private readonly SystemHealthCheckOptions _options;

    public CommandHostSystemHealthCheck(
        ITicketing ticketing,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        IHealthCommandQueue healthCommandQueue,
        SystemHealthCheckOptions options)
    {
        _ticketing = ticketing;
        _uploadsBlobClient = uploadsBlobClient;
        _healthCommandQueue = healthCommandQueue;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var ticketId = await _ticketing.CreateTicket(new Dictionary<string, string>
            {
                { "Action", "HealthCheck"}
            }, cancellationToken);

        var fileName = "healthcheck.bin";
        await _uploadsBlobClient.CreateDummyFile(fileName, cancellationToken);

        var command = new Command(new CheckCommandHostHealth
        {
            TicketId = ticketId,
            AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
            FileName = fileName
        });
        await _healthCommandQueue.WriteAsync(command, cancellationToken);

        return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, _options.CheckTimeout, cancellationToken);
    }
}
