namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

public class InwinningUploadDurationWarningTests
{
    private static readonly InwinningUploadDescription Upload = new(
        new DownloadId(Guid.Parse("8b6a5f4e-0c1d-4c4f-9d43-0a1b2c3d4e5f")),
        new UploadId(Guid.Parse("1f2e3d4c-5b6a-4978-8a9b-0c1d2e3f4a5b")),
        "44021",
        "Inwinning Gent");

    [Fact]
    public void WhenStillRunning_ThenAWarningIsLoggedAt8And9And10Minutes()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        using var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", Upload, timeProvider);

        timeProvider.Advance(TimeSpan.FromMinutes(7) + TimeSpan.FromSeconds(59));
        logger.Entries.Should().BeEmpty();

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        timeProvider.Advance(TimeSpan.FromMinutes(5));

        logger.Entries.Should().HaveCount(3);
        logger.Entries.Should().OnlyContain(x => x.Level == LogLevel.Warning);
        logger.Entries.Select(x => x.Message).Should().Equal(
            "UploadInwinningExtract is na 8 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021, Beschrijving Inwinning Gent",
            "UploadInwinningExtract is na 9 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021, Beschrijving Inwinning Gent",
            "UploadInwinningExtract is na 10 minuten nog bezig: DownloadId 8b6a5f4e0c1d4c4f9d430a1b2c3d4e5f, UploadId 1f2e3d4c5b6a49788a9b0c1d2e3f4a5b, NisCode 44021, Beschrijving Inwinning Gent");
    }

    [Fact]
    public void WhenFinishedInTime_ThenNothingIsLogged()
    {
        var timeProvider = new ManualTimeProvider();
        var logger = new CapturingLogger();

        var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", Upload, timeProvider);
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

        var warning = new InwinningUploadDurationWarning(logger, "UploadInwinningExtract", Upload, timeProvider);
        timeProvider.Advance(TimeSpan.FromMinutes(8.5));
        warning.Dispose();

        timeProvider.Advance(TimeSpan.FromMinutes(10));

        logger.Entries.Should().ContainSingle().Which.Message.Should().Contain("na 8 minuten");
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

    // Only one-shot timers are needed: each one fires once its due time has passed and the clock is advanced.
    private sealed class ManualTimeProvider : TimeProvider
    {
        private readonly List<ManualTimer> _timers = [];
        private DateTimeOffset _now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new ManualTimer(callback, state, _now + dueTime);
            _timers.Add(timer);
            return timer;
        }

        public void Advance(TimeSpan by)
        {
            _now += by;
            foreach (var timer in _timers.Where(x => !x.Fired && !x.Disposed && x.DueAt <= _now).OrderBy(x => x.DueAt).ToList())
            {
                timer.Fire();
            }
        }

        private sealed class ManualTimer(TimerCallback callback, object? state, DateTimeOffset dueAt) : ITimer
        {
            public DateTimeOffset DueAt { get; } = dueAt;
            public bool Fired { get; private set; }
            public bool Disposed { get; private set; }

            public void Fire()
            {
                Fired = true;
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
