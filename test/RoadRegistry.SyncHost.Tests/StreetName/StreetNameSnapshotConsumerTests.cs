namespace RoadRegistry.SyncHost.Tests.StreetName
{
    using Autofac;
    using BackOffice;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Straatnaam;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Testing;
    using RoadRegistry.StreetName;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Sync.StreetNameRegistry;

    public class StreetNameSnapshotConsumerTests
    {
        private readonly ILoggerFactory _loggerFactory;

        public StreetNameSnapshotConsumerTests(
            ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        [Fact]
        public async Task CanConsumeSuccessfully_Created()
        {
            var streetName1 = new StreetNameSnapshotRecord
            {
                Identificator = new DeseriazableIdentificator("https://data.vlaanderen.be/id/straatnaam", "1", ""),
                Gemeente = new StraatnaamDetailGemeente
                {
                    ObjectId = "12345",
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Gemeente", Taal.NL))
                },
                StraatnaamStatus = "inGebruik",
                Straatnamen = new List<DeseriazableGeografischeNaam>
                {
                    new("Straat", Taal.NL)
                }
            };

            var (consumer, store, topicConsumer) = BuildSetup();

            topicConsumer.SeedMessage(streetName1.Identificator.Id, streetName1);

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);
            var message = Assert.Single(page.Messages);
            Assert.Equal(nameof(StreetNameCreated), message.Type);
            var createdMessage = JsonConvert.DeserializeObject<StreetNameCreated>(await message.GetJsonData());

            Assert.Equal(streetName1.Identificator.Id, createdMessage.Record.StreetNameId);
            Assert.Equal(streetName1.Identificator.ObjectId, createdMessage.Record.PersistentLocalId.ToString());
            Assert.Equal(streetName1.Gemeente.ObjectId, createdMessage.Record.NisCode);
            Assert.Equal(streetName1.StraatnaamStatus, createdMessage.Record.StreetNameStatus);
            Assert.Equal(streetName1.Straatnamen.Single().Spelling, createdMessage.Record.DutchName);
        }

        [Fact]
        public async Task CanConsumeSuccessfully_Modified()
        {
            var streetNameObjectId = "1";

            var streetName1 = new StreetNameSnapshotRecord
            {
                Identificator = new DeseriazableIdentificator("https://data.vlaanderen.be/id/straatnaam", streetNameObjectId, ""),
                Gemeente = new StraatnaamDetailGemeente
                {
                    ObjectId = "12345",
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Gemeente", Taal.NL))
                },
                StraatnaamStatus = "inGebruik",
                Straatnamen = new List<DeseriazableGeografischeNaam>
                {
                    new("Straat", Taal.NL)
                }
            };

            var streetName1Modification = new StreetNameSnapshotRecord
            {
                Identificator = new DeseriazableIdentificator("https://data.vlaanderen.be/id/straatnaam", streetNameObjectId, ""),
                Gemeente = new StraatnaamDetailGemeente
                {
                    ObjectId = "12345",
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Gemeente", Taal.NL))
                },
                StraatnaamStatus = "gehistoreerd",
                Straatnamen = new List<DeseriazableGeografischeNaam>
                {
                    new("Straat changed", Taal.NL)
                },
                HomoniemToevoegingen = new List<DeseriazableGeografischeNaam>
                {
                    new("Homonieme toevoeging", Taal.NL)
                }
            };

            var (consumer, store, topicConsumer) = BuildSetup();

            topicConsumer
                .SeedMessage(streetName1.Identificator.Id, streetName1)
                .SeedMessage(streetName1Modification.Identificator.Id, streetName1Modification)
                ;

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 2);
            var message = page.Messages[1];
            Assert.Equal(nameof(StreetNameModified), message.Type);
            var modifiedMessage = JsonConvert.DeserializeObject<StreetNameModified>(await message.GetJsonData());

            Assert.True(modifiedMessage.NameModified);
            Assert.True(modifiedMessage.StatusModified);
            Assert.True(modifiedMessage.HomonymAdditionModified);
            Assert.Equal(streetName1Modification.Identificator.Id, modifiedMessage.Record.StreetNameId);
            Assert.Equal(streetName1Modification.Identificator.ObjectId, modifiedMessage.Record.PersistentLocalId.ToString());
            Assert.Equal(streetName1Modification.Gemeente.ObjectId, modifiedMessage.Record.NisCode);
            Assert.Equal(streetName1Modification.StraatnaamStatus, modifiedMessage.Record.StreetNameStatus);
            Assert.Equal(streetName1Modification.Straatnamen.Single().Spelling, modifiedMessage.Record.DutchName);
            Assert.Equal(streetName1Modification.HomoniemToevoegingen.Single().Spelling, modifiedMessage.Record.DutchHomonymAddition);
        }

        [Fact]
        public async Task CanConsumeSuccessfully_Removed()
        {
            var streetNameObjectId = "1";
            var streetNameId = "https://data.vlaanderen.be/id/straatnaam/1";

            var streetName1 = new StreetNameSnapshotRecord
            {
                Identificator = new DeseriazableIdentificator("https://data.vlaanderen.be/id/straatnaam", streetNameObjectId, ""),
                Gemeente = new StraatnaamDetailGemeente
                {
                    ObjectId = "12345",
                    Gemeentenaam = new Gemeentenaam(new GeografischeNaam("Gemeente", Taal.NL))
                },
                StraatnaamStatus = "inGebruik",
                Straatnamen = new List<DeseriazableGeografischeNaam>
                {
                    new("Straat", Taal.NL)
                }
            };

            var streetName1Remove = new StreetNameSnapshotRecord();

            var (consumer, store, topicConsumer) = BuildSetup();

            topicConsumer
                .SeedMessage(streetName1.Identificator.Id, streetName1)
                .SeedMessage(streetNameId, streetName1Remove)
                ;

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 3);
            {
                var streamMessage = page.Messages[1];
                Assert.Equal(nameof(StreetNameRemoved), streamMessage.Type);
                Assert.Equal($"streetname-{streetNameObjectId}", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<StreetNameRemoved>(await streamMessage.GetJsonData());
                Assert.Equal(streetNameId, message.StreetNameId);
            }
            {
                var streamMessage = page.Messages[2];
                Assert.Equal(nameof(UnlinkRoadSegmentsFromStreetName), streamMessage.Type);
                Assert.Equal("roadnetwork-command-queue", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<UnlinkRoadSegmentsFromStreetName>(await streamMessage.GetJsonData());
                Assert.Equal(streetNameObjectId, message.Id.ToString());
            }
        }
        
        private (StreetNameSnapshotConsumer, IStreamStore, InMemoryStreetNameSnapshotTopicConsumer) BuildSetup(
            Action<StreetNameSnapshotConsumerContext> configureDbContext = null
        )
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => new EventSourcedEntityMap());
            containerBuilder.Register(_ => new ConfigurationBuilder().Build()).As<IConfiguration>();
            containerBuilder.Register(_ => new LoggerFactory()).As<ILoggerFactory>();

            containerBuilder
                .RegisterDbContext<StreetNameSnapshotConsumerContext>(string.Empty,
                    _ => { }
                    , dbContextOptionsBuilder =>
                    {
                        var context = new StreetNameSnapshotConsumerContext(dbContextOptionsBuilder.Options);
                        configureDbContext?.Invoke(context);
                        return context;
                    }
                );

            var lifetimeScope = containerBuilder.Build();

            var store = new InMemoryStreamStore();
            var topicConsumer = new InMemoryStreetNameSnapshotTopicConsumer(() => lifetimeScope.Resolve<StreetNameSnapshotConsumerContext>());

            return (new StreetNameSnapshotConsumer(
                lifetimeScope,
                store,
                new StreetNameEventWriter(store, EnrichEvent.WithTime(new FakeClock(NodaConstants.UnixEpoch))),
                new RoadNetworkCommandQueue(store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
                topicConsumer,
                _loggerFactory.CreateLogger<StreetNameSnapshotConsumer>()
            ), store, topicConsumer);
        }
    }
}
