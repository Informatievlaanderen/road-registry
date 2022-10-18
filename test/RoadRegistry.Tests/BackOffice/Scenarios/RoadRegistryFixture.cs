namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework.Testing;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using SqlStreamStore;

public abstract class RoadRegistryFixture : IDisposable
{
    private readonly ScenarioRunner _runner;

    protected RoadRegistryFixture(ComparisonConfig comparisonConfig = null)
    {
        Fixture = new Fixture();
        Client = new MemoryBlobClient();
        Store = new InMemoryStreamStore();
        Clock = new FakeClock(NodaConstants.UnixEpoch);
        ZipArchiveValidator = new FakeZipArchiveAfterFeatureCompareValidator();

        _runner = new ScenarioRunner(
            Resolve.WhenEqualToMessage(new CommandHandlerModule[]
            {
                new RoadNetworkCommandModule(Store, new FakeRoadNetworkSnapshotReader(), new FakeRoadNetworkSnapshotWriter(), Clock),
                new RoadNetworkExtractCommandModule(new RoadNetworkExtractUploadsBlobClient(Client), Store, new FakeRoadNetworkSnapshotReader(), ZipArchiveValidator, Clock)
            }),
            Store,
            Settings,
            Mapping,
            StreamNameConversions.PassThru
        )
        {
            ComparisonConfig = comparisonConfig
        };
    }

    protected MemoryBlobClient Client { get; }
    protected FakeClock Clock { get; }

    public void Dispose()
    {
        Store?.Dispose();
    }

    protected Fixture Fixture { get; }

    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    protected Task Run(Func<Scenario, IExpectExceptionScenarioBuilder> builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        return builder(new Scenario()).AssertAsync(_runner);
    }

    protected Task Run(Func<Scenario, IExpectEventsScenarioBuilder> builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        return builder(new Scenario()).AssertAsync(_runner);
    }

    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    protected IStreamStore Store { get; }
    protected IZipArchiveAfterFeatureCompareValidator ZipArchiveValidator { get; set; }
}