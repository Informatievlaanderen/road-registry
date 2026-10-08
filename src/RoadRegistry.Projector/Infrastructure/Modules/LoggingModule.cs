namespace RoadRegistry.Projector.Infrastructure.Modules;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;

public static class LoggingModule
{
    // Only the providers are replaced here, never the logger: UseDefaultForApi has already built Log.Logger from the
    // Serilog section, with the enrichers and - through the ConfigureSerilog hook in Program - the Slack sink and the
    // common-error filters. Building a second logger from the configuration alone silently dropped both, which is how
    // every Marten daemon error ended up console-only while the other hosts kept reporting on Slack.
    //
    // The providers do have to be cleared: Host.CreateDefaultBuilder adds the console, debug and event-source ones,
    // and leaving those in place duplicates every line next to the Serilog output.
    public static IServiceCollection RegisterLoggingModule(this IServiceCollection services)
    {
        SelfLog.Enable(Console.WriteLine);

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(Log.Logger);
        });

        return services;
    }
}
