namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using Microsoft.Extensions.Logging;

    public class TimedEventReader : IEventReader
    {
        private readonly IEventReader _inner;
        private readonly ILogger<TimedEventReader> _logger;
        private readonly string _name;

        public TimedEventReader(IEventReader inner, ILogger<TimedEventReader> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _name = inner.GetType().Name.EndsWith("Reader")
                ? inner.GetType().Name.Substring(0, inner.GetType().Name.Length - 6)
                : inner.GetType().Name;
        }

        public IEnumerable<RecordedEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var watch = Stopwatch.StartNew();
            _logger.LogInformation("Reading of {0} started ...", _name);
            foreach (var @event in _inner.ReadEvents(connection)) yield return @event;
            _logger.LogInformation("Reading {0} took {1}ms.", _name, watch.ElapsedMilliseconds);
        }
    }
}
