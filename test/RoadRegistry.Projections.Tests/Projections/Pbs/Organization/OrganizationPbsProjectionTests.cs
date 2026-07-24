namespace RoadRegistry.Projections.Tests.Projections.Pbs.Organization;

using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema.Records;
using RoadRegistry.RoadNode.Events.V1;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;

public class OrganizationPbsProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private PbsProjectionScenario Scenario() =>
        new(new OrganizationPbsProjection());

    private ProvenanceData Provenance => new(_testData.Provenance);

    private OrganizationId OrganizationId => _testData.Fixture.Create<OrganizationId>();

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles only the Organization V2 events; everything else is excluded.
        var excludeEventTypes = new[]
        {
            // RoadNode V1
            typeof(ImportedRoadNode), typeof(RoadNodeAdded), typeof(RoadNodeModified), typeof(RoadNodeRemoved),
            // RoadNode V2
            typeof(RoadNodeWasAdded), typeof(RoadNodeTypeWasChanged), typeof(RoadNodeWasModified),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeWasRemoved), typeof(RoadNodeWasRemovedBecauseOfMigration),
            // RoadSegment V1
            typeof(ImportedRoadSegment), typeof(OutlinedRoadSegmentRemoved), typeof(RoadSegmentAdded),
            typeof(RoadSegmentAddedToEuropeanRoad), typeof(RoadSegmentAddedToNationalRoad), typeof(RoadSegmentAddedToNumberedRoad),
            typeof(RoadSegmentAttributesModified), typeof(RoadSegmentGeometryModified), typeof(RoadSegmentModified),
            typeof(RoadSegmentRemoved), typeof(RoadSegmentRemovedFromEuropeanRoad), typeof(RoadSegmentRemovedFromNationalRoad),
            typeof(RoadSegmentRemovedFromNumberedRoad), typeof(RoadSegmentStreetNamesChanged),
            // RoadSegment V2
            typeof(OutlinedRoadSegmentWasAdded), typeof(RoadSegmentGeometryWasModified), typeof(RoadSegmentStreetNameIdWasChanged),
            typeof(RoadSegmentWasAdded), typeof(RoadSegmentWasAddedToEuropeanRoad), typeof(RoadSegmentWasAddedToNationalRoad),
            typeof(RoadSegmentWasMerged), typeof(RoadSegmentWasMigrated), typeof(RoadSegmentWasModified),
            typeof(RoadSegmentWasRemoved), typeof(RoadSegmentWasRemovedBecauseOfMigration), typeof(RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadSegmentWasRemovedFromNationalRoad), typeof(RoadSegmentWasRetired), typeof(RoadSegmentWasRetiredBecauseOfMerger),
            typeof(RoadSegmentWasRetiredBecauseOfSplit), typeof(RoadSegmentWasSplit),
            // GradeJunction V2
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionWasModified), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
            // GradeSeparatedJunction V1
            typeof(ImportedGradeSeparatedJunction), typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified), typeof(GradeSeparatedJunctionRemoved),
            typeof(GradeSeparatedJunctionGeometryModified),
            // GradeSeparatedJunction V2
            typeof(GradeSeparatedJunctionWasAdded), typeof(GradeSeparatedJunctionWasModified), typeof(GradeSeparatedJunctionGeometryWasChanged),
            typeof(GradeSeparatedJunctionWasRemoved), typeof(GradeSeparatedJunctionWasRemovedBecauseOfMigration),
            // StreetName V2
            typeof(StreetNameWasCreated), typeof(StreetNameWasModified),
            typeof(StreetNameWasRemoved), typeof(StreetNameWasRenamed),
            // ScopedRoadNetwork
            typeof(RoadNetworkChangesAccepted), typeof(MunicipalityWasMigrated),
            typeof(RoadNetworkWasChangedBecauseOfExtract)
        };

        PbsProjectionEventCoverage.AssertHandledExactlyOnce(new OrganizationPbsProjection(), excludeEventTypes);
    }

    [Fact]
    public async Task WhenOrganizationWasCreated_ThenCachedButNotAMaintainer()
    {
        var scenario = Scenario();
        var organizationId = OrganizationId;

        await scenario.GivenAsync(new OrganizationWasCreated
        {
            OrganizationId = organizationId,
            Name = "Agentschap Wegen en Verkeer",
            OvoCode = "OVO000001",
            KboNumber = null,
            Provenance = Provenance
        });

        var cache = await scenario.Find<OrganizationCacheRecord>(organizationId.ToString());
        Assert.NotNull(cache);
        Assert.Equal("Agentschap Wegen en Verkeer", cache!.Naam);
        Assert.Equal("OVO000001", cache.OvoCode);
        Assert.False(cache.IsWegbeheerder);

        // The code list only holds maintainers, so nothing yet.
        Assert.Empty(await scenario.Query<RoadSegmentMaintenanceAuthorityCodeListRecord>());
    }

    [Fact]
    public async Task WhenOrganizationBecomesMaintainer_ThenAddedToCodeList()
    {
        var scenario = Scenario();
        var organizationId = OrganizationId;

        await scenario.GivenAsync(new OrganizationWasCreated
        {
            OrganizationId = organizationId,
            Name = "Agentschap Wegen en Verkeer",
            OvoCode = "OVO000001",
            KboNumber = null,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = organizationId,
            Name = null, // a null field leaves the cached value unchanged
            OvoCode = null,
            KboNumber = null,
            IsMaintainer = true,
            Provenance = Provenance
        });

        var cache = await scenario.Find<OrganizationCacheRecord>(organizationId.ToString());
        Assert.True(cache!.IsWegbeheerder);
        Assert.Equal("Agentschap Wegen en Verkeer", cache.Naam); // kept: the modify carried no name

        var codeList = await scenario.Find<RoadSegmentMaintenanceAuthorityCodeListRecord>(organizationId.ToString());
        Assert.NotNull(codeList);
        Assert.Equal("Agentschap Wegen en Verkeer", codeList!.LBLBEHEER);
        Assert.Equal("OVO000001", codeList.OVOCODE);
    }

    [Fact]
    public async Task WhenOrganizationStopsBeingMaintainer_ThenRemovedFromCodeList()
    {
        var scenario = Scenario();
        var organizationId = OrganizationId;

        await scenario.GivenAsync(new OrganizationWasCreated
        {
            OrganizationId = organizationId,
            Name = "Agentschap Wegen en Verkeer",
            OvoCode = "OVO000001",
            KboNumber = null,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = organizationId,
            IsMaintainer = true,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = organizationId,
            IsMaintainer = false,
            Provenance = Provenance
        });

        Assert.False((await scenario.Find<OrganizationCacheRecord>(organizationId.ToString()))!.IsWegbeheerder);
        Assert.Null(await scenario.Find<RoadSegmentMaintenanceAuthorityCodeListRecord>(organizationId.ToString()));
    }

    [Fact]
    public async Task WhenOrganizationWasImported_ThenCached()
    {
        var scenario = Scenario();
        var organizationId = OrganizationId;

        await scenario.GivenAsync(new OrganizationWasImported
        {
            OrganizationId = organizationId,
            Name = "Imported org",
            Provenance = Provenance
        });

        var cache = await scenario.Find<OrganizationCacheRecord>(organizationId.ToString());
        Assert.NotNull(cache);
        Assert.Equal("Imported org", cache!.Naam);
        Assert.False(cache.IsWegbeheerder);
    }

    [Fact]
    public async Task WhenOrganizationWasRemoved_ThenCacheAndCodeListCleared()
    {
        var scenario = Scenario();
        var organizationId = OrganizationId;

        await scenario.GivenAsync(new OrganizationWasCreated
        {
            OrganizationId = organizationId,
            Name = "Agentschap Wegen en Verkeer",
            OvoCode = "OVO000001",
            KboNumber = null,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = organizationId,
            IsMaintainer = true,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new OrganizationWasRemoved
        {
            OrganizationId = organizationId,
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<OrganizationCacheRecord>(organizationId.ToString()));
        Assert.Null(await scenario.Find<RoadSegmentMaintenanceAuthorityCodeListRecord>(organizationId.ToString()));
    }
}
