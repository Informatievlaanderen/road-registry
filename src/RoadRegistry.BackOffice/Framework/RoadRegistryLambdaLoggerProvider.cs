using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace RoadRegistry.BackOffice.Framework
{
    /// <summary>
    /// The provider for the <see cref="RoadRegistryLambdaLogger"/>.
    /// </summary>
    [ProviderAlias("RoadRegistryLambdaLogger")]
    public class RoadRegistryLambdaLoggerProvider : ILoggerProvider
    {
        private static int _globalFactoryID;
        private readonly int _factoryID;
        private readonly ConcurrentDictionary<string, RoadRegistryLambdaLogger> _loggers;
        private readonly RoadRegistryLambdaLoggerFormatter _formatter;

        public RoadRegistryLambdaLoggerProvider()
        {
            _factoryID = Interlocked.Increment(ref _globalFactoryID);
            _loggers = new ConcurrentDictionary<string, RoadRegistryLambdaLogger>();
            _formatter = new RoadRegistryLambdaLoggerFormatter();
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.TryGetValue(categoryName, out RoadRegistryLambdaLogger logger) ?
                logger :
                _loggers.GetOrAdd(categoryName, new RoadRegistryLambdaLogger(categoryName)
                {
                    Formatter = _formatter,
                });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var logger in _loggers.Values)
            {
                logger.Level = LogLevel.None;
            }
            _loggers.Clear();
        }
    }

}
