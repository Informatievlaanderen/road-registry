namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public class FakeSqsQueueConsumer : ISqsQueueConsumer
{
    private readonly ILogger _logger;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;

    public FakeSqsQueueConsumer(SqsJsonMessageSerializer sqsJsonMessageSerializer, ILoggerFactory loggerFactory)
    {
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<Result<SqsJsonMessage>> Consume(string queueUrl, Func<object, Task> messageHandler, CancellationToken cancellationToken)
    {
        var sqsQueueFilePath = Path.Combine(FindSqsDirectoryPath(), queueUrl.Split('/').Last());

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(sqsQueueFilePath))
                {
                    var messagesQueue = new Queue<string>(JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync(sqsQueueFilePath, cancellationToken)));
                    while (messagesQueue.Any())
                    {
                        var serializedMessage = messagesQueue.Dequeue();

                        var sqsMessage = _sqsJsonMessageSerializer.Deserialize(serializedMessage);
                        await messageHandler(sqsMessage);

                        await File.WriteAllTextAsync(sqsQueueFilePath, JsonConvert.SerializeObject(messagesQueue.ToList(), Formatting.Indented), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while consuming queue: {QueueUrl}", queueUrl);
                throw;
            }

            Thread.Sleep(5000);
        }

        throw new OperationCanceledException();
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
