namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using Newtonsoft.Json;
using Xunit;

public class InitiateFeatureCompareDockerContainerTests
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly TestLambdaContext _context;
    private readonly FeatureCompareFunctions _functions;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public InitiateFeatureCompareDockerContainerTests()
    {
        _context = new TestLambdaContext
        {
            Logger = new TestLambdaLogger(),
            MemoryLimitInMB = 256
        };

        _functions = new FeatureCompareFunctions();
        _jsonSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Fact]
    public async Task When_container_instance_should_be_started_after_upload()
    {
        var message = new UploadRoadNetworkChangesArchive
        {
            ArchiveId = "a096806de4dc4677bb27370331f85869"
        };
        var command = new SimpleQueueCommand(message);

        var @event = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = JsonConvert.SerializeObject(command, _jsonSerializerSettings)
                }
            }
        };

        Environment.SetEnvironmentVariable("MINIO_SERVER", "http://localhost:9010");
        Environment.SetEnvironmentVariable("MINIO_ROOT_USER", "Q3AM3UQ867SPQQA43P2F");
        Environment.SetEnvironmentVariable("MINIO_ROOT_PASSWORD", "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG");

        await _functions.InitiateFeatureCompareDockerContainer(@event, _context, _cancellationTokenSource.Token);
    }

    [Fact]
    public async Task When_container_instance_should_be_started_after_upload_extract()
    {
        var message = new UploadRoadNetworkExtractChangesArchive
        {
            ArchiveId = "a096806de4dc4677bb27370331f85869"
        };
        var command = new SimpleQueueCommand(message);

        var @event = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new()
                {
                    Body = JsonConvert.SerializeObject(command, _jsonSerializerSettings)
                }
            }
        };

        await _functions.InitiateFeatureCompareDockerContainer(@event, _context, _cancellationTokenSource.Token);
    }
}
