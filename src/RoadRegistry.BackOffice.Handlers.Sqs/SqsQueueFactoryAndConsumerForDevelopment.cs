namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Uploads;

public class SqsQueueFactoryAndConsumerForDevelopment : ISqsQueueFactory, ISqsQueueConsumer
{
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly string _sqsDirectoryPath;

    public SqsQueueFactoryAndConsumerForDevelopment(SqsJsonMessageSerializer sqsJsonMessageSerializer, ILoggerFactory loggerFactory)
    {
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer.ThrowIfNull();
        _loggerFactory = loggerFactory.ThrowIfNull();
        _logger = loggerFactory.CreateLogger(GetType());
        _sqsDirectoryPath = FindSqsDirectoryPath();
    }

    public ISqsQueue Create(string queueUrl)
    {
        return new SqsQueueForDevelopment(_sqsDirectoryPath, queueUrl, _sqsJsonMessageSerializer, _loggerFactory.CreateLogger<SqsQueueForDevelopment>());
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

    public async Task<Result<SqsJsonMessage>> Consume(string queueUrl, Func<object, Task> messageHandler, CancellationToken cancellationToken)
    {
        var queueDirectory = FindSqsDirectoryPath();
        var queueFileName = queueUrl.Split('/').Last();
        var queueWatcher = new FileSystemWatcher(queueDirectory, queueFileName);
        var sqsQueueFilePath = Path.Combine(queueDirectory, queueFileName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(sqsQueueFilePath))
                {
                    var messages = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync(sqsQueueFilePath, cancellationToken));
                    while (messages.Any())
                    {
                        var serializedMessage = messages.First();

                        var sqsMessage = _sqsJsonMessageSerializer.Deserialize(serializedMessage);
                        await messageHandler(sqsMessage);

                        messages = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync(sqsQueueFilePath, cancellationToken));
                        messages.RemoveAt(0);
                        await File.WriteAllTextAsync(sqsQueueFilePath, JsonConvert.SerializeObject(messages.ToList(), Formatting.Indented), cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while consuming queue: {QueueUrl}", queueUrl);
                throw;
            }

            queueWatcher.WaitForChanged(WatcherChangeTypes.Changed | WatcherChangeTypes.Created);
            Thread.Sleep(500);
        }

        throw new OperationCanceledException();
    }

    private sealed class SqsQueueForDevelopment : ISqsQueue
    {
        private readonly string _sqsDirectoryPath;
        private readonly string _queueUrl;
        private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
        private readonly ILogger<SqsQueueForDevelopment> _logger;

        public SqsQueueForDevelopment(string sqsDirectoryPath, string queueUrl, SqsJsonMessageSerializer sqsJsonMessageSerializer, ILogger<SqsQueueForDevelopment> logger)
        {
            _sqsDirectoryPath = sqsDirectoryPath.ThrowIfNull();
            _queueUrl = queueUrl.ThrowIfNull();
            _sqsJsonMessageSerializer = sqsJsonMessageSerializer.ThrowIfNull();
            _logger = logger.ThrowIfNull();
        }

        public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
        {
            var sqsQueueFilePath = Path.Combine(_sqsDirectoryPath, _queueUrl.Split('/').Last());
            var serializedMessages = File.Exists(sqsQueueFilePath)
                ? JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(sqsQueueFilePath))
                : [];

            serializedMessages.Add(_sqsJsonMessageSerializer.Serialize(message));

            File.WriteAllText(sqsQueueFilePath, JsonConvert.SerializeObject(serializedMessages, Formatting.Indented));

            _logger.LogInformation("Enqueuing item on '{QueueUrl}':\n{Message}", _queueUrl, _sqsJsonMessageSerializer.Serialize(message));
            return Task.FromResult(true);
        }
    }
}
