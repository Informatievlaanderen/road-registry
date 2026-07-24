namespace RoadRegistry.Projections.Tests.Projections.Pbs.RoadSegment;

using System.Linq;
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
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;

public class RoadSegmentPbsProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private PbsProjectionScenario Scenario() =>
        new(new RoadSegmentPbsProjection());

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles all RoadSegment V1 + V2 events; everything else is excluded.
        var excludeEventTypes = new[]
        {
            // RoadNode V1
            typeof(ImportedRoadNode), typeof(RoadNodeAdded), typeof(RoadNodeModified), typeof(RoadNodeRemoved),
            // RoadNode V2
            typeof(RoadNodeWasAdded), typeof(RoadNodeTypeWasChanged), typeof(RoadNodeWasModified),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeWasRemoved), typeof(RoadNodeWasRemovedBecauseOfMigration),
            // GradeJunction V2
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionWasModified), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
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
            // StreetName V2
            typeof(StreetNameWasCreated), typeof(StreetNameWasModified),
            typeof(StreetNameWasRemoved), typeof(StreetNameWasRenamed),
            // ScopedRoadNetwork
            typeof(RoadNetworkChangesAccepted), typeof(MunicipalityWasMigrated),
            typeof(RoadNetworkWasChangedBecauseOfExtract)
        };

        PbsProjectionEventCoverage.AssertHandledExactlyOnce(new RoadSegmentPbsProjection(), excludeEventTypes);
    }

    [Fact]
    public async Task WhenAnEventIsRedelivered_ThenItIsSkippedBecauseOfTheProjectionStatePosition()
    {
        var scenario = Scenario();

        // Apply the add: the segment is written and the projection-state position advances to its sequence.
        await scenario.GivenAsync(_testData.Segment1Added);

        // Simulate an out-of-band edit; if the re-delivered event were re-applied it would rebuild (overwrite) it.
        await scenario.SeedAsync(async context =>
        {
            var segment = await context.RoadSegments.FindAsync(1);
            segment!.LBLSTATUS = "edited-marker";
        });

        // The Marten daemon re-delivers the same batch (same sequences) after a partial failure.
        await scenario.RedeliverLastBatchAsync();

        // The handler was skipped (sequence <= projection-state position), so the edit survives instead of a rebuild.
        var segment = await scenario.Find<RoadSegmentRecord>(1);
        Assert.Equal("edited-marker", segment!.LBLSTATUS);
    }

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenSegmentStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);

        var segment = await scenario.Find<RoadSegmentRecord>(1);
        Assert.NotNull(segment);
        Assert.Equal(1, segment!.WS_OIDN);
        Assert.Equal(RoadSegmentStatusV2.Gerealiseerd.Translation.Identifier, segment.STATUS);
        Assert.Equal(1, segment.B_WK_OIDN); // start node
        Assert.Equal(2, segment.E_WK_OIDN); // end node
        Assert.NotNull(segment.GEOMETRIE);
        Assert.NotNull(segment.CREATIE);
        Assert.NotNull(segment.VERSIE);
    }

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenDynamicAttributeTablesPopulated()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);

        Assert.NotEmpty(await scenario.Query<RoadSegmentMorphologyAttributeRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.NotEmpty(await scenario.Query<RoadSegmentCategoryAttributeRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.NotEmpty(await scenario.Query<RoadSegmentAccessRestrictionAttributeRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.NotEmpty(await scenario.Query<RoadSegmentSurfaceTypeAttributeRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.NotEmpty(await scenario.Query<RoadSegmentMaintenanceAuthorityAttributeRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.NotEmpty(await scenario.Query<RoadSegmentStreetNameAttributeRecord>(q => q.Where(x => x.WS_OIDN == 1)));
    }

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenDerivedRowsBuilt()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);

        var derived = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.NotEmpty(derived);
        Assert.All(derived, row => Assert.NotNull(row.GEOMETRIE));
    }

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenEuropeanAndNationalRoadsStored()
    {
        var scenario = Scenario();

        // Segment1 carries exactly one European and one national road number.
        await scenario.GivenAsync(_testData.Segment1Added);

        Assert.Single(await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.Single(await scenario.Query<NationalRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
    }

    [Fact]
    public async Task WhenRoadSegmentWasModified_ThenStatusUpdatedAndGeometryKept()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);
        await scenario.GivenAsync(new RoadSegmentWasModified
        {
            RoadSegmentId = new RoadSegmentId(1),
            Status = RoadSegmentStatusV2.BuitenGebruik,
            Provenance = Provenance
        });

        var segment = await scenario.Find<RoadSegmentRecord>(1);
        Assert.Equal(RoadSegmentStatusV2.BuitenGebruik.Translation.Identifier, segment!.STATUS);
        Assert.NotNull(segment.GEOMETRIE); // geometry not carried on this event, so it is kept
    }

    [Fact]
    public async Task WhenRoadSegmentWasRemoved_ThenSegmentAndAllRelatedRowsDeleted()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);
        await scenario.GivenAsync(new RoadSegmentWasRemoved
        {
            RoadSegmentId = new RoadSegmentId(1),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<RoadSegmentRecord>(1));
        Assert.Empty(await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.Empty(await scenario.Query<RoadSegmentMorphologyAttributeRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.Empty(await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.Empty(await scenario.Query<NationalRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
    }

    [Fact]
    public async Task WhenRoadSegmentWasAddedToEuropeanRoad_ThenRelationStoredAndRemovable()
    {
        var scenario = Scenario();
        var number = _testData.Fixture.Create<EuropeanRoadNumber>();

        await scenario.GivenAsync(new RoadSegmentWasAddedToEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(5),
            Number = number,
            Provenance = Provenance
        });

        var rows = await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 5));
        Assert.Single(rows);
        Assert.Equal(number.ToString(), rows[0].EUNUMMER);

        await scenario.GivenAsync(new RoadSegmentWasRemovedFromEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(5),
            Number = number,
            Provenance = Provenance
        });

        Assert.Empty(await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 5)));
    }

    [Fact]
    public async Task WhenAddingSameEuropeanRoadTwice_ThenNotDuplicated()
    {
        var scenario = Scenario();
        var number = _testData.Fixture.Create<EuropeanRoadNumber>();

        await scenario.GivenAsync(new RoadSegmentWasAddedToEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(5),
            Number = number,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new RoadSegmentWasAddedToEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(5),
            Number = number,
            Provenance = Provenance
        });

        Assert.Single(await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 5)));
    }
}
