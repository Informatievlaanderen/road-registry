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
    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

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

    protected Fixture Fixture { get; }
    protected IStreamStore Store { get; }
    protected FakeClock Clock { get; }
    protected MemoryBlobClient Client { get; }
    protected IZipArchiveAfterFeatureCompareValidator ZipArchiveValidator { get; set; }

    public void Dispose()
    {
        Store?.Dispose();
    }

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
}
