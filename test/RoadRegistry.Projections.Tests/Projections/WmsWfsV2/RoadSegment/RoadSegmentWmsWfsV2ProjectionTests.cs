namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2.RoadSegment;

using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Projections.Tests.Projections;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema.Records;
using RoadRegistry.RoadNode.Events.V1;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;

public class RoadSegmentWmsWfsV2ProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private WmsWfsV2ProjectionScenario Scenario() =>
        new(new RoadSegmentWmsWfsV2Projection());

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
            typeof(RoadNetworkChangesAccepted),
            typeof(RoadNetworkWasChanged)
        };

        WmsWfsV2ProjectionEventCoverage.AssertHandledExactlyOnce(new RoadSegmentWmsWfsV2Projection(), excludeEventTypes);
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
        Assert.NotEqual(default, segment.CREATIE);
        Assert.NotEqual(default, segment.VERSIE);
    }

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenGeometriesAreStoredAs2D()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);

        // The WMS/WFS target database must hold plain 2D geometries (no Z, no M), whatever the incoming geometry
        // carries.
        var segment = await scenario.Find<RoadSegmentRecord>(1);
        SqlServerGeometry.AssertIs2D(segment!.GEOMETRIE!);

        var derived = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.NotEmpty(derived);
        Assert.All(derived, row => SqlServerGeometry.AssertIs2D(row.GEOMETRIE!));
    }

    [Fact]
    public async Task WhenRoadSegmentIsRederivedFromItsStoredGeometry_ThenGeometriesAreStillStoredAs2D()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);

        // Every event after the add re-derives the flattened rows from the segment geometry as it comes back out of
        // the database, and EF hands that back with a Z and an M ordinate declared (see SqlServerGeometry). The
        // in-memory provider returns the instance that was written, so stand in for the round trip here.
        await scenario.SeedAsync(async context =>
        {
            var stored = await context.RoadSegments.FindAsync(1);
            stored!.GEOMETRIE = SqlServerGeometry.AsReadFromSqlServer(stored.GEOMETRIE!);
            // The replacement holds the same X/Y, so EF's geometry comparer sees no change and would drop it.
            context.Entry(stored).Property(x => x.GEOMETRIE).IsModified = true;
        });

        await scenario.GivenAsync(new RoadSegmentWasModified
        {
            RoadSegmentId = new RoadSegmentId(1),
            Status = RoadSegmentStatusV2.BuitenGebruik,
            Provenance = Provenance
        });

        // The segment's own column is not asserted here: it is only ever written from an event geometry, and this
        // event carries none, so EF leaves the stored value alone. The flattened rows are rewritten from it every time.
        var derived = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.NotEmpty(derived);
        Assert.All(derived, row => SqlServerGeometry.AssertIs2D(row.GEOMETRIE!));
    }

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenDynamicAttributesBlobPopulated()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);

        var segment = await scenario.Find<RoadSegmentRecord>(1);
        Assert.NotNull(segment);
        var attrs = segment!.DynamicAttributes;
        Assert.NotEmpty(attrs.Morphology);
        Assert.NotEmpty(attrs.Category);
        Assert.NotEmpty(attrs.AccessRestriction);
        Assert.NotEmpty(attrs.SurfaceType);
        Assert.NotEmpty(attrs.MaintenanceAuthority);
        Assert.NotEmpty(attrs.StreetName);
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
        Assert.Empty(await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.Empty(await scenario.Query<NationalRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
    }

    // The projection applies a whole batch of events to one DbContext and saves once at the end, so anything written
    // earlier in the batch is still only in the change tracker. A lookup that queries the database will not see it.
    // Every test above splits its events over separate batches, which is why that never showed.

    [Fact]
    public async Task WhenRoadSegmentIsAddedAndRemovedInTheSameBatch_ThenTheDerivedRowsAreDeletedToo()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(
            _testData.Segment1Added,
            new RoadSegmentWasRemoved
            {
                RoadSegmentId = new RoadSegmentId(1),
                Provenance = Provenance
            });

        Assert.Null(await scenario.Find<RoadSegmentRecord>(1));
        Assert.Empty(await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.Empty(await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
        Assert.Empty(await scenario.Query<NationalRoadRecord>(q => q.Where(x => x.WS_OIDN == 1)));
    }

    [Fact]
    public async Task WhenRoadSegmentIsAddedAndModifiedInTheSameBatch_ThenTheDerivedRowsKeepTheAttributesTheEventDoesNotCarry()
    {
        var scenario = Scenario();

        // The modify event carries only the status, so every other attribute has to be kept as stored - including the
        // ones written by the add event earlier in this same batch.
        await scenario.GivenAsync(
            _testData.Segment1Added,
            new RoadSegmentWasModified
            {
                RoadSegmentId = new RoadSegmentId(1),
                Status = RoadSegmentStatusV2.BuitenGebruik,
                Provenance = Provenance
            });

        var derived = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.NotEmpty(derived);
        Assert.All(derived, row =>
        {
            Assert.NotNull(row.WEGCAT);
            Assert.NotNull(row.LBLWEGCAT);
            Assert.NotNull(row.MORF);
            Assert.NotNull(row.LBEHEER);
        });
    }

    [Fact]
    public async Task WhenRoadSegmentIsAddedAndModifiedInTheSameBatch_ThenTheDerivedRowsAreReplacedRatherThanAppended()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);
        var afterAdd = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));

        var other = Scenario();
        await other.GivenAsync(
            _testData.Segment1Added,
            new RoadSegmentWasModified
            {
                RoadSegmentId = new RoadSegmentId(1),
                Status = RoadSegmentStatusV2.BuitenGebruik,
                Provenance = Provenance
            });

        // A rebuild replaces the derived rows. WS_TEMPID is an identity column, so a delete that misses the rows added
        // earlier in the batch appends a second set instead of failing on the key.
        var afterModify = await other.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.Equal(afterAdd.Count, afterModify.Count);
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

    [Fact]
    public async Task WhenRoadSegmentWasAdded_ThenDerivedRowsCarryRoadNumbers()
    {
        var scenario = Scenario();

        // Segment1 carries exactly one European and one national road number.
        await scenario.GivenAsync(_testData.Segment1Added);

        var derived = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.NotEmpty(derived);
        Assert.All(derived, row => Assert.False(string.IsNullOrEmpty(row.EUNUMMERS)));
        Assert.All(derived, row => Assert.False(string.IsNullOrEmpty(row.NWNUMMERS)));
    }

    [Fact]
    public async Task WhenSegmentHasMultipleEuropeanRoads_ThenDerivedEunummersIsSortedDistinctAggregate()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1Added);
        // Add two more European roads to the same segment through the stand-alone events.
        await scenario.GivenAsync(new RoadSegmentWasAddedToEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(1),
            Number = _testData.Fixture.Create<EuropeanRoadNumber>(),
            Provenance = Provenance
        });
        await scenario.GivenAsync(new RoadSegmentWasAddedToEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(1),
            Number = _testData.Fixture.Create<EuropeanRoadNumber>(),
            Provenance = Provenance
        });

        // The derived rows must carry the sorted, distinct " / "-joined aggregate of the segment's EuropeanRoads.
        var expected = ExpectedAggregate((await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 1))).Select(x => x.EUNUMMER));

        var derived = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.NotEmpty(derived);
        Assert.All(derived, row => Assert.Equal(expected, row.EUNUMMERS));
    }

    [Fact]
    public async Task WhenEuropeanRoadRemoved_ThenDerivedEunummersUpdated()
    {
        var scenario = Scenario();
        var number = _testData.Fixture.Create<EuropeanRoadNumber>();

        await scenario.GivenAsync(_testData.Segment1Added);
        await scenario.GivenAsync(new RoadSegmentWasAddedToEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(1),
            Number = number,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new RoadSegmentWasRemovedFromEuropeanRoad
        {
            RoadSegmentId = new RoadSegmentId(1),
            Number = number,
            Provenance = Provenance
        });

        var expected = ExpectedAggregate((await scenario.Query<EuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == 1))).Select(x => x.EUNUMMER));

        var derived = await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1));
        Assert.All(derived, row => Assert.Equal(expected, row.EUNUMMERS));
    }

    // Mirrors the projection's aggregation: distinct, alphabetically sorted, " / "-joined, null when empty.
    private static string? ExpectedAggregate(System.Collections.Generic.IEnumerable<string?> numbers)
    {
        var list = numbers.Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x, System.StringComparer.Ordinal).ToList();
        return list.Count > 0 ? string.Join(" / ", list) : null;
    }
}
