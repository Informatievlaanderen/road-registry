namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

public class InwinningUploadDurationWarningTests
{
    private static readonly DownloadId DownloadId = new(Guid.Parse("8b6a5f4e-0c1d-4c4f-9d43-0a1b2c3d4e5f"));
    private static readonly UploadId UploadId = new(Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"));

    [Fact]
    public void WhenStillRunning_ThenAWarningIsLoggedAsErrorEveryMinuteFrom20Minutes()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        using var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", DownloadId, UploadId, "44021", timeProvider);

        AdvanceInSteps(timeProvider, TimeSpan.FromMinutes(19) + TimeSpan.FromSeconds(59));
        logger.Entries.Should().BeEmpty();

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        AdvanceInSteps(timeProvider, TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(59));

        logger.Entries.Should().HaveCount(3);
        logger.Entries.Should().OnlyContain(x => x.Level == LogLevel.Error);
        logger.Entries.Select(x => x.Message).Should().Equal(
            "Warning: UploadInwinningExtract is na 20 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021",
            "Warning: UploadInwinningExtract is na 21 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021",
            "Warning: UploadInwinningExtract is na 22 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021");
    }

    [Fact]
    public void WhenFinishedInTime_ThenNothingIsLogged()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", DownloadId, UploadId, "44021", timeProvider);
        AdvanceInSteps(timeProvider, TimeSpan.FromMinutes(19));
        warning.Dispose();

        AdvanceInSteps(timeProvider, TimeSpan.FromMinutes(15));

        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public void WhenFinishedAfter20Minutes_ThenTheRepeatedWarningStops()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        var warning = new InwinningUploadDurationWarning(logger, "MigrateDryRunRoadNetwork", DownloadId, UploadId, timeProvider: timeProvider);
        AdvanceInSteps(timeProvider, TimeSpan.FromMinutes(21.5));
        warning.Dispose();

        AdvanceInSteps(timeProvider, TimeSpan.FromMinutes(10));

        logger.Entries.Select(x => x.Message).Should().HaveCount(2)
            .And.Subject.Last().Should().Contain("na 21 minuten");
    }

    [Fact]
    public void WhenThereIsNoNisCode_ThenTheDownloadIdIdentifiesTheExtract()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        using var warning = new InwinningUploadDurationWarning(logger, "MigrateDryRunRoadNetwork", DownloadId, UploadId, timeProvider: timeProvider);
        timeProvider.Advance(TimeSpan.FromMinutes(20));

        logger.Entries.Should().ContainSingle().Which.Message.Should().Be(
            "Warning: MigrateDryRunRoadNetwork is na 20 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b");
    }

    // The manual clock fires a periodic timer once per advance, so time is moved on in steps shorter than its period.
    private static void AdvanceInSteps(ManualTimeProvider timeProvider, TimeSpan by)
    {
        var step = TimeSpan.FromSeconds(30);
        for (var advanced = TimeSpan.Zero; advanced < by; advanced += step)
        {
            timeProvider.Advance(by - advanced < step ? by - advanced : step);
        }
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }

    // A timer fires once its due time has passed and the clock is advanced, and then again every period if it has one.
    // Advancing by more than a period fires a periodic timer only once.
    private sealed class ManualTimeProvider : TimeProvider
    {
        private readonly List<ManualTimer> _timers = [];
        private DateTimeOffset _now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new ManualTimer(callback, state, _now + dueTime, period);
            _timers.Add(timer);
            return timer;
        }

        public void Advance(TimeSpan by)
        {
            _now += by;
            foreach (var timer in _timers.Where(x => !x.Disposed && x.DueAt is not null && x.DueAt <= _now).OrderBy(x => x.DueAt).ToList())
            {
                timer.Fire();
            }
        }

        private sealed class ManualTimer(TimerCallback callback, object? state, DateTimeOffset dueAt, TimeSpan period) : ITimer
        {
            public DateTimeOffset? DueAt { get; private set; } = dueAt;
            public bool Disposed { get; private set; }

            public void Fire()
            {
                DueAt = period == Timeout.InfiniteTimeSpan ? null : DueAt + period;
                callback(state);
            }

            public bool Change(TimeSpan dueTime, TimeSpan period) => throw new NotSupportedException();
            public void Dispose() => Disposed = true;
            public ValueTask DisposeAsync()
            {
                Disposed = true;
                return ValueTask.CompletedTask;
            }
        }
    }
}
