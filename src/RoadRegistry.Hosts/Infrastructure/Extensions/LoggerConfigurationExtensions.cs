namespace RoadRegistry.Hosts.Infrastructure.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;
    using Serilog.Sinks.Slack.Models;
    using Serilog.Sinks.Slack;

    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration AddSlackSink<T>(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            var slackSinkConfiguation = configuration.GetSection(nameof(SlackSinkOptions));

            if (slackSinkConfiguation.Exists())
            {
                var sinkOptions = new SlackSinkOptions
                {
                    CustomUserName = typeof(T).Namespace,
                    MinimumLogEventLevel = LogEventLevel.Error
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
