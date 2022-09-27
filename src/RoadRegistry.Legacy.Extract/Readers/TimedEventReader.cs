namespace RoadRegistry.Legacy.Extract.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;

    public class TimedEventReader : IEventReader
    {
        public static readonly int DefaultThreshold = 1000;

        private readonly IEventReader _inner;
        private readonly ILogger<TimedEventReader> _logger;
        private readonly string _name;
        private readonly int _threshold;

        public TimedEventReader(IEventReader inner, int threshold, ILogger<TimedEventReader> logger)
        {
            if (threshold <= 0) throw new ArgumentOutOfRangeException(nameof(threshold), "The threshold needs to be greater than or equal to 1.");

            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _threshold = threshold;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _name = inner.GetType().Name.EndsWith("Reader")
                ? inner.GetType().Name.Substring(0, inner.GetType().Name.Length - 6)
                : inner.GetType().Name;
        }

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var watch = Stopwatch.StartNew();
            _logger.LogInformation("Reading of {0} started ...", _name);
            var readCount = 0;
            foreach (var @event in _inner.ReadEvents(connection))
            {
                readCount++;
                if (readCount % _threshold == 0) _logger.LogInformation("Read {0} {1} within {2}ms so far ...", readCount, _name, watch.ElapsedMilliseconds);
                yield return @event;
            }

            _logger.LogInformation("Reading {0} {1} took {2}ms.", readCount, _name, watch.ElapsedMilliseconds);
        }
    }
}
