namespace RoadRegistry.Hosts.Infrastructure.Extensions
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Display;
    using Serilog.Sinks.Slack.Models;
    using Serilog.Sinks.Slack;

    public static class LoggerConfigurationExtensions
    {
        public static readonly TimeSpan SlackSinkPeriod = TimeSpan.FromSeconds(2);
        public static readonly TimeSpan SlackSinkSendTimeout = TimeSpan.FromSeconds(5);

        private const string SlackSinkOutputTemplate = "{Message}";

        /// <param name="synchronous">
        ///     Sends each log event to Slack before returning, for hosts whose process can be suspended or
        ///     torn down before the batching timer gets to flush, such as an AWS Lambda.
        /// </param>
        public static LoggerConfiguration AddSlackSink<T>(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, bool synchronous = false)
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
                    if (synchronous)
                    {
                        loggerConfiguration.WriteTo.Sink(
                            new SynchronousSlackSink(
                                sinkOptions,
                                new MessageTemplateTextFormatter(SlackSinkOutputTemplate),
                                SlackSinkSendTimeout),
                            sinkOptions.MinimumLogEventLevel);
                    }
                    else
                    {
                        loggerConfiguration.WriteTo.Slack(sinkOptions);
                    }
                }
            }

            return loggerConfiguration;
        }
    }
}
