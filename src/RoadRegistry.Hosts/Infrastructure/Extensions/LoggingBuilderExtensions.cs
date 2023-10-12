namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using BackOffice.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Debugging;

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

    public static ILoggingBuilder AddSerilog<T>(this ILoggingBuilder builder, IConfiguration configuration)
    {
        SelfLog.Enable(Console.WriteLine);

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentUserName().Filter
            .ByExcluding(logEvent => logEvent.Exception is OperationCanceledException)
            .AddSlackSink<T>(configuration);

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.AddSerilog(Log.Logger);

        return builder;
    }
}
