namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.Framework;

using Autofac;
using BackOffice;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NodaTime;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore;
using TicketingService.Abstractions;

public abstract class SqsLambdaHandlerFixture<TSqsLambdaRequestHandler, TSqsLambdaRequest, TSqsRequest> : ApplicationFixture, IAsyncLifetime
    where TSqsLambdaRequestHandler : SqsLambdaHandler<TSqsLambdaRequest>
    where TSqsLambdaRequest : SqsLambdaRequest
    where TSqsRequest : SqsRequest
{
    private const string ConfigurationDetailUrl = "http://base/{0}";

    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private static readonly StreamNameConverter StreamNameConverter = StreamNameConversions.PassThru;
    protected readonly IConfiguration Configuration;
    protected readonly ICustomRetryPolicy CustomRetryPolicy;
    protected readonly IIdempotentCommandHandler IdempotentCommandHandler;
    protected readonly ILifetimeScope LifetimeScope;
    protected readonly ILoggerFactory LoggerFactory;
    protected readonly SqsLambdaHandlerOptions Options;
    protected readonly IRoadNetworkCommandQueue RoadNetworkCommandQueue;
    protected readonly IRoadRegistryContext RoadRegistryContext;
    protected readonly IStreamStore Store;
    protected readonly RoadNetworkTestData TestData = new();

    protected SqsLambdaHandlerFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IStreamStore streamStore,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IClock clock,
        SqsLambdaHandlerOptions options
    )
    {
        TestData.CopyCustomizationsTo(ObjectProvider);

        Configuration = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "DetailUrl", ConfigurationDetailUrl }
            })
            .Build();

        CustomRetryPolicy = customRetryPolicy;
        Store = streamStore;

        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register(_ => new EventSourcedEntityMap())
            .AsSelf()
            .SingleInstance();
        var container = containerBuilder.Build();
        LifetimeScope = container.BeginLifetimeScope();

        RoadRegistryContext = new RoadRegistryContext(
            LifetimeScope.Resolve<EventSourcedEntityMap>(),
            Store, new FakeRoadNetworkSnapshotReader(),
            Settings,
            Mapping,
            EnrichEvent.WithTime(clock),
            new NullLoggerFactory());
        RoadNetworkCommandQueue = roadNetworkCommandQueue;
        Clock = clock;
        Options = options;

        LoggerFactory = new LoggerFactory();

        TicketingMock = MockTicketing();

        var commandHandlerDispatcher = BuildCommandHandlerDispatcher();
        if (commandHandlerDispatcher is not null)
        {
            IdempotentCommandHandler = new RoadRegistryIdempotentCommandHandler(commandHandlerDispatcher);
        }

        Exception = null;
    }

    public IClock Clock { get; }
    protected abstract TSqsLambdaRequestHandler SqsLambdaRequestHandler { get; }
    protected abstract TSqsLambdaRequest SqsLambdaRequest { get; }
    protected abstract TSqsRequest SqsRequest { get; }
    protected Mock<ITicketing> TicketingMock { get; }
    public bool Result { get; private set; }
    public Exception? Exception { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            await SetupAsync();

            await SqsLambdaRequestHandler.Handle(SqsLambdaRequest, CancellationToken.None);

            Result = await VerifyTicketAsync();
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual CommandHandlerDispatcher BuildCommandHandlerDispatcher()
    {
        return null;
    }

    protected Task Given(StreamName streamName, params object[] events)
    {
        return Store.Given(Mapping, Settings, StreamNameConverter, streamName, events);
    }

    protected virtual Mock<ITicketing> MockTicketing()
    {
        return new Mock<ITicketing>();
    }

    protected abstract Task SetupAsync();

    protected bool VerifyThatTicketHasCompleted(string location, string eTag)
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        TicketingMock.Verify(x =>
            x.Complete(
                It.IsAny<Guid>(),
                new TicketResult(
                    new ETagResponse(location, eTag)
                ),
                CancellationToken.None
            )
        );

        return true;
    }

    protected async Task<bool> VerifyThatTicketHasCompleted(RoadSegmentId roadSegmentId)
    {
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);

        return VerifyThatTicketHasCompleted(string.Format(ConfigurationDetailUrl, roadSegmentId), roadSegment?.LastEventHash);
    }

    protected bool VerifyThatTicketHasError(string code, string message)
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        TicketingMock.Verify(x =>
            x.Error(It.IsAny<Guid>(),
                new TicketError(
                    message,
                    code),
                CancellationToken.None));

        return true;
    }

    protected abstract Task<bool> VerifyTicketAsync();
}
