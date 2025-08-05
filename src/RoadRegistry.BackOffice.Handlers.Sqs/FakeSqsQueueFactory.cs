namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Uploads;

public class FakeSqsQueueFactory : ISqsQueueFactory
{
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
    private readonly ILoggerFactory _loggerFactory;

    public FakeSqsQueueFactory(SqsJsonMessageSerializer sqsJsonMessageSerializer, ILoggerFactory loggerFactory)
    {
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer.ThrowIfNull();
        _loggerFactory = loggerFactory.ThrowIfNull();
    }

    public ISqsQueue Create(string queueUrl)
    {
        return new FakeSqsQueue(queueUrl, _sqsJsonMessageSerializer, _loggerFactory.CreateLogger<FakeSqsQueue>());
    }

    private sealed class FakeSqsQueue : ISqsQueue
    {
        private readonly string _queueUrl;
        private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
        private readonly ILogger<FakeSqsQueue> _logger;

        public FakeSqsQueue(string queueUrl, SqsJsonMessageSerializer sqsJsonMessageSerializer, ILogger<FakeSqsQueue> logger)
        {
            _queueUrl = queueUrl.ThrowIfNull();
            _sqsJsonMessageSerializer = sqsJsonMessageSerializer.ThrowIfNull();
            _logger = logger.ThrowIfNull();
        }

        public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
        {
            var sqsQueueFilePath = Path.Combine(FindSqsDirectoryPath(), _queueUrl.Split('/').Last());
            var serializedMessages = File.Exists(sqsQueueFilePath)
                ? JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(sqsQueueFilePath))
                : [];

            serializedMessages.Add(_sqsJsonMessageSerializer.Serialize(message));

            File.WriteAllText(sqsQueueFilePath, JsonConvert.SerializeObject(serializedMessages, Formatting.Indented));

            _logger.LogInformation("Enqueuing item on '{QueueUrl}':\n{Message}", _queueUrl, _sqsJsonMessageSerializer.Serialize(message));
            return Task.FromResult(true);
        }

        private static string FindSqsDirectoryPath()
        {
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            while (currentDirectory.Parent is not null)
            {
                var sqsDirectory = Path.Combine(currentDirectory.FullName, ".sqs");
                if (Directory.Exists(sqsDirectory))
                {
                    return sqsDirectory;
                }

                currentDirectory = currentDirectory.Parent;
            }

            throw new DirectoryNotFoundException("Could not find sqs directory");
        }
    }
}
