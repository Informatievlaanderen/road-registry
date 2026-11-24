namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

public class InwinningUploadDurationWarningTests
{
    private static readonly DownloadId DownloadId = new(Guid.Parse("8b6a5f4e-0c1d-4c4f-9d43-0a1b2c3d4e5f"));
    private static readonly UploadId UploadId = new(Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b"));

    [Fact]
    public void WhenStillRunning_ThenAWarningIsLoggedAsErrorAt8And9MinutesAndEveryMinuteFrom10Minutes()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        using var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", DownloadId, UploadId, "44021", timeProvider);

        timeProvider.Advance(TimeSpan.FromMinutes(7) + TimeSpan.FromSeconds(59));
        logger.Entries.Should().BeEmpty();

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        timeProvider.Advance(TimeSpan.FromSeconds(59));

        logger.Entries.Should().HaveCount(6);
        logger.Entries.Should().OnlyContain(x => x.Level == LogLevel.Error);
        logger.Entries.Select(x => x.Message).Should().Equal(
            "Warning: UploadInwinningExtract is na 8 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021",
            "Warning: UploadInwinningExtract is na 9 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021",
            "Warning: UploadInwinningExtract is na 10 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021",
            "Warning: UploadInwinningExtract is na 11 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021",
            "Warning: UploadInwinningExtract is na 12 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021",
            "Warning: UploadInwinningExtract is na 13 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021");
    }

    [Fact]
    public void WhenFinishedInTime_ThenNothingIsLogged()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", DownloadId, UploadId, "44021", timeProvider);
        timeProvider.Advance(TimeSpan.FromMinutes(7));
        warning.Dispose();

        timeProvider.Advance(TimeSpan.FromMinutes(10));

        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public void WhenFinishedBetweenTwoWarnings_ThenOnlyTheEarlierOnesAreLogged()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", DownloadId, UploadId, "44021", timeProvider);
        timeProvider.Advance(TimeSpan.FromMinutes(8.5));
        warning.Dispose();

        timeProvider.Advance(TimeSpan.FromMinutes(10));

        logger.Entries.Should().ContainSingle().Which.Message.Should().Contain("na 8 minuten");
    }

    [Fact]
    public void WhenFinishedAfter10Minutes_ThenTheRepeatedWarningStops()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        var warning = new InwinningUploadDurationWarning(logger, "MigrateDryRunRoadNetwork", DownloadId, UploadId, timeProvider: timeProvider);
        foreach (var _ in Enumerable.Range(0, 23))
        {
            timeProvider.Advance(TimeSpan.FromSeconds(30));
        }
        warning.Dispose();

        foreach (var _ in Enumerable.Range(0, 10))
        {
            timeProvider.Advance(TimeSpan.FromMinutes(1));
        }

        logger.Entries.Select(x => x.Message).Should().HaveCount(4)
            .And.Subject.Last().Should().Contain("na 11 minuten");
    }

    [Fact]
    public void WhenThereIsNoNisCode_ThenTheDownloadIdIdentifiesTheExtract()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        using var warning = new InwinningUploadDurationWarning(logger, "MigrateDryRunRoadNetwork", DownloadId, UploadId, timeProvider: timeProvider);
        timeProvider.Advance(TimeSpan.FromMinutes(8));

        logger.Entries.Should().ContainSingle().Which.Message.Should().Be(
            "Warning: MigrateDryRunRoadNetwork is na 8 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b");
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
