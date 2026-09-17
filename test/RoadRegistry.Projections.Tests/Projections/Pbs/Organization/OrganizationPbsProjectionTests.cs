namespace RoadRegistry.Projections.Tests.Projections.Pbs.Organization;

using System.Collections.Generic;
using System.Threading;
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
            typeof(global::RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange),
            typeof(global::RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasChangedToGradeJunction),
            typeof(global::RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasChangedToGradeSeparatedJunction),
            typeof(global::RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange),
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
            typeof(OutlinedRoadSegmentWasAdded), typeof(RoadSegmentGeometryWasModified), typeof(RoadSegmentStreetNameIdWasChanged), typeof(RoadSegmentGeometryDrawMethodWasChanged),
            typeof(RoadSegmentWasAdded), typeof(RoadSegmentWasAddedToEuropeanRoad), typeof(RoadSegmentWasAddedToNationalRoad),
            typeof(RoadSegmentWasMerged), typeof(RoadSegmentWasMigrated), typeof(RoadSegmentWasModified),
            typeof(RoadSegmentWasRealizedFromPlanned),
            typeof(RoadSegmentWasRealizedFromOutOfUse),
            typeof(RoadSegmentWasCorrectedFromHistorizedToRealized),
            typeof(RoadSegmentWasCorrectedFromRealizedToPlanned),
            typeof(RoadSegmentWasTakenOutOfUseFromRealized),
            typeof(RoadSegmentWasHistorizedFromRealized),
            typeof(RoadSegmentWasHistorizedFromOutOfUse),
            typeof(RoadSegmentWasCorrectedFromNotRealizedToPlanned),
            typeof(RoadSegmentWasCorrectedFromHistorizedToOutOfUse),
            typeof(RoadSegmentWasNotRealizedFromPlanned),
            typeof(RoadSegmentWasRemoved), typeof(RoadSegmentWasRemovedBecauseOfMigration), typeof(RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadSegmentWasRemovedFromNationalRoad), typeof(RoadSegmentWasRetiredBecauseOfMerger),
            typeof(RoadSegmentWasRetiredBecauseOfSplit), typeof(RoadSegmentWasSplit),
            // GradeJunction V2
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionWasModified), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
            // GradeSeparatedJunction V1
            typeof(ImportedGradeSeparatedJunction), typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified), typeof(GradeSeparatedJunctionRemoved),
            typeof(GradeSeparatedJunctionGeometryModified),
            // GradeSeparatedJunction V2
            typeof(GradeSeparatedJunctionWasAdded), typeof(GradeSeparatedJunctionWasModified), typeof(GradeSeparatedJunctionWasMigrated), typeof(GradeSeparatedJunctionGeometryWasChanged),
            typeof(GradeSeparatedJunctionWasRemoved), typeof(GradeSeparatedJunctionWasRemovedBecauseOfMigration),
            // StreetName V2
            typeof(StreetNameWasCreated), typeof(StreetNameWasModified),
            typeof(StreetNameWasRemoved), typeof(StreetNameWasRenamed),
            // ScopedRoadNetwork
            typeof(RoadNetworkChangesAccepted),
            typeof(RoadNetworkWasChanged)
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

        // The code list only holds maintainers, so nothing yet besides the predefined "andere" and "niet gekend".
        Assert.DoesNotContain(await scenario.Query<RoadSegmentMaintenanceAuthorityCodeListRecord>(), x => x.BEHEER == organizationId.ToString());
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

    [Fact]
    public async Task WhenTheProjectionStartsFromNothing_ThenAndereAndNietGekendAreInTheCodeList()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new OrganizationWasImported
        {
            OrganizationId = OrganizationId,
            Name = "Imported org",
            Provenance = Provenance
        });

        AssertPredefinedMaintenanceAuthorities(await scenario.Query<RoadSegmentMaintenanceAuthorityCodeListRecord>());
    }

    // For a read model projected before these rows existed: the startup sync adds them, and corrects them if they differ.
    [Fact]
    public async Task WhenPredefinedMaintenanceAuthoritiesAreSynced_ThenAndereAndNietGekendAreInTheCodeList()
    {
        var scenario = Scenario();
        await scenario.SeedAsync(context =>
        {
            context.RoadSegmentMaintenanceAuthorityCodeList.Add(new RoadSegmentMaintenanceAuthorityCodeListRecord
            {
                BEHEER = "-8",
                LBLBEHEER = "outdated",
                OVOCODE = "OVO000001"
            });
            return Task.CompletedTask;
        });

        await scenario.SeedAsync(context => PbsPredefinedMaintenanceAuthorities.SyncAsync(context, CancellationToken.None));

        AssertPredefinedMaintenanceAuthorities(await scenario.Query<RoadSegmentMaintenanceAuthorityCodeListRecord>());
    }

    // "andere" and "niet gekend" are seeded, not projected: an organization event carrying their id leaves the code
    // list as it is.
    [Theory]
    [InlineData("-7", "andere")]
    [InlineData("-8", "niet gekend")]
    public async Task WhenPredefinedMaintenanceAuthorityIsModifiedOrRemoved_ThenCodeListIsLeftAlone(string organisatieId, string label)
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new OrganizationWasImported
        {
            OrganizationId = new OrganizationId(organisatieId),
            Name = "Imported org",
            Provenance = Provenance
        });
        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = new OrganizationId(organisatieId),
            IsMaintainer = false,
            Provenance = Provenance
        });

        var codeList = await scenario.Find<RoadSegmentMaintenanceAuthorityCodeListRecord>(organisatieId);
        Assert.NotNull(codeList);
        Assert.Equal(label, codeList!.LBLBEHEER);

        await scenario.GivenAsync(new OrganizationWasRemoved
        {
            OrganizationId = new OrganizationId(organisatieId),
            Provenance = Provenance
        });

        Assert.NotNull(await scenario.Find<RoadSegmentMaintenanceAuthorityCodeListRecord>(organisatieId));
    }

    private static void AssertPredefinedMaintenanceAuthorities(IReadOnlyCollection<RoadSegmentMaintenanceAuthorityCodeListRecord> codeList)
    {
        Assert.Equal(2, codeList.Count);

        var other = Assert.Single(codeList, x => x.BEHEER == "-7");
        Assert.Equal("andere", other.LBLBEHEER);
        Assert.Null(other.OVOCODE);

        var unknown = Assert.Single(codeList, x => x.BEHEER == "-8");
        Assert.Equal("niet gekend", unknown.LBLBEHEER);
        Assert.Null(unknown.OVOCODE);
    }
}
