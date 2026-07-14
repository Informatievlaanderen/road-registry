namespace RoadRegistry.Projections.Tests.Projections.Pbs.StreetName;

using System.Threading.Tasks;
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

public class StreetNamePbsProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private PbsProjectionScenario Scenario() =>
        new(factory => new[] { new StreetNamePbsProjection(factory) });

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles only the StreetName V2 events; everything else is excluded.
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
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
            // GradeSeparatedJunction V1
            typeof(ImportedGradeSeparatedJunction), typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified), typeof(GradeSeparatedJunctionRemoved),
            typeof(GradeSeparatedJunctionGeometryModified),
            // GradeSeparatedJunction V2
            typeof(GradeSeparatedJunctionWasAdded), typeof(GradeSeparatedJunctionWasModified), typeof(GradeSeparatedJunctionGeometryWasChanged),
            typeof(GradeSeparatedJunctionWasRemoved), typeof(GradeSeparatedJunctionWasRemovedBecauseOfMigration),
            // Organization V2
            typeof(OrganizationWasImported), typeof(OrganizationWasCreated),
            typeof(OrganizationWasModified), typeof(OrganizationWasRemoved),
            // ScopedRoadNetwork
            typeof(RoadNetworkChangesAccepted), typeof(MunicipalityWasMigrated),
            typeof(RoadNetworkWasChangedBecauseOfExtract)
        };

        PbsProjectionEventCoverage.AssertHandledExactlyOnce(new StreetNamePbsProjection(null!), excludeEventTypes);
    }

    [Fact]
    public async Task WhenStreetNameWasCreated_ThenCached()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = new StreetNameLocalId(1),
            DutchName = "Kerkstraat",
            Provenance = Provenance
        });

        var record = await scenario.Find<StreetNameCacheRecord>(1);
        Assert.NotNull(record);
        Assert.Equal("Kerkstraat", record!.Naam);
    }

    [Fact]
    public async Task WhenStreetNameWasModified_ThenNameUpdated()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = new StreetNameLocalId(1),
            DutchName = "Kerkstraat",
            Provenance = Provenance
        });
        await scenario.GivenAsync(new StreetNameWasModified
        {
            StreetNameId = new StreetNameLocalId(1),
            DutchName = "Nieuwstraat",
            NisCode = null,
            Status = null,
            Provenance = Provenance
        });

        var record = await scenario.Find<StreetNameCacheRecord>(1);
        Assert.Equal("Nieuwstraat", record!.Naam);
    }

    [Fact]
    public async Task WhenStreetNameWasRemoved_ThenDeleted()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = new StreetNameLocalId(1),
            DutchName = "Kerkstraat",
            Provenance = Provenance
        });
        await scenario.GivenAsync(new StreetNameWasRemoved
        {
            StreetNameId = new StreetNameLocalId(1),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<StreetNameCacheRecord>(1));
    }

    [Fact]
    public async Task WhenStreetNameWasRenamed_ThenStaleCacheEntryDropped()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new StreetNameWasCreated
        {
            StreetNameId = new StreetNameLocalId(1),
            DutchName = "Kerkstraat",
            Provenance = Provenance
        });
        await scenario.GivenAsync(new StreetNameWasRenamed
        {
            StreetNameId = new StreetNameLocalId(1),
            DestinationStreetNameId = new StreetNameLocalId(2),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<StreetNameCacheRecord>(1));
    }
}
