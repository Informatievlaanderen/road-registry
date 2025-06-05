namespace RoadRegistry.SyncHost.Tests.Organization
{
    using System.Text.Json;
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
    using Microsoft.Extensions.Configuration.Json;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json;
    using RoadRegistry.Tests.BackOffice;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Sync.OrganizationRegistry;
    using Sync.OrganizationRegistry.Models;
    using SyncHost.Organization;
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
                new OrganizationCommandQueue(store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
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

            Assert.Equal(organizationId, changeOrganizationMessage.Code);
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
            Assert.Single(loggerMock.Invocations,
                x => x.Arguments.Count >= 3
                     && Equals(x.Arguments[0], LogLevel.Error)
                     && x.Arguments[2].ToString().StartsWith($"Multiple Organizations found with a link to {ovoCode}"));
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

        [Fact]
        public async Task WithOneActiveNisCode_ThenNisCodeIsUsedAsCode()
        {
            var nisCode = "10001";

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = _fixture.Create<OrganizationName>(),
                OvoNumber = _fixture.Create<OrganizationOvoCode>(),
                Keys =
                [
                    new Key
                    {
                        KeyTypeName = "NIS",
                        Value = nisCode,
                        Validity = new Validity
                        {
                            Start = new DateTime(2000,1,1)
                        }
                    }
                ]
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));

            await consumer.StartAsync(CancellationToken.None);

            var createOrganizationMessage = await store.GetLastMessage<CreateOrganization>();

            Assert.Equal(nisCode, createOrganizationMessage.Code);
            Assert.Equal(organization1.Name, createOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.OvoCode);
        }

        [Fact]
        public async Task WithOneExpiredNisCode_ThenNisCodeIsUsedAsCode()
        {
            var nisCode = "10001";

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = _fixture.Create<OrganizationName>(),
                OvoNumber = _fixture.Create<OrganizationOvoCode>(),
                Keys =
                [
                    new Key
                    {
                        KeyTypeName = "NIS",
                        Value = nisCode,
                        Validity = new Validity
                        {
                            Start = new DateTime(2000,1,1),
                            End = new DateTime(2010,1,1)
                        }
                    }
                ]
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));

            await consumer.StartAsync(CancellationToken.None);

            var createOrganizationMessage = await store.GetLastMessage<CreateOrganization>();

            Assert.Equal(nisCode, createOrganizationMessage.Code);
            Assert.Equal(organization1.Name, createOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.OvoCode);
        }

        [Fact]
        public async Task WithMultipleNisCodes_ThenCurrentNisCodeIsUsedAsCode()
        {
            var nisCode1 = "10001";
            var nisCode2 = "10002";

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = _fixture.Create<OrganizationName>(),
                OvoNumber = _fixture.Create<OrganizationOvoCode>(),
                Keys =
                [
                    new Key
                    {
                        KeyTypeName = "NIS",
                        Value = nisCode1,
                        Validity = new Validity
                        {
                            Start = new DateTime(2000,1,1),
                            End = new DateTime(2010,1,1)
                        }
                    },
                    new Key
                    {
                        KeyTypeName = "NIS",
                        Value = nisCode2,
                        Validity = new Validity
                        {
                            Start = new DateTime(2010,1,2)
                        }
                    }
                ]
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));

            await consumer.StartAsync(CancellationToken.None);

            var createOrganizationMessage = await store.GetLastMessage<CreateOrganization>();

            Assert.Equal(nisCode2, createOrganizationMessage.Code);
            Assert.Equal(organization1.Name, createOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.OvoCode);
        }

        [Fact]
        public async Task WithFutureNisCode_ThenCurrentNisCodeIsUsedAsCode()
        {
            var nisCode1 = "10001";
            var nisCode2 = "10002";

            var organization1 = new Organization
            {
                ChangeId = 1,
                Name = _fixture.Create<OrganizationName>(),
                OvoNumber = _fixture.Create<OrganizationOvoCode>(),
                Keys =
                [
                    new Key
                    {
                        KeyTypeName = "NIS",
                        Value = nisCode1,
                        Validity = new Validity
                        {
                            Start = new DateTime(2000,1,1),
                            End = new DateTime(2999,12,31)
                        }
                    },
                    new Key
                    {
                        KeyTypeName = "NIS",
                        Value = nisCode2,
                        Validity = new Validity
                        {
                            Start = new DateTime(3000,1,1)
                        }
                    }
                ]
            };

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organization1));

            await consumer.StartAsync(CancellationToken.None);

            var createOrganizationMessage = await store.GetLastMessage<CreateOrganization>();

            Assert.Equal(nisCode1, createOrganizationMessage.Code);
            Assert.Equal(organization1.Name, createOrganizationMessage.Name);
            Assert.Equal(organization1.OvoNumber, createOrganizationMessage.OvoCode);
        }

        [Fact]
        public async Task WithJsonResponse_ThenCurrentNisCodeIsUsedAsCode()
        {
            var json = """
                       [
                         {
                           "changeId": 1117298,
                           "changeTime": "2025-06-05",
                           "id": "7c26f2cf-0e39-478e-a273-203d9bdb4072",
                           "name": "Gemeente Wingene",
                           "ovoNumber": "OVO002277",
                           "validity": {},
                           "kboNumber": "0207495470",
                           "keys": [
                             {
                               "organisationKeyId": "9b9276ab-d3b5-43e7-ad5d-b9d2615bd70c",
                               "keyTypeId": "5f1e81fa-b638-4c48-9bf8-125b3a203724",
                               "keyTypeName": "Piavo_Sortering",
                               "value": "40_2860",
                               "validity": {
                                 "start": "1977-01-01"
                               }
                             },
                             {
                               "organisationKeyId": "1de7739b-824b-4395-9016-ce24c89695f7",
                               "keyTypeId": "371462ac-f885-4b1e-b657-b04da30a6377",
                               "keyTypeName": "NIS",
                               "value": "37018",
                               "validity": {
                                 "start": "1977-01-01"
                               }
                             },
                             {
                               "organisationKeyId": "565941a1-88dd-4d30-896e-98559d2c5132",
                               "keyTypeId": "e26ee59c-e992-4e18-9ea9-ee3b33d7ba06",
                               "keyTypeName": "INR",
                               "value": "494",
                               "validity": {
                                 "start": "1977-01-01"
                               }
                             },
                             {
                               "organisationKeyId": "b0373838-efe8-4626-b7b3-2c2540bee315",
                               "keyTypeId": "95dfef4a-ba38-47d7-bd76-05151aec8141",
                               "keyTypeName": "KBO",
                               "value": "0207.495.470",
                               "validity": {
                                 "start": "1977-01-01"
                               }
                             },
                             {
                               "organisationKeyId": "6aae3324-96eb-4557-ab2d-2430c9c6e250",
                               "keyTypeId": "7f2994e5-211a-9c15-0d0e-c32b709774d8",
                               "keyTypeName": "CIB",
                               "value": "CIB000037242",
                               "validity": {}
                             }
                           ]
                         }
                       ]
                       """;
            using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            var organizations = await System.Text.Json.JsonSerializer.DeserializeAsync<IEnumerable<Organization>>(jsonStream, JsonSerializerOptions.Web);

            var (consumer, store) = BuildSetup(new FakeOrganizationReader(organizations!.ToArray()));

            await consumer.StartAsync(CancellationToken.None);

            var createOrganizationMessage = await store.GetLastMessage<CreateOrganization>();

            Assert.Equal("37018", createOrganizationMessage.Code);
            Assert.Equal("Gemeente Wingene", createOrganizationMessage.Name);
            Assert.Equal("OVO002277", createOrganizationMessage.OvoCode);
        }
    }
}
