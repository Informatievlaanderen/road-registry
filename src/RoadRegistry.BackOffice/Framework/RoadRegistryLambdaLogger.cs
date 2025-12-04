using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace RoadRegistry.BackOffice.Framework
{
    using Amazon.Lambda.Core;
    using Microsoft.Extensions.Logging.Abstractions;
    using LogLevel = Microsoft.Extensions.Logging.LogLevel;

    internal sealed class RoadRegistryLambdaLogger : ILogger
    {
        public RoadRegistryLambdaLoggerFormatter Formatter { get; set; }

        public RoadRegistryLambdaLogger(string categoryName)
        {
            CategoryName = categoryName;
            Level = LogLevel.Trace;
        }

        public string CategoryName { get; }

        public LogLevel Level { get; set; }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && logLevel >= Level;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var sw = new StringWriter();
            var logEntry = new LogEntry<TState>(logLevel, CategoryName, eventId, state, exception, formatter);
            Formatter.Write(in logEntry, sw);

            var message = sw.GetStringBuilder().ToString();

            LambdaLogger.Log(message);
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    }

    internal sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
