namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using BackOffice.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

/// <summary>
///     Extension methods for the <see cref="ILoggerFactory" /> class.
/// </summary>
public static class EventSourceLoggerFactoryExtensions
{
    /// <summary>
    ///     Adds an event logger named 'EventSource' to the factory.
    /// </summary>
    /// <param name="builder">The extension method argument.</param>
    /// <returns>The <see cref="ILoggingBuilder" /> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddRoadRegistryLambdaLogger(this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, RoadRegistryLambdaLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<RoadRegistryLambdaLoggerOptions, RoadRegistryLambdaLoggerProvider>(builder.Services);
        return builder;
    }
}
