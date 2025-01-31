namespace RoadRegistry.Tests.BackOffice.Scenarios;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Framework.Testing;
using Hosts;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Infrastructure.Modules;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using SqlStreamStore;
using TicketingService.Abstractions;

public abstract class RoadRegistryTestBase : AutofacBasedTestBase, IDisposable
{
    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private ScenarioRunner _runner;

    protected RoadRegistryTestBase(ITestOutputHelper testOutputHelper, ComparisonConfig comparisonConfig = null)
        : base(testOutputHelper)
    {
        ScopedContainer = Container.BeginLifetimeScope();

        ObjectProvider = new Fixture();
        ObjectProvider.Register(() => (ISnapshotStrategy)NoSnapshotStrategy.Instance);

        Client = new MemoryBlobClient();
        Clock = new FakeClock(NodaConstants.UnixEpoch);
        ZipArchiveBeforeFeatureCompareValidator = new FakeZipArchiveBeforeFeatureCompareValidator();
        ExtractUploadFailedEmailClient = new FakeExtractUploadFailedEmailClient();
        LoggerFactory = Container.Resolve<ILoggerFactory>();

        WithStore(new InMemoryStreamStore(), comparisonConfig);
        RoadRegistryContext = new RoadRegistryContext(
            ScopedContainer.Resolve<EventSourcedEntityMap>(),
            Store,
            new FakeRoadNetworkSnapshotReader(),
            Settings,
            Mapping,
            EnrichEvent.WithTime(Clock),
            LoggerFactory);
        Organizations = new Organizations(
            ScopedContainer.Resolve<EventSourcedEntityMap>(),
            Store,
            Settings,
            Mapping);
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder.RegisterInstance(new EventSourcedEntityMap());
        containerBuilder.RegisterInstance(new FakeRoadNetworkIdGenerator()).As<IRoadNetworkIdGenerator>();
        containerBuilder.RegisterInstance(TicketingMock.Object);
    }

    protected ILifetimeScope ScopedContainer { get; }
    public MemoryBlobClient Client { get; }
    public FakeClock Clock { get; }
    public Fixture ObjectProvider { get; }
    public IStreamStore Store { get; private set; }
    public IZipArchiveBeforeFeatureCompareValidator ZipArchiveBeforeFeatureCompareValidator { get; set; }
    public IExtractUploadFailedEmailClient ExtractUploadFailedEmailClient { get; set; }
    protected IRoadRegistryContext RoadRegistryContext { get; }
    protected Organizations Organizations { get; }
    protected ILoggerFactory LoggerFactory { get; }
    protected Mock<ITicketing> TicketingMock { get; } = new();

    public void Dispose()
    {
        Store?.Dispose();
        ScopedContainer?.Dispose();
    }

    protected override void ConfigureCommandHandling(ContainerBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:Events", "x" },
                { "ConnectionStrings:Snapshots", "x" }
            })
            .Build();

        builder.Register(a => (IConfiguration)configuration);
        builder.RegisterInstance<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions());

        builder
            .RegisterModule(new CommandHandlingModule())
            .RegisterModule(new SqlStreamStoreModule())
            .RegisterModule(new SqlSnapshotStoreModule());
    }

    protected override void ConfigureEventHandling(ContainerBuilder builder)
    {
        var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
    }

    public Task Given(StreamName streamName, params object[] events)
    {
        return Given(events.Select(@event => new RecordedEvent(streamName, @event)).ToArray());
    }

    public Task Given(RecordedEvent[] events)
    {
        return _runner.WriteGivens(events);
    }

    public Task Run(Func<Scenario, IExpectExceptionScenarioBuilder> builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder(new Scenario()).AssertAsync(_runner);
    }

    public Task Run(Func<Scenario, IExpectEventsScenarioBuilder> builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        var scenarioBuilder = builder(new Scenario());

        var idGenerator = (FakeRoadNetworkIdGenerator)ScopedContainer.Resolve<IRoadNetworkIdGenerator>();
        idGenerator.SeedEvents(scenarioBuilder.Build()
            .Givens
            .Select(x => x.Event)
            .ToList());

        return scenarioBuilder.AssertAsync(_runner);
    }

    public RoadRegistryTestBase WithStore(IStreamStore store, ComparisonConfig comparisonConfig = null)
    {
        Store = store.ThrowIfNull();

        _runner = new ScenarioRunner(
            Resolve.WhenEqualToMessage(new CommandHandlerModule[]
            {
                new RoadNetworkCommandModule(
                    Store,
                    ScopedContainer,
                    new FakeRoadNetworkSnapshotReader(),
                    Clock,
                    new UseOvoCodeInChangeRoadNetworkFeatureToggle(true),
                    new FakeExtractUploadFailedEmailClient(),
                    LoggerFactory),
                new RoadNetworkExtractCommandModule(
                    new RoadNetworkExtractUploadsBlobClient(Client),
                    Store,
                    ScopedContainer,
                    new FakeRoadNetworkSnapshotReader(),
                    ZipArchiveBeforeFeatureCompareValidator,
                    ExtractUploadFailedEmailClient,
                    Clock,
                    LoggerFactory),
                new OrganizationCommandModule(
                    Store,
                    ScopedContainer,
                    new FakeRoadNetworkSnapshotReader(),
                    Clock,
                    LoggerFactory)
            }),
            Store,
            Settings,
            Mapping,
            StreamNameConversions.PassThru
        )
        {
            ComparisonConfig = comparisonConfig
        };

        return this;
    }
}
