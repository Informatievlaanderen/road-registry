namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public interface ISystemHealthCheckService
{
    Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken);
}

public class SystemHealthCheckService : ISystemHealthCheckService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SystemHealthCheckOptions _options;

    public SystemHealthCheckService(
        IServiceScopeFactory scopeFactory,
        SystemHealthCheckOptions options)
    {
        _scopeFactory = scopeFactory.ThrowIfNull();
        _options = options.ThrowIfNull();
    }

    public async Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken)
    {
        var totalTime = Stopwatch.StartNew();

        var tasks = new Task<HealthReportEntry>[_options.HealthCheckTypes.Count];
        var index = 0;

        foreach (var healthCheckType in _options.HealthCheckTypes)
        {
            tasks[index++] = Task.Run(() => RunCheckAsync(healthCheckType, cancellationToken), cancellationToken);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        index = 0;
        var entries = new Dictionary<string, HealthReportEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var healthCheckType in _options.HealthCheckTypes)
        {
            entries[healthCheckType.Name] = tasks[index++].Result;
        }

        var totalElapsedTime = totalTime.Elapsed;
        var report = new HealthReport(entries, totalElapsedTime);
        return report;
    }

    private async Task<HealthReportEntry> RunCheckAsync(Type healthCheckType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var scope = _scopeFactory.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var healthCheck = (ISystemHealthCheck)scope.ServiceProvider.GetRequiredService(healthCheckType);
            var registration = new HealthCheckRegistration(healthCheckType.Name, healthCheck, HealthStatus.Unhealthy, new[] { WellKnownHealthCheckTags.System });

            var stopwatch = Stopwatch.StartNew();
            var context = new HealthCheckContext { Registration = registration };

            HealthReportEntry entry;
            CancellationTokenSource? timeoutCancellationTokenSource = null;
            try
            {
                HealthCheckResult result;

                var checkCancellationToken = cancellationToken;
                if (registration.Timeout > TimeSpan.Zero)
                {
                    timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCancellationTokenSource.CancelAfter(registration.Timeout);
                    checkCancellationToken = timeoutCancellationTokenSource.Token;
                }

                result = await healthCheck.CheckHealthAsync(context, checkCancellationToken).ConfigureAwait(false);

                var duration = stopwatch.Elapsed;

                entry = new HealthReportEntry(
                    status: result.Status,
                    description: result.Description,
                    duration: duration,
                    exception: result.Exception,
                    data: result.Data,
                    tags: registration.Tags);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                var duration = stopwatch.Elapsed;
                entry = new HealthReportEntry(
                    status: registration.FailureStatus,
                    description: "A timeout occurred while running check.",
                    duration: duration,
                    exception: ex,
                    data: null,
                    tags: registration.Tags);
            }

            // Allow cancellation to propagate if it's not a timeout.
            catch (Exception ex) when (ex as OperationCanceledException == null)
            {
                var duration = stopwatch.Elapsed;
                entry = new HealthReportEntry(
                    status: registration.FailureStatus,
                    description: ex.Message,
                    duration: duration,
                    exception: ex,
                    data: null,
                    tags: registration.Tags);
            }

            finally
            {
                timeoutCancellationTokenSource?.Dispose();
            }

            return entry;
        }
    }
}

public sealed class SystemHealthCheckOptions
{
    public required ICollection<Type> HealthCheckTypes { get; init; }
}
