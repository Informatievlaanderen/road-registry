namespace RoadRegistry.Extensions;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

public static class LoggingExtensions
{
    public static IDisposable TimeAction(this ILogger logger, [CallerMemberName] string action = "")
    {
        return new TimedAction(logger, action);
    }

    private sealed class TimedAction : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _action;
        private readonly LogLevel _loglevel;
        private readonly Stopwatch _stopwatch;
        private readonly bool _enabled;

        public TimedAction(ILogger logger, string action, LogLevel loglevel = LogLevel.Warning)
        {
            _enabled = logger.IsEnabled(loglevel);
            if (!_enabled)
            {
                return;
            }

            _logger = logger;
            _action = action;
            _loglevel = loglevel;

            _logger.Log(loglevel, "Started {Action}", action);
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_enabled)
            {
                return;
            }

            _logger.Log(_loglevel, "Finished {Action}: took {Elapsed}", _action, _stopwatch.Elapsed);
        }
    }
}
