namespace RoadRegistry.BackOffice;

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Extensions;
using Exceptions;
using Microsoft.Extensions.Logging;

public class SqsQueuePublisher : ISqsQueuePublisher
{
    private readonly ILogger<SqsQueuePublisher> _logger;
    private readonly SqsOptions _sqsOptions;

    public SqsQueuePublisher(
        SqsOptions sqsOptions,
        ILogger<SqsQueuePublisher> logger)
    {
        _sqsOptions = sqsOptions ?? throw new SqsOptionsNotFoundException(nameof(sqsOptions));
        _logger = logger ?? throw new LoggerNotFoundException<SqsQueuePublisher>();
    }

    public virtual async Task<bool> CopyToQueue<T>(string queueUrl, T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            var serializer = Newtonsoft.Json.JsonSerializer.CreateDefault(_sqsOptions.JsonSerializerSettings);
            var sqsJsonMessage = SqsJsonMessage.Create(message, serializer);
            var json = serializer.Serialize(sqsJsonMessage);

            _logger.LogInformation("Placing message onto queue {QueueUrl}: {Message}", queueUrl, json);
        }

        var result = await Sqs.CopyToQueue(_sqsOptions, queueUrl, message, queueOptions, cancellationToken);

        return result;
    }
}
