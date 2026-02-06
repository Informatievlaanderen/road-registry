namespace RoadRegistry.BackOffice.Api.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure;

public sealed class JobScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly Type _jobType;
    private readonly TimeSpan _period;

    public JobScheduler(
        IServiceScopeFactory scopeFactory,
        ILoggerFactory loggerFactory,
        Type jobType,
        TimeSpan period)
    {
        _scopeFactory = scopeFactory;
        _logger = loggerFactory.CreateLogger(jobType);
        _jobType = jobType;
        _period = period;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DelayUntilNextTickAsync(stoppingToken);

        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        var jobName = _jobType.FullName ?? _jobType.Name;
        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("Scheduled job starting: {JobType} at {StartedAt}", jobName, startedAt);

        try
        {
            using var scope = _scopeFactory.CreateScope();

            var job = (IScheduledJob)scope.ServiceProvider.GetRequiredService(_jobType);
            await job.RunAsync(ct);

            var finishedAt = DateTimeOffset.UtcNow;
            _logger.LogInformation(
                "Scheduled job finished: {JobType} at {FinishedAt} (duration {DurationMs} ms)",
                jobName, finishedAt, (finishedAt - startedAt).TotalMilliseconds);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Scheduled job cancelled due to shutdown: {JobType}", jobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled job failed: {JobType}", jobName);
        }
    }

    private async Task DelayUntilNextTickAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.Now;
        var nextHour = new DateTimeOffset(
            now.Year, now.Month, now.Day,
            now.Hour, 0, 0,
            now.Offset).Add(_period);

        var delay = nextHour - now;
        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, ct);
    }
}

public static class JobSchedulerExtensions
{
    public static IServiceCollection AddScheduledJob<T>(this IServiceCollection services, TimeSpan period)
        where T : IScheduledJob
    {
        return services.AddHostedService(sp => new JobScheduler(sp.GetRequiredService<IServiceScopeFactory>(), sp.GetRequiredService<ILoggerFactory>(), typeof(T), period));
    }
}
