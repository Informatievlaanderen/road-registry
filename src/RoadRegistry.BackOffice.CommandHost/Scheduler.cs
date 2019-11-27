namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class Scheduler : IHostedService
    {
        private static readonly TimeSpan DefaultFrequency = TimeSpan.FromSeconds(1);

        private readonly IClock _clock;
        private readonly ILogger<Scheduler> _logger;

        private readonly TimeSpan _frequency;

        private CancellationTokenSource _messagePumpCancellation;

        private BufferBlock<object> _messagePumpInbox;

        private Timer _timer;

        private Task _messagePump;

        public Scheduler(IClock clock, ILogger<Scheduler> logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _frequency = DefaultFrequency;
        }

        public void Schedule(Action action, TimeSpan due)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _messagePumpInbox?.Post(
                new ScheduleAction
                {
                    Action = action,
                    Due = _clock.GetCurrentInstant().Plus(Duration.FromTimeSpan(due))
                });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _messagePumpCancellation = new CancellationTokenSource();
            _messagePumpInbox = new BufferBlock<object>(new DataflowBlockOptions
            {
                BoundedCapacity = int.MaxValue,
                EnsureOrdered = false,
                MaxMessagesPerTask = 1,
                CancellationToken = _messagePumpCancellation.Token
            });
            _timer = new Timer(
                _ => _messagePumpInbox.Post(new TimerElapsed { Time = _clock.GetCurrentInstant() }),
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);
            _messagePump = Task.Run(async () =>
            {
                var scheduled = new List<ScheduledAction>();
                while (!_messagePumpCancellation.IsCancellationRequested)
                {
                    try
                    {
                        switch (await _messagePumpInbox.ReceiveAsync(_messagePumpCancellation.Token))
                        {
                            case TimerElapsed elapsed:
                                if (_logger.IsEnabled(LogLevel.Debug))
                                {
                                    _logger.Log(LogLevel.Debug, "Timer elapsed at instant {0}.", elapsed.Time);
                                }

                                var dueEntries = scheduled
                                    .Where(entry => entry.Due <= elapsed.Time)
                                    .ToArray();
                                if (_logger.IsEnabled(LogLevel.Debug))
                                {
                                    _logger.Log(LogLevel.Debug, "{0} actions due.", dueEntries.Length);
                                }

                                foreach (var dueEntry in dueEntries)
                                {
                                    dueEntry.Action();
                                    scheduled.Remove(dueEntry);
                                }

                                if (scheduled.Count == 0) // deactivate timer when no more work
                                {
                                    if (_logger.IsEnabled(LogLevel.Debug))
                                    {
                                        _logger.Log(LogLevel.Debug,
                                            "Timer deactivated because no more scheduled actions.");
                                    }

                                    _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                                }

                                break;
                            case ScheduleAction schedule:
                                if (scheduled.Count == 0) // activate timer when more work
                                {
                                    if (_logger.IsEnabled(LogLevel.Debug))
                                    {
                                        _logger.Log(LogLevel.Debug, "Timer activate because new scheduled actions.");
                                    }

                                    _timer.Change(_frequency, _frequency);
                                }

                                if (_logger.IsEnabled(LogLevel.Debug))
                                {
                                    _logger.Log(LogLevel.Debug, "Scheduling an action to be executed at {0}.",
                                        schedule.Due);
                                }

                                scheduled.Add(new ScheduledAction(schedule.Action, schedule.Due));
                                break;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.Log(LogLevel.Information, "Scheduler message pump is exiting due to cancellation.");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.Log(LogLevel.Information, "Scheduler message pump is exiting due to cancellation.");
                        }
                    }
                    catch (Exception exception)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.Log(LogLevel.Error, exception, "Scheduler message pump is exiting due to a bug.");
                        }
                    }
                }
            }, _messagePumpCancellation.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _messagePumpCancellation.Cancel();
            _messagePumpInbox.Complete();
            await _messagePumpInbox.Completion;
            await _messagePump;
            _messagePumpCancellation.Dispose();
            _timer.Dispose();
        }

        private class ScheduledAction
        {
            public Action Action { get; }

            public Instant Due { get; }

            public ScheduledAction(Action action, Instant due)
            {
                Action = action;
                Due = due;
            }
        }

        private class ScheduleAction
        {
            public Action Action { get; set; }

            public Instant Due { get; set; }
        }

        private class TimerElapsed
        {
            public Instant Time { get; set; }
        }
    }
}
