using System;

namespace RoadRegistry.Hosts.Infrastructure.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;
    using Serilog.Sinks.Slack;
    using Serilog.Sinks.Slack.Models;

    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration AddSlackSink(this LoggerConfiguration loggerConfiguration, Type type, IConfiguration configuration)
        {
            var slackSinkConfiguation = configuration.GetSection(nameof(SlackSinkOptions));

            if (slackSinkConfiguation.Exists())
            {
                var sinkOptions = new SlackSinkOptions
                {
                    CustomUserName = type.Namespace,
                    MinimumLogEventLevel = LogEventLevel.Error
                };
                slackSinkConfiguation.Bind(sinkOptions);
                if (sinkOptions.WebHookUrl is not null) loggerConfiguration.WriteTo.Slack(sinkOptions);
            }

            return loggerConfiguration;
        }

        public static LoggerConfiguration AddSlackSink<T>(this LoggerConfiguration loggerConfiguration, IConfiguration configuration) => AddSlackSink(loggerConfiguration, typeof(T), configuration);
    }
}
