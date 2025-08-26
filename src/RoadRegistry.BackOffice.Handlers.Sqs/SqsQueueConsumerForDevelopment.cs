namespace RoadRegistry.BackOffice.Handlers.Sqs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Uploads;

public class SqsQueueConsumerForDevelopment : ISqsQueueConsumer
{
    private readonly ILogger _logger;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;

    public SqsQueueConsumerForDevelopment(SqsJsonMessageSerializer sqsJsonMessageSerializer, ILoggerFactory loggerFactory)
    {
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
        _logger = loggerFactory.CreateLogger(GetType());
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
