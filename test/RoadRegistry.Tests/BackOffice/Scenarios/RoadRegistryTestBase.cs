namespace RoadRegistry.Tests.BackOffice.Scenarios;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Extensions;
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
using RoadNetwork;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Infrastructure.Modules;
using RoadRegistry.BackOffice.Messages;
using ScopedRoadNetwork;
using SqlStreamStore;
using TicketingService.Abstractions;

public abstract class RoadRegistryTestBase : AutofacBasedTestBase, IDisposable
{
    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private ScenarioRunner? _runner;
    private Func<ScenarioRunner> _runnerFactory;
    private ScenarioRunner Runner => _runner ??= _runnerFactory();

    protected RoadRegistryTestBase(ITestOutputHelper testOutputHelper, ComparisonConfig comparisonConfig = null)
        : base(testOutputHelper)
    {
        ObjectProvider = FixtureFactory.Create();
        ObjectProvider.Register(() => (ISnapshotStrategy)NoSnapshotStrategy.Instance);

        Client = new MemoryBlobClient();
        Clock = new FakeClock(NodaConstants.UnixEpoch);
        ZipArchiveBeforeFeatureCompareValidator = new FakeZipArchiveBeforeFeatureCompareValidator();
        ExtractUploadFailedEmailClient = new FakeExtractUploadFailedEmailClient();
        SetStore(new InMemoryStreamStore(), comparisonConfig);

        RoadRegistryContext = ScopedContainer.Resolve<IRoadRegistryContext>();
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder.RegisterModule<ContextModule>();

        containerBuilder.RegisterInstance(new EventSourcedEntityMap());
        containerBuilder.RegisterInstance(new FakeRoadNetworkIdGenerator()).As<IRoadNetworkIdGenerator>();
        containerBuilder.RegisterInstance(TicketingMock.Object);
        containerBuilder.RegisterInstance(Store);
        containerBuilder.RegisterInstance(LoggerFactory);
        containerBuilder.RegisterInstance(EnrichEvent.WithTime(Clock));
        containerBuilder.RegisterInstance(new FakeRoadNetworkSnapshotReader()).As<IRoadNetworkSnapshotReader>();
    }

    private ILifetimeScope? _scopedContainer;
    protected ILifetimeScope ScopedContainer => _scopedContainer ??= Container.BeginLifetimeScope();
    public MemoryBlobClient Client { get; }
    public FakeClock Clock { get; }
    public Fixture ObjectProvider { get; }
    public IStreamStore Store { get; private set; }
    public IZipArchiveBeforeFeatureCompareValidator ZipArchiveBeforeFeatureCompareValidator { get; set; }
    public IExtractUploadFailedEmailClient ExtractUploadFailedEmailClient { get; set; }
    protected IRoadRegistryContext RoadRegistryContext { get; }
    protected ILoggerFactory LoggerFactory { get; } = new NullLoggerFactory();
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

        builder.Register(_ => (IConfiguration)configuration);
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
        var idGenerator = (FakeRoadNetworkIdGenerator)ScopedContainer.Resolve<IRoadNetworkIdGenerator>();
        idGenerator.SeedEvents(events
            .Select(x => x.Event)
            .ToList());

        return Runner.WriteGivens(events);
    }

    public Task Run(Func<Scenario, IExpectExceptionScenarioBuilder> builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder(new Scenario()).AssertAsync(Runner);
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

        return scenarioBuilder.AssertAsync(Runner);
    }

    private void SetStore(IStreamStore store, ComparisonConfig? comparisonConfig)
    {
        Store = store.ThrowIfNull();

        var zipArchiveBeforeFeatureCompareValidatorFactoryMock = new Mock<IZipArchiveBeforeFeatureCompareValidatorFactory>();
        zipArchiveBeforeFeatureCompareValidatorFactoryMock
            .Setup(x => x.Create(It.IsAny<string>()))
            .Returns(ZipArchiveBeforeFeatureCompareValidator);

        _runnerFactory = () => new ScenarioRunner(
            Resolve.WhenEqualToMessage([
                new RoadNetworkCommandModule(
                    Store,
                    ScopedContainer,
                    Container.Resolve<IRoadNetworkSnapshotReader>(),
                    Clock,
                    new FakeExtractUploadFailedEmailClient(),
                    LoggerFactory),
                new RoadNetworkExtractCommandModule(
                    new RoadNetworkExtractUploadsBlobClient(Client),
                    Store,
                    ScopedContainer,
                    Container.Resolve<IRoadNetworkSnapshotReader>(),
                    zipArchiveBeforeFeatureCompareValidatorFactoryMock.Object,
                    ExtractUploadFailedEmailClient,
                    Clock,
                    LoggerFactory),
                new OrganizationCommandModule(
                    Store,
                    ScopedContainer,
                    Container.Resolve<IRoadNetworkSnapshotReader>(),
                    Clock,
                    LoggerFactory)
            ]),
            Store,
            Settings,
            Mapping,
            StreamNameConversions.PassThru
        )
        {
            ComparisonConfig = comparisonConfig
        };
    }
}
