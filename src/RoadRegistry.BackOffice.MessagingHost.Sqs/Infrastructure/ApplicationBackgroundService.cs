namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Infrastructure;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public abstract class ApplicationBackgroundService : BackgroundService
{
    protected readonly int DelaySeconds;
    protected readonly ILogger Logger;
    protected readonly IMediator Mediator;

    protected ApplicationBackgroundService(IMediator mediator, ILogger logger, int delaySeconds = 30)
    {
        Mediator = mediator;
        Logger = logger;
        DelaySeconds = delaySeconds;
    }

    protected sealed override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                await ExecuteCallbackAsync(cancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogTrace(ex, "Task has been cancelled, wait until next run...");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"An unhandled exception has occurred! {ex.Message}");
            }
            finally
            {
                await Task.Delay(DelaySeconds * 1000, cancellationToken);
            }
        }
    }

    protected abstract Task ExecuteCallbackAsync(CancellationToken cancellationToken);
}
