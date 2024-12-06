namespace RoadRegistry.Hosts.Infrastructure.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;
    using Serilog.Sinks.Slack.Models;
    using Serilog.Sinks.Slack;

    public static class LoggerConfigurationExtensions
    {
        public static readonly TimeSpan SlackSinkPeriod = TimeSpan.FromSeconds(2);

        public static LoggerConfiguration AddSlackSink<T>(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            var slackSinkConfiguation = configuration.GetSection(nameof(SlackSinkOptions));

            if (slackSinkConfiguation.Exists())
            {
                var sinkOptions = new SlackSinkOptions
                {
                    CustomUserName = typeof(T).Namespace,
                    MinimumLogEventLevel = LogEventLevel.Error,
                    Period = SlackSinkPeriod
                };
                slackSinkConfiguation.Bind(sinkOptions);
                if (!string.IsNullOrEmpty(sinkOptions.WebHookUrl))
                {
                    loggerConfiguration.WriteTo.Slack(sinkOptions);
                }
            }

            return loggerConfiguration;
        }
    }
}
