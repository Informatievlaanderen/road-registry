namespace RoadRegistry.BackOffice.Scenarios
{
    using System;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Core;
    using Extracts;
    using Framework;
    using Messages;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Testing;
    using RoadRegistry.Framework.Testing;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Uploads;

    public abstract class RoadRegistryFixture : IDisposable
    {
        private static readonly JsonSerializerSettings Settings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly EventMapping Mapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        private readonly ScenarioRunner _runner;

        protected Fixture Fixture { get; }
        protected IStreamStore Store { get; }
        protected FakeClock Clock { get; }
        protected MemoryBlobClient Client { get; }

        protected IZipArchiveValidator ZipArchiveValidator { get; set; }

        private class FakeRoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
        {
            public Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken)
            {
                return Task.FromResult<(RoadNetworkSnapshot snapshot, int version)>((null, ExpectedVersion.NoStream));
            }
        }

        private class FakeRoadNetworkSnapshotWriter : IRoadNetworkSnapshotWriter
        {
            public Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        private class FakeZipArchiveValidator : IZipArchiveValidator
        {
            public ZipArchiveProblems Validate(ZipArchive archive)
            {
                return archive.GetEntry("error") != null
                    ? ZipArchiveProblems.Single(new FileError("error", "reason"))
                    : ZipArchiveProblems.None;
            }
        }

        protected RoadRegistryFixture()
        {
            Fixture = new Fixture();
            Client = new MemoryBlobClient();
            Store = new InMemoryStreamStore();
            Clock = new FakeClock(NodaConstants.UnixEpoch);
            ZipArchiveValidator = new FakeZipArchiveValidator();

            _runner = new ScenarioRunner(
                Resolve.WhenEqualToMessage(new CommandHandlerModule[] {
                        new RoadNetworkCommandModule(Store, new FakeRoadNetworkSnapshotReader(), new FakeRoadNetworkSnapshotWriter(), Clock),
                        new RoadNetworkExtractCommandModule(new RoadNetworkExtractUploadsBlobClient(Client), Store, new FakeRoadNetworkSnapshotReader(), ZipArchiveValidator, Clock)
                    }),
                Store,
                Settings,
                Mapping,
                StreamNameConversions.PassThru
            );
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

        public void Dispose()
        {
            Store?.Dispose();
        }
    }
}
