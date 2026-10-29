namespace RoadRegistry.Hosts.Infrastructure;

using System;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Slack;
using Serilog.Sinks.Slack.Models;
using IBatchedLogEventSink = Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink;

/// <summary>
///     Sends every log event to Slack before returning, instead of handing it to the periodic batching
///     that <see cref="SlackSink" /> does by default. AWS Lambda freezes the execution environment as soon
///     as an invocation ends, so the batching timer never gets to flush and the log event is lost.
/// </summary>
public sealed class SynchronousSlackSink : ILogEventSink, IDisposable
{
    private readonly SlackSink _slackSink;
    private readonly TimeSpan _timeout;

    public SynchronousSlackSink(SlackSinkOptions options, ITextFormatter textFormatter, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(textFormatter);

        _slackSink = new SlackSink(options, textFormatter);
        _timeout = timeout;
    }

    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);

        try
        {
            ((IBatchedLogEventSink)_slackSink)
                .EmitBatchAsync([logEvent])
                .WaitAsync(_timeout)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception exception)
        {
            // Logging must never take down the invocation it is reporting about.
            SelfLog.WriteLine("Unable to send log event to Slack: {0}", exception);
        }
    }

    public void Dispose()
    {
        _slackSink.Dispose();
    }
}
