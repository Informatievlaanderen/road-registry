namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

/// <summary>
/// Records every log entry so tests can assert on what was logged.
/// </summary>
public sealed class CapturingLogger<T> : ILogger<T>
{
    public record LogEntry(LogLevel Level, Exception? Exception, string Message);

    private readonly List<LogEntry> _entries = new();
    public IReadOnlyList<LogEntry> Entries => _entries;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
    }
}
