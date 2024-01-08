namespace RoadRegistry.Tests.BackOffice
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Editor.Schema;
    using Editor.Schema.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.IO;
    using Newtonsoft.Json;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Core;
    using RoadRegistry.BackOffice.Extracts.Dbase.Organizations;
    using RoadRegistry.BackOffice.FeatureToggles;
    using RoadRegistry.BackOffice.Framework;
    using RoadRegistry.BackOffice.Messages;
    using RoadRegistry.Hosts;
    using RoadRegistry.Tests.Framework.Projections;
    using SqlStreamStore;

    public class OrganizationRepositoryTests
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

            var sut = await BuildOrganizationRepository(
                configureStore: async store =>
                {
                    await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(organizationId), new ImportedOrganization
                    {
                        Code = organizationId,
                        Name = organizationName
                    });
                }
            );

            var organization = await sut.FindByIdOrOvoCodeAsync(organizationId, CancellationToken.None);
            Assert.NotNull(organization);
            Assert.Equal(organizationId, organization.Code);
            Assert.Equal(organizationName, organization.Name);
        }

        [Fact]
        public async Task ShouldFindByOvoCodeAsIdWithFeatureFlagEnabled()
        {
            var organizationName = new OrganizationName("DV");
            var ovoCode = new OrganizationOvoCode(1);
            var ovoCodeAsOrganizationId = new OrganizationId(ovoCode);

            var sut = await BuildOrganizationRepository(
                configureStore: async store =>
                {
                    await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(ovoCodeAsOrganizationId), new ImportedOrganization
                    {
                        Code = ovoCode,
                        Name = organizationName
                    });
                },
                useOvoCodeInChangeRoadNetworkFeatureToggle: true
            );

            Assert.Equal(ovoCode.ToString(), ovoCodeAsOrganizationId.ToString());

            var organization = await sut.FindByIdOrOvoCodeAsync(ovoCodeAsOrganizationId, CancellationToken.None);
            Assert.NotNull(organization);
            Assert.Equal(ovoCodeAsOrganizationId, organization.Code);
            Assert.Equal(organizationName, organization.Name);
        }

        [Fact]
        public async Task ShouldNotFindByOvoCodeAsIdWithFeatureFlagDisabled()
        {
            var organizationName = new OrganizationName("DV");
            var ovoCode = new OrganizationOvoCode(1);
            var ovoCodeAsOrganizationId = new OrganizationId(ovoCode);

            var sut = await BuildOrganizationRepository(
                configureStore: async store =>
                {
                    await store.Given(Mapping, Settings, StreamNameConverter, Organizations.ToStreamName(ovoCodeAsOrganizationId), new ImportedOrganization
                    {
                        Code = ovoCode,
                        Name = organizationName
                    });
                },
                useOvoCodeInChangeRoadNetworkFeatureToggle: false
            );

            Assert.Equal(ovoCode.ToString(), ovoCodeAsOrganizationId.ToString());

            var organization = await sut.FindByIdOrOvoCodeAsync(ovoCodeAsOrganizationId, CancellationToken.None);
            Assert.Null(organization);
        }

        [Fact]
        public async Task ShouldFindByOvoCodeAsMappedFromOldId()
        {
            var organizationId = new OrganizationId("ABC");
            var organizationName = new OrganizationName("DV");
            var ovoCode = new OrganizationOvoCode(1);

            var sut = await BuildOrganizationRepository(
                configureEditorContext: async editorContext =>
                {
                    await editorContext.Organizations.AddRangeAsync(
                        new OrganizationRecord
                        {
                            Code = organizationId,
                            SortableCode = organizationId,
                            DbaseSchemaVersion = RoadRegistry.BackOffice.Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord.DbaseSchemaVersion,
                            DbaseRecord = new RoadRegistry.BackOffice.Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord
                            {
                                ORG = { Value = organizationId },
                                LBLORG = { Value = organizationName },
                                OVOCODE = { Value = ovoCode }
                            }.ToBytes(new RecyclableMemoryStreamManager(), FileEncoding.UTF8)
                        }
                    );

                    await editorContext.SaveChangesAsync(CancellationToken.None);
                }
            );

            Assert.NotEqual(ovoCode.ToString(), organizationId.ToString());

            var organization = await sut.FindByIdOrOvoCodeAsync(new OrganizationId(ovoCode), CancellationToken.None);
            Assert.NotNull(organization);
            Assert.Equal(organizationId, organization.Code);
            Assert.Equal(organizationName, organization.Name);
        }

        [Fact]
        public async Task ShouldNotFindById()
        {
            var organizationId = new OrganizationId("ABC");

            var sut = await BuildOrganizationRepository();

            var organization = await sut.FindByIdOrOvoCodeAsync(organizationId, CancellationToken.None);
            Assert.Null(organization);
        }

        private async Task<IOrganizationRepository> BuildOrganizationRepository(
            Func<IStreamStore, Task> configureStore = null,
            Func<EditorContext, Task> configureEditorContext = null,
            bool useOvoCodeInChangeRoadNetworkFeatureToggle = false)
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
            }

            var roadRegistryContext = new RoadRegistryContext(
                new EventSourcedEntityMap(),
                store,
                new FakeRoadNetworkSnapshotReader(),
                Settings,
                Mapping,
                new NullLoggerFactory());

            return new OrganizationRepository(
                editorContext,
                new RecyclableMemoryStreamManager(),
                FileEncoding.UTF8,
                new UseOvoCodeInChangeRoadNetworkFeatureToggle(useOvoCodeInChangeRoadNetworkFeatureToggle),
                roadRegistryContext,
                new NullLogger<OrganizationRepository>());
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
