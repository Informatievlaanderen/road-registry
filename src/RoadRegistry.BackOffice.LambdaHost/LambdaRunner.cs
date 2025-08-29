namespace RoadRegistry.BackOffice.LambdaHost;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Configuration;
using Handlers.Sqs.Lambda;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Uploads;

public class LambdaRunner
{
    private readonly ISqsQueueConsumer _sqsQueueConsumer;
    private readonly SqsQueueUrlOptions _sqsQueueUrlOptions;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
    private readonly SqsOptions _sqsOptions;
    private readonly ILogger _logger;

    public LambdaRunner(ISqsQueueConsumer sqsQueueConsumer, SqsQueueUrlOptions sqsQueueUrlOptions, SqsJsonMessageSerializer sqsJsonMessageSerializer, SqsOptions sqsOptions, ILoggerFactory loggerFactory)
    {
        _sqsQueueConsumer = sqsQueueConsumer;
        _sqsQueueUrlOptions = sqsQueueUrlOptions;
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
        _sqsOptions = sqsOptions;
        _logger = loggerFactory.CreateLogger<LambdaRunner>();
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _sqsQueueConsumer.Consume(_sqsQueueUrlOptions.BackOffice, message =>
        {
            var sqsEvent = new SQSEvent
            {
                Records =
                [
                    new()
                    {
                        Body = _sqsJsonMessageSerializer.Serialize(message),
                        Attributes = new Dictionary<string, string>
                        {
                            {"MessageGroupId", string.Empty}
                        }
                    }
                ]
            };
            var sqsEventJson = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(sqsEvent, _sqsOptions.JsonSerializerSettings));

            var lambdaContext = new TestLambdaContext
            {
                Logger = new LambdaLogger(_logger)
            };
            var lambdaFunction = new Function();
            lambdaFunction.Handler(sqsEventJson, lambdaContext).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }, stoppingToken);
    }

    private sealed class LambdaLogger : ILambdaLogger
    {
        private readonly ILogger _logger;

        public LambdaLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogLine(string message)
        {
            _logger.LogInformation(message);
        }
    }
}
