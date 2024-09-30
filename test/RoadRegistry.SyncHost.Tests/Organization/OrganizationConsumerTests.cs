namespace RoadRegistry.SyncHost.Tests.Organization
{
    using Autofac;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Extracts.Dbase.Organizations;
    using BackOffice.Extracts.Dbase.Organizations.V2;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Editor.Schema;
    using Editor.Schema.Extensions;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Moq;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Sync.OrganizationRegistry;
    using Organization = Sync.OrganizationRegistry.Models.Organization;
    //TODO-rik update tests for KboNumber
    public class OrganizationConsumerTests
    {
        private static readonly EventMapping Mapping = new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
        private static readonly JsonSerializerSettings Settings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly StreamNameConverter StreamNameConverter = StreamNameConversions.PassThru;

        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly FileEncoding _fileEncoding;

        public OrganizationConsumerTests(
            RecyclableMemoryStreamManager memoryStreamManager,
            FileEncoding fileEncoding)
        {
            _memoryStreamManager = memoryStreamManager;
            _fileEncoding = fileEncoding;
        }

        private (OrganizationConsumer, IStreamStore) BuildSetup(IOrganizationReader organizationReader,
            Action<OrganizationConsumerContext>? configureOrganizationConsumerContext = null,
            Action<EditorContext>? configureEditorContext = null,
            ILoggerFactory? loggerFactory = null
        )
        {
            loggerFactory ??= new LoggerFactory();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => new EventSourcedEntityMap());
            containerBuilder.Register(_ => new ConfigurationBuilder().Build()).As<IConfiguration>();
            containerBuilder.Register(_ => loggerFactory).As<ILoggerFactory>();

            containerBuilder
                .RegisterDbContext<OrganizationConsumerContext>(string.Empty,
                    _ => { }
                    , dbContextOptionsBuilder =>
                    {
                        var context = new OrganizationConsumerContext(dbContextOptionsBuilder.Options);
                        configureOrganizationConsumerContext?.Invoke(context);
                        return context;
                    }
                );
            containerBuilder
                .RegisterDbContext<EditorContext>(string.Empty,
                    _ => { }
                    , dbContextOptionsBuilder =>
                    {
                        var context = new EditorContext(dbContextOptionsBuilder.Options);
                        configureEditorContext?.Invoke(context);
                        return context;
                    }
                );

            var store = new InMemoryStreamStore();

            return (new OrganizationConsumer(
                containerBuilder.Build(),
                new OrganizationConsumerOptions { ConsumerDelaySeconds = -1 },
                organizationReader,
                store,
                _memoryStreamManager,
                _fileEncoding,
                new RoadNetworkCommandQueue(store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
                loggerFactory
            ), store);
        }

        [Fact]
        public async Task CreatesOrganization()
        {
            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = "Org 1",
                OvoNumber = "OVO100001"
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);
            var message = Assert.Single(page.Messages);
            Assert.Equal(nameof(CreateOrganization), message.Type);
            var createOrganizationMessage = JsonConvert.DeserializeObject<CreateOrganization>(await message.GetJsonData());

            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.Code);
            Assert.Equal(organization1.Name, createOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.OvoCode);
        }

        [Fact]
        public async Task RenamesOrganization()
        {
            var ovoCode = new OrganizationOvoCode("OVO100001");

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = "Org 1",
                OvoNumber = ovoCode
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));
            await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(new OrganizationId(ovoCode)), new CreateOrganizationAccepted
            {
                Code = ovoCode,
                Name = "WR Org 1",
                OvoCode = ovoCode
            });

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 2);
            var message = page.Messages[1];
            Assert.Equal(nameof(ChangeOrganization), message.Type);
            var changeOrganizationMessage = JsonConvert.DeserializeObject<ChangeOrganization>(await message.GetJsonData());

            Assert.Equal(organization1.OvoNumber, changeOrganizationMessage.Code);
            Assert.Equal(organization1.Name, changeOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, changeOrganizationMessage.OvoCode);
        }

        [Fact]
        public async Task DoesNothingWhenNoChangesAreDetected()
        {
            var ovoCode = new OrganizationOvoCode("OVO100001");

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = "Org 1",
                OvoNumber = ovoCode
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));
            await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(new OrganizationId(ovoCode)), new CreateOrganizationAccepted
            {
                Code = ovoCode,
                Name = "Org 1",
                OvoCode = ovoCode
            });

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 2);
            Assert.Single(page.Messages);
        }

        [Fact]
        public async Task LogsErrorWhenMultipleOrganizationAreFoundWithTheSameOvoCode()
        {
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            var organization1DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = "WR1" },
                LBLORG = { Value = "WR Org 1" },
                OVOCODE = { Value = "OVO100001" }
            };
            var organization2DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = "WR2" },
                LBLORG = { Value = "WR Org 2" },
                OVOCODE = { Value = "OVO100001" }
            };
            Assert.Equal(organization1DbaseRecord.OVOCODE.Value, organization2DbaseRecord.OVOCODE.Value);

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = "Org 1",
                OvoNumber = "OVO100001"
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1),
                configureEditorContext: context =>
                {
                    context.Organizations.Add(new OrganizationRecord
                    {
                        Code = organization1DbaseRecord.ORG.Value,
                        DbaseRecord = organization1DbaseRecord.ToBytes(_memoryStreamManager, _fileEncoding),
                        DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion
                    });
                    context.Organizations.Add(new OrganizationRecord
                    {
                        Code = organization2DbaseRecord.ORG.Value,
                        DbaseRecord = organization2DbaseRecord.ToBytes(_memoryStreamManager, _fileEncoding),
                        DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion
                    });
                },
                loggerFactory: loggerFactoryMock.Object);

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);

            Assert.Empty(page.Messages);
            Assert.Single(loggerMock.Invocations
                .Where(x => x.Arguments.Count >= 3
                            && Equals(x.Arguments[0], LogLevel.Error)
                            && x.Arguments[2].ToString().StartsWith("Multiple Organizations found with a link to OVO100001")));
        }

        [Fact]
        public async Task CanResumeConsumingSuccessfully()
        {
            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = "Org 1",
                OvoNumber = "OVO100001"
            };
            var organization2 = new Organization
            {
                ChangeId = 2,
                Name = "Org 2",
                OvoNumber = "OVO100002"
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1, organization2),
                configureOrganizationConsumerContext: dbContext =>
                {
                    dbContext.ProjectionStates.Add(new ProjectionStateItem
                    {
                        Name = OrganizationConsumer.ProjectionStateName,
                        Position = 1
                    });
                });

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);
            var message = Assert.Single(page.Messages);
            Assert.Equal(nameof(CreateOrganization), message.Type);
            var createOrganizationMessage = JsonConvert.DeserializeObject<CreateOrganization>(await message.GetJsonData());

            Assert.Equal(organization2.OvoNumber, createOrganizationMessage.Code);
            Assert.Equal(organization2.Name, createOrganizationMessage.Name);
            Assert.Equal(organization2.OvoNumber, createOrganizationMessage.OvoCode);
        }
    }
}
