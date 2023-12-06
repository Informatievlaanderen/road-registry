namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Autofac;
using AutoFixture;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Editor.Schema;
using Editor.Schema.Extensions;
using Hosts;
using Infrastructure;
using Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
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
    protected readonly IChangeRoadNetworkDispatcher ChangeRoadNetworkDispatcher;
    protected readonly IConfiguration Configuration;
    protected readonly ICustomRetryPolicy CustomRetryPolicy;
    protected readonly IIdempotentCommandHandler IdempotentCommandHandler;
    protected readonly ILifetimeScope LifetimeScope;
    protected readonly ILoggerFactory LoggerFactory;
    protected readonly SqsLambdaHandlerOptions Options;
    protected readonly IRoadRegistryContext RoadRegistryContext;
    protected readonly IStreamStore Store;

    protected SqsLambdaHandlerFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options
    )
    {
        TestData = new RoadNetworkTestData(CustomizeTestData);
        TestData.CopyCustomizationsTo(ObjectProvider);

        Configuration = configuration;
        CustomRetryPolicy = customRetryPolicy;
        Store = new InMemoryStreamStore();

        var containerBuilder = new ContainerBuilder();
        containerBuilder
            .Register(_ => new EventSourcedEntityMap())
            .AsSelf()
            .SingleInstance();
        containerBuilder.RegisterInstance(new FakeRoadNetworkIdGenerator()).As<IRoadNetworkIdGenerator>();
        var container = containerBuilder.Build();
        LifetimeScope = container.BeginLifetimeScope();

        RoadRegistryContext = new RoadRegistryContext(LifetimeScope.Resolve<EventSourcedEntityMap>(), Store, new FakeRoadNetworkSnapshotReader(), Settings, Mapping, new NullLoggerFactory());
        EditorContext = new FakeEditorContextFactory().CreateDbContext(Array.Empty<string>());

        Clock = clock;
        Options = options;
        LoggerFactory = new LoggerFactory();

        TicketingMock = MockTicketing();

        IdempotentCommandHandler = new RoadRegistryIdempotentCommandHandler(BuildCommandHandlerDispatcher());
        RecyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        FileEncoding = FileEncoding.UTF8;

        ChangeRoadNetworkDispatcher = new ChangeRoadNetworkDispatcher(
            new RoadNetworkCommandQueue(Store, new ApplicationMetadata(RoadRegistryApplication.Lambda)),
            IdempotentCommandHandler,
            LifetimeScope.Resolve<EventSourcedEntityMap>(),
            new FakeOrganizationRepository(),
            LoggerFactory.CreateLogger<ChangeRoadNetworkDispatcher>());

        Exception = null;
    }

    protected RoadNetworkTestData TestData { get; }
    public IClock Clock { get; }
    protected abstract TSqsLambdaRequestHandler SqsLambdaRequestHandler { get; }
    protected abstract TSqsLambdaRequest SqsLambdaRequest { get; }
    protected abstract TSqsRequest SqsRequest { get; }
    protected Mock<ITicketing> TicketingMock { get; }
    public bool Result { get; private set; }
    public Exception? Exception { get; private set; }

    public EditorContext EditorContext { get; }
    public RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; }
    public FileEncoding FileEncoding { get; }


    public async Task InitializeAsync()
    {
        try
        {
            await EditorContext.Organizations.AddAsync(
                new OrganizationRecord
                {
                    Code = "AGIV",
                    SortableCode = "AGIV",
                    DbaseSchemaVersion = BackOffice.Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord.DbaseSchemaVersion,
                    DbaseRecord = new BackOffice.Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord
                    {
                        ORG = { Value = "AGIV" },
                        LBLORG = { Value = "Agentschap voor Geografische Informatie Vlaanderen" }
                    }.ToBytes(RecyclableMemoryStreamManager, FileEncoding)
                });
            await EditorContext.SaveChangesAsync();

            await SetupAsync();

            await SqsLambdaRequestHandler.Handle(SqsLambdaRequest, CancellationToken.None);

            Result = await VerifyTicketAsync();
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
    }

    public async Task DisposeAsync()
    {
        await LifetimeScope.DisposeAsync();
    }

    protected abstract CommandHandlerDispatcher BuildCommandHandlerDispatcher();

    protected virtual void CustomizeTestData(Fixture fixture)
    {
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
        return VerifyThatTicketHasCompleted(new ETagResponse(location, eTag));
    }

    protected bool VerifyThatTicketHasCompleted(object response)
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        TicketingMock.Verify(x =>
            x.Complete(
                It.IsAny<Guid>(),
                new TicketResult(response),
                CancellationToken.None
            )
        );

        return true;
    }

    protected async Task<bool> VerifyThatTicketHasCompleted(RoadSegmentId roadSegmentId)
    {
        var roadNetwork = await RoadRegistryContext.RoadNetworks.ForOutlinedRoadSegment(roadSegmentId, CancellationToken.None);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);

        return VerifyThatTicketHasCompleted(string.Format(ConfigurationDetailUrl, roadSegmentId), roadSegment?.LastEventHash ?? string.Empty);
    }

    protected virtual bool VerifyThatTicketHasError(string code, string message)
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
