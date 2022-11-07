namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Messages;
using Newtonsoft.Json;

public class InitiateFeatureCompareDockerContainerTests
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly TestLambdaContext _context;
    private readonly SqsBackOfficeHandlerFunctions _functions;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public InitiateFeatureCompareDockerContainerTests()
    {
        _context = new TestLambdaContext
        {
            Logger = new TestLambdaLogger(),
            MemoryLimitInMB = 256
        };

        _functions = new SqsBackOfficeHandlerFunctions();
        _jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Fact(Skip = "TODO: Configuration pipeline")]
    public async Task When_container_instance_should_be_started_after_upload()
    {
        var serializer = JsonSerializer.Create(_jsonSerializerSettings);
        var message = new UploadRoadNetworkChangesArchive
        {
            ArchiveId = "a096806de4dc4677bb27370331f85869"
        };
        var sqsJsonMessage = SqsJsonMessage.Create(message, serializer);

        var @event = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = JsonConvert.SerializeObject(sqsJsonMessage, _jsonSerializerSettings)
                }
            }
        };

        await _functions.InitiateFeatureCompareDockerContainer(@event, _context, _cancellationTokenSource.Token);
    }

    [Fact(Skip = "TODO: Configuration pipeline")]
    public async Task When_container_instance_should_be_started_after_upload_extract()
    {
        var serializer = JsonSerializer.Create(_jsonSerializerSettings);
        var message = new UploadRoadNetworkExtractChangesArchive
        {
            ArchiveId = "a096806de4dc4677bb27370331f85869"
        };
        var sqsJsonMessage = SqsJsonMessage.Create(message, serializer);

        var @event = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = JsonConvert.SerializeObject(sqsJsonMessage, _jsonSerializerSettings)
                }
            }
        };

        await _functions.InitiateFeatureCompareDockerContainer(@event, _context, _cancellationTokenSource.Token);
    }
}
