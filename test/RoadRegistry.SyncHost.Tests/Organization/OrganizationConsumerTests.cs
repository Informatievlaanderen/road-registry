namespace RoadRegistry.SyncHost.Tests.Organization
{
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Editor.Schema;
    using Editor.Schema.Organizations;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json;
    using RoadRegistry.Tests.BackOffice;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Sync.OrganizationRegistry;
    using Organization = Sync.OrganizationRegistry.Models.Organization;

    public class OrganizationConsumerTests
    {
        private static readonly EventMapping Mapping = new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
        private static readonly JsonSerializerSettings Settings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly StreamNameConverter StreamNameConverter = StreamNameConversions.PassThru;

        private readonly Fixture _fixture;

        public OrganizationConsumerTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeOrganizationName();
            _fixture.CustomizeOrganizationOvoCode();
            _fixture.CustomizeOrganizationKboNumber();
        }

        private (OrganizationConsumer, IStreamStore) BuildSetup(IOrganizationReader organizationReader,
            Action<OrganizationConsumerContext>? configureOrganizationConsumerContext = null,
            Action<EditorContext>? configureEditorContext = null,
            ILoggerFactory? loggerFactory = null
        )
        {
            loggerFactory ??= new NullLoggerFactory();

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
                        context.SaveChanges();
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
                        context.SaveChanges();
                        return context;
                    }
                );

            var store = new InMemoryStreamStore();

            return (new OrganizationConsumer(
                containerBuilder.Build(),
                new OrganizationConsumerOptions
                {
                    ConsumerDelaySeconds = -1,
                    DisableWaitForEditorContextProjection = true
                },
                organizationReader,
                store,
                new RoadNetworkCommandQueue(store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
                loggerFactory
            ), store);
        }

        [Fact]
        public async Task GivenNoOrganization_ThenCreateOrganization()
        {
            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = _fixture.Create<OrganizationName>(),
                OvoNumber = _fixture.Create<OrganizationOvoCode>()
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));

            await consumer.StartAsync(CancellationToken.None);

            var createOrganizationMessage = await store.GetLastMessage<CreateOrganization>();

            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.Code);
            Assert.Equal(organization1.Name, createOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.OvoCode);
        }

        [Fact]
        public async Task GivenOrganization_ThenChangeOrganization()
        {
            var organizationId = _fixture.Create<OrganizationId>();
            var ovoCode = _fixture.Create<OrganizationOvoCode>();

            var existingOrganization = new CreateOrganizationAccepted
            {
                Code = organizationId,
                Name = _fixture.Create<OrganizationName>(),
                OvoCode = ovoCode,
                KboNumber = null
            };

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = _fixture.Create<OrganizationName>(),
                OvoNumber = ovoCode,
                KboNumber = _fixture.Create<OrganizationKboNumber>()
            };

            //TODO-rik gebruik existingOrganization voor onderstaande editorContext en Store initialisatie
            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1),
                configureEditorContext: editorContext =>
                {
                    editorContext.OrganizationsV2.Add(new OrganizationRecordV2
                    {
                        Id = 1,
                        Code = existingOrganization.Code,
                        Name = existingOrganization.Name,
                        OvoCode = existingOrganization.OvoCode,
                        KboNumber = existingOrganization.KboNumber,
                        IsMaintainer = _fixture.Create<bool>()
                    });
                });
            await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(organizationId), existingOrganization);

            await consumer.StartAsync(CancellationToken.None);

            var changeOrganizationMessage = await store.GetLastMessage<ChangeOrganization>();

            Assert.Equal(organization1.OvoNumber, changeOrganizationMessage.Code);
            Assert.Equal(organization1.Name, changeOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, changeOrganizationMessage.OvoCode);
            Assert.Equal(organization1.KboNumber, changeOrganizationMessage.KboNumber);
        }

        [Fact]
        public async Task WhenMultipleOrganizationAreFoundWithTheSameOvoCode_ThenLogsError()
        {
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            var ovoCode = _fixture.Create<OrganizationOvoCode>();

            var organization1Record = new OrganizationRecordV2
            {
                Code = _fixture.Create<OrganizationId>(),
                Name = _fixture.Create<OrganizationName>(),
                OvoCode = ovoCode
            };
            var organization2Record = new OrganizationRecordV2
            {
                Code = _fixture.Create<OrganizationId>(),
                Name = _fixture.Create<OrganizationName>(),
                OvoCode = ovoCode
            };
            Assert.Equal(organization1Record.OvoCode, organization2Record.OvoCode);

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = _fixture.Create<OrganizationName>(),
                OvoNumber = ovoCode
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1),
                configureEditorContext: context =>
                {
                    context.OrganizationsV2.Add(organization1Record);
                    context.OrganizationsV2.Add(organization2Record);
                },
                loggerFactory: loggerFactoryMock.Object);

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);

            Assert.Empty(page.Messages);
            Assert.Single(loggerMock.Invocations
                .Where(x => x.Arguments.Count >= 3
                            && Equals(x.Arguments[0], LogLevel.Error)
                            && x.Arguments[2].ToString().StartsWith($"Multiple Organizations found with a link to {ovoCode}")));
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

            var createOrganizationMessage = await store.GetLastMessage<CreateOrganization>();

            Assert.Equal(organization2.OvoNumber, createOrganizationMessage.Code);
            Assert.Equal(organization2.Name, createOrganizationMessage.Name);
            Assert.Equal(organization2.OvoNumber, createOrganizationMessage.OvoCode);
        }
    }
}
