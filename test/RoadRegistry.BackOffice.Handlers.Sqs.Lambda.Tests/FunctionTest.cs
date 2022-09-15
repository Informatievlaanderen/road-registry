using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;


namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Amazon.Lambda.SQSEvents;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using Newtonsoft.Json;

public class FunctionTest
{
    private readonly TestLambdaContext _context;
    private readonly Functions _functions;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public FunctionTest()
    {
        _context = new TestLambdaContext
        {
            Logger = new TestLambdaLogger(),
            MemoryLimitInMB = 256,
        };
        _functions = new();
        _jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        _cancellationTokenSource = new();
    }

    [Fact]
    public async Task When_checking_message_dequeue_availability()
    {
        await _functions.CheckSQSMessageAvailableForProcessing(_context, _cancellationTokenSource.Token);
    }

    [Fact]
    public async Task When_container_instance_should_be_started_after_upload()
    {
        var message = new UploadRoadNetworkChangesArchive
        {
            ArchiveId = "a096806de4dc4677bb27370331f85869"
        };
        var command = new SimpleQueueCommand(message);

        var @event = new SQSEvent()
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = JsonConvert.SerializeObject(command, _jsonSerializerSettings)
                }
            }
        };

        await _functions.InitiateDockerFeatureCompare(@event, _context, _cancellationTokenSource.Token);
    }

    [Fact]
    public async Task When_container_instance_should_be_started_after_upload_extract()
    {
        var message = new UploadRoadNetworkExtractChangesArchive
        {
            ArchiveId = "a096806de4dc4677bb27370331f85869"
        };
        var command = new SimpleQueueCommand(message);

        var @event = new SQSEvent()
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = JsonConvert.SerializeObject(command, _jsonSerializerSettings)
                }
            }
        };

        await _functions.InitiateDockerFeatureCompare(@event, _context, _cancellationTokenSource.Token);
    }
}
