namespace RoadRegistry.SyncHost.Tests.Organization
{
    using Autofac;
    using BackOffice;
    using BackOffice.Extracts.Dbase.Organizations;
    using BackOffice.Extracts.Dbase.Organizations.V2;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Editor.Projections;
    using Editor.Schema;
    using Editor.Schema.Extensions;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Sync.OrganizationRegistry;
    using Sync.OrganizationRegistry.Models;

    public class OrganizationConsumerTests
    {
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly FileEncoding _fileEncoding;
        private readonly ILoggerFactory _loggerFactory;

        public OrganizationConsumerTests(
            RecyclableMemoryStreamManager memoryStreamManager,
            FileEncoding fileEncoding,
            ILoggerFactory loggerFactory)
        {
            _memoryStreamManager = memoryStreamManager;
            _fileEncoding = fileEncoding;
            _loggerFactory = loggerFactory;
        }

        private (OrganizationConsumer, IStreamStore) BuildSetup(IOrganizationReader organizationReader,
            Action<OrganizationConsumerContext> configureOrganizationConsumerContext = null,
            Action<EditorContext> configureEditorContext = null
        )
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => new EventSourcedEntityMap());
            containerBuilder.Register(_ => new ConfigurationBuilder().Build()).As<IConfiguration>();
            containerBuilder.Register(_ => new LoggerFactory()).As<ILoggerFactory>();

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
                _loggerFactory
            ), store);
        }

        [Fact]
        public async Task CanConsumeOrganizationsSuccessfully()
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

        [Fact]
        public async Task CanUpdateOrganizationsWithOldOrganizationIdSuccessfully()
        {
            var organization1DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = "WR1" },
                LBLORG = { Value = "WR Org 1" },
                OVOCODE = { Value = "OVO100001" }
            };
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
                        Code = "WR1",
                        DbaseRecord = organization1DbaseRecord.ToBytes(_memoryStreamManager, _fileEncoding),
                        DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion
                    });
                });

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 2);

            Assert.Equal(2, page.Messages.Length);

            {
                var message = page.Messages[0];
                Assert.Equal(nameof(CreateOrganization), message.Type);
                var createOrganizationMessage = JsonConvert.DeserializeObject<CreateOrganization>(await message.GetJsonData());

                Assert.Equal(organization1.OvoNumber, createOrganizationMessage.Code);
                Assert.Equal(organization1.Name, createOrganizationMessage.Name);
                Assert.Equal(organization1.OvoNumber, createOrganizationMessage.OvoCode);
            }

            {
                var message = page.Messages[1];
                Assert.Equal(nameof(ChangeOrganization), message.Type);
                var changeOrganizationMessage = JsonConvert.DeserializeObject<ChangeOrganization>(await message.GetJsonData());
                
                Assert.Equal(organization1DbaseRecord.ORG.Value, changeOrganizationMessage.Code);
                Assert.Equal(organization1.Name, changeOrganizationMessage.Name);
                Assert.Equal(organization1.OvoNumber, changeOrganizationMessage.OvoCode);
            }
        }
    }
}
