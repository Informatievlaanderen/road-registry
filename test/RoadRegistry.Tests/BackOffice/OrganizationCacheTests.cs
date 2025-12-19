namespace RoadRegistry.Tests.BackOffice
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Editor.Schema;
    using Editor.Schema.Organizations;
    using Framework.Projections;
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Testing;
        using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Core;
    using RoadRegistry.BackOffice.Framework;
    using RoadRegistry.BackOffice.Messages;
    using RoadRegistry.Infrastructure;
    using SqlStreamStore;

    public class OrganizationCacheTests
    {
        private static readonly EventMapping Mapping = new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
        private static readonly JsonSerializerSettings Settings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly StreamNameConverter StreamNameConverter = StreamNameConversions.PassThru;

        [Theory]
        [InlineData("ABC", "DV")]
        [InlineData("-7", "andere")]
        [InlineData("-8", "niet gekend")]
        public async Task ShouldFindById(string organizationIdValue, string organizationNameValue)
        {
            var organizationId = new OrganizationId(organizationIdValue);
            var organizationName = new OrganizationName(organizationNameValue);

            var sut = await BuildOrganizationCache(
                configureStore: async store =>
                {
                    await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(organizationId), new ImportedOrganization
                    {
                        Code = organizationId,
                        Name = organizationName
                    });
                }
            );

            var organization = await sut.FindByIdOrOvoCodeOrKboNumberAsync(organizationId, CancellationToken.None);
            Assert.NotNull(organization);
            Assert.Equal(organizationId, organization.Code);
            Assert.Equal(organizationName, organization.Name);
        }

        [Theory]
        [InlineData("0123456789", "DV")]
        public async Task ShouldFindByKboNumber(string kboNumber, string organizationNameValue)
        {
            var organizationId = new OrganizationId("ABC");
            var organizationName = new OrganizationName(organizationNameValue);

            var sut = await BuildOrganizationCache(
                configureStore: async store =>
                {
                    await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(organizationId), new CreateOrganizationAccepted
                    {
                        Code = organizationId,
                        Name = organizationName,
                        KboNumber = kboNumber
                    });
                },
                configureEditorContext: async editorContext =>
                {
                    await editorContext.OrganizationsV2.AddAsync(new OrganizationRecordV2
                    {
                        Code = organizationId,
                        Name = organizationName,
                        KboNumber = kboNumber
                    });
                }
            );

            var organization = await sut.FindByIdOrOvoCodeOrKboNumberAsync(new OrganizationId(kboNumber), CancellationToken.None);
            Assert.NotNull(organization);
            Assert.Equal(organizationId, organization.Code);
            Assert.Equal(organizationName, organization.Name);
            Assert.Equal(kboNumber, organization.KboNumber);
        }

        [Fact]
        public async Task ShouldFindByOvoCodeAsId()
        {
            var organizationName = new OrganizationName("DV");
            var ovoCode = new OrganizationOvoCode(1);
            var ovoCodeAsOrganizationId = new OrganizationId(ovoCode);

            var sut = await BuildOrganizationCache(
                configureStore: async store =>
                {
                    await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(ovoCodeAsOrganizationId), new ImportedOrganization
                    {
                        Code = ovoCode,
                        Name = organizationName
                    });
                }
            );

            Assert.Equal(ovoCode.ToString(), ovoCodeAsOrganizationId.ToString());

            var organization = await sut.FindByIdOrOvoCodeOrKboNumberAsync(ovoCodeAsOrganizationId, CancellationToken.None);
            Assert.NotNull(organization);
            Assert.Equal(ovoCodeAsOrganizationId, organization.Code);
            Assert.Equal(organizationName, organization.Name);
        }

        [Fact]
        public async Task GivenOldIdWithOvoCodeMapping_WhenFindByOvoCode_ThenShouldFindOldId()
        {
            var organizationId = new OrganizationId("ABC");
            var organizationName = new OrganizationName("DV");
            var ovoCode = new OrganizationOvoCode(1);

            var sut = await BuildOrganizationCache(
                configureStore: async store =>
                {
                    await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(organizationId), new CreateOrganizationAccepted
                    {
                        Code = organizationId,
                        Name = organizationName,
                        OvoCode = ovoCode
                    });
                },
                configureEditorContext: async editorContext =>
                {
                    await editorContext.OrganizationsV2.AddRangeAsync(
                        new OrganizationRecordV2
                        {
                            Code = organizationId,
                            Name = organizationName,
                            OvoCode = ovoCode
                        }
                    );
                }
            );

            Assert.NotEqual(ovoCode.ToString(), organizationId.ToString());

            var organization = await sut.FindByIdOrOvoCodeOrKboNumberAsync(new OrganizationId(ovoCode), CancellationToken.None);
            Assert.NotNull(organization);
            Assert.Equal(organizationId, organization.Code);
            Assert.Equal(organizationName, organization.Name);
        }

        [Fact]
        public async Task ShouldNotFindById()
        {
            var organizationId = new OrganizationId("ABC");

            var sut = await BuildOrganizationCache();

            var organization = await sut.FindByIdOrOvoCodeOrKboNumberAsync(organizationId, CancellationToken.None);
            Assert.Null(organization);
        }

        private async Task<IOrganizationCache> BuildOrganizationCache(
            Func<IStreamStore, Task> configureStore = null,
            Func<EditorContext, Task> configureEditorContext = null)
        {
            var store = new InMemoryStreamStore();
            if (configureStore is not null)
            {
                await configureStore(store);
            }

            var editorContext = CreateEditorContext();
            if (configureEditorContext is not null)
            {
                await configureEditorContext(editorContext);
                await editorContext.SaveChangesAsync();
            }

            var roadRegistryContext = new RoadRegistryContext(
                new EventSourcedEntityMap(),
                store,
                new FakeRoadNetworkSnapshotReader(),
                Settings,
                Mapping,
                EnrichEvent.WithTime(new FakeClock(NodaConstants.UnixEpoch)),
                new NullLoggerFactory());

            return new OrganizationCache(
                editorContext,
                roadRegistryContext,
                new NullLogger<OrganizationCache>());
        }

        private static EditorContext CreateEditorContext()
        {
            var database = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<EditorContext>()
                .UseInMemoryDatabase(database)
                .EnableSensitiveDataLogging()
                .Options;

            return new MemoryEditorContext(options);
        }
    }
}
