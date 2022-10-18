namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Amazon.Lambda.TestUtilities;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;

public class CheckFeatureCompareDockerContainerTests
{
    public CheckFeatureCompareDockerContainerTests()
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

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly TestLambdaContext _context;
    private readonly SqsBackOfficeHandlerFunctions _functions;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    [Fact(Skip = "TODO: Configuration pipeline")]
    public async Task When_checking_message_dequeue_availability()
    {
        await _functions.CheckFeatureCompareDockerContainer(_context, _cancellationTokenSource.Token);
    }
}
