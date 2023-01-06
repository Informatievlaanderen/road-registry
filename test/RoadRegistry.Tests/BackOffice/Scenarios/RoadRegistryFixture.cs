namespace RoadRegistry.Tests.BackOffice.Scenarios;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
using Framework.Testing;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Infrastructure.Modules;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using SqlStreamStore;
using Xunit.Abstractions;

public abstract class RoadRegistryFixture : AutofacBasedTest, IDisposable
{
    protected string ConfigDetailUrl => "http://base/{0}";

    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private readonly Func<EventSourcedEntityMap> _entityMapFactory;
    private ScenarioRunner _runner;

    protected RoadRegistryFixture(ITestOutputHelper testOutputHelper, ComparisonConfig comparisonConfig = null)
        : base(testOutputHelper)
    {
        _entityMapFactory = () => new EventSourcedEntityMap();

        Fixture = new Fixture();
        Fixture.Register(() => (ISnapshotStrategy)NoSnapshotStrategy.Instance);

        Client = new MemoryBlobClient();
        Clock = new FakeClock(NodaConstants.UnixEpoch);
        ZipArchiveValidator = new FakeZipArchiveAfterFeatureCompareValidator();
        LoggerFactory = new LoggerFactory();

        WithStore(new InMemoryStreamStore(), comparisonConfig);
        RoadRegistryContext = new RoadRegistryContext(new EventSourcedEntityMap(), Store, new FakeRoadNetworkSnapshotReader(), Settings, Mapping);
    }

    public MemoryBlobClient Client { get; }
    public FakeClock Clock { get; }
    public Fixture Fixture { get; }
    public IStreamStore Store { get; private set; }
    public IZipArchiveAfterFeatureCompareValidator ZipArchiveValidator { get; set; }
    protected IRoadRegistryContext RoadRegistryContext { get; }
    protected LoggerFactory LoggerFactory { get; }

    public void Dispose()
    {
        Store?.Dispose();
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

        return builder(new Scenario()).AssertAsync(_runner);
    }

    public RoadRegistryFixture WithStore(IStreamStore store, ComparisonConfig comparisonConfig = null)
    {
        Store = store.ThrowIfNull();

        _runner = new ScenarioRunner(
            Resolve.WhenEqualToMessage(new CommandHandlerModule[]
            {
                new RoadNetworkCommandModule(Store, _entityMapFactory, new FakeRoadNetworkSnapshotReader(), new FakeRoadNetworkSnapshotWriter(), Clock, LoggerFactory.CreateLogger<RoadNetworkCommandModule>()),
                new RoadNetworkExtractCommandModule(new RoadNetworkExtractUploadsBlobClient(Client), Store, _entityMapFactory, new FakeRoadNetworkSnapshotReader(), ZipArchiveValidator, Clock, LoggerFactory.CreateLogger<RoadNetworkExtractCommandModule>())
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

    protected override void ConfigureCommandHandling(ContainerBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:Events", "x" },
                { "ConnectionStrings:Snapshots", "x" },
                { "DetailUrl", ConfigDetailUrl }
            })
            .Build();

        builder.Register((a) => (IConfiguration)configuration);

        builder
            .RegisterModule(new CommandHandlingModule())
            .RegisterModule(new SqlStreamStoreModule())
            .RegisterModule(new SqlSnapshotStoreModule());

        //builder
        //    .Register(c => new MunicipalityFactory(Fixture.Create<ISnapshotStrategy>()))
        //    .As<IMunicipalityFactory>();
    }

    protected override void ConfigureEventHandling(ContainerBuilder builder)
    {
        var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
    }

    protected string FormatDetailUrl(object o) => string.Format(ConfigDetailUrl, o);
}
