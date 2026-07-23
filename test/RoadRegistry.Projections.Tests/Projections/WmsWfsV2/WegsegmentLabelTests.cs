namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2;

using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema.Records;

// Covers the denormalized LSTRNM / RSTRNM / STRNM / LBLBEHEER label columns on AfgeleideWegsegmenten: they are resolved
// from the caches when a segment is derived, and refreshed on the affected derived rows when a street name or
// organization is renamed/removed.
public class WegsegmentLabelTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public async Task WhenSegmentIsDerived_ThenStreetNameLabelIsResolvedFromCache()
    {
        var scenario = new WmsWfsV2ProjectionScenario(new RoadSegmentWmsWfsV2Projection());

        await scenario.GivenAsync(_testData.Segment1Added);

        var before = (await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1))).First();
        Assert.NotNull(before.LSTRNMID);
        Assert.Null(before.LSTRNM); // no street name cached yet, so the label resolves to null

        var streetNameId = before.LSTRNMID!.Value;

        // Populate the cache, then re-derive the segment via a modify: the label is now resolved from the cache.
        await scenario.SeedAsync(context =>
        {
            context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = streetNameId, Naam = "Dorpsstraat" });
            return Task.CompletedTask;
        });
        await scenario.GivenAsync(new RoadSegmentWasModified
        {
            RoadSegmentId = new RoadSegmentId(1),
            Status = RoadSegmentStatusV2.Gerealiseerd,
            Provenance = Provenance
        });

        var after = (await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1))).First();
        Assert.Equal("Dorpsstraat", after.LSTRNM);
        Assert.Equal("Dorpsstraat", after.STRNM); // left == right street name collapses to a single label
    }

    [Fact]
    public async Task WhenStreetNameWasModified_ThenDerivedStreetNameLabelsAreRefreshed()
    {
        var scenario = new WmsWfsV2ProjectionScenario(new StreetNameWmsWfsV2Projection());

        await scenario.SeedAsync(context =>
        {
            context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = 100, Naam = "Kerkstraat" });
            context.DerivedRoadSegments.Add(new DerivedRoadSegmentRecord
            {
                WS_OIDN = 1,
                LSTRNMID = 100,
                RSTRNMID = 100,
                LSTRNM = "Kerkstraat",
                RSTRNM = "Kerkstraat",
                STRNM = "Kerkstraat"
            });
            return Task.CompletedTask;
        });

        await scenario.GivenAsync(new StreetNameWasModified
        {
            StreetNameId = new StreetNameLocalId(100),
            DutchName = "Nieuwstraat",
            NisCode = null,
            Status = null,
            Provenance = Provenance
        });

        var derived = (await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1))).Single();
        Assert.Equal("Nieuwstraat", derived.LSTRNM);
        Assert.Equal("Nieuwstraat", derived.RSTRNM);
        Assert.Equal("Nieuwstraat", derived.STRNM);
    }

    [Fact]
    public async Task WhenStreetNameWasRemoved_ThenDerivedStreetNameLabelsAreLeftUntouched()
    {
        // Removal does not cascade to the derived labels: the affected segments each get their own
        // RoadSegmentStreetNameIdWasChanged event, which re-derives them.
        var scenario = new WmsWfsV2ProjectionScenario(new StreetNameWmsWfsV2Projection());

        await scenario.SeedAsync(context =>
        {
            context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = 100, Naam = "Kerkstraat" });
            context.DerivedRoadSegments.Add(new DerivedRoadSegmentRecord
            {
                WS_OIDN = 1,
                LSTRNMID = 100,
                RSTRNMID = 100,
                LSTRNM = "Kerkstraat",
                RSTRNM = "Kerkstraat",
                STRNM = "Kerkstraat"
            });
            return Task.CompletedTask;
        });

        await scenario.GivenAsync(new StreetNameWasRemoved
        {
            StreetNameId = new StreetNameLocalId(100),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<StreetNameCacheRecord>(100)); // the cache entry is dropped
        var derived = (await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 1))).Single();
        Assert.Equal("Kerkstraat", derived.LSTRNM); // labels remain until the segment is re-derived
        Assert.Equal("Kerkstraat", derived.STRNM);
    }

    [Fact]
    public async Task WhenOrganizationWasRenamedToMunicipality_ThenDerivedBeheerLabelIsRefreshed()
    {
        var scenario = new WmsWfsV2ProjectionScenario(new OrganizationWmsWfsV2Projection());

        var organizationId = _testData.Fixture.Create<OrganizationId>();
        var code = organizationId.ToString();

        await scenario.SeedAsync(context =>
        {
            context.OrganizationCache.Add(new OrganizationCacheRecord { OrganisatieId = code, Naam = "Andere BV" });
            context.DerivedRoadSegments.Add(new DerivedRoadSegmentRecord
            {
                WS_OIDN = 2,
                STATUS = 11, // in use
                LBEHEER = code,
                RBEHEER = code,
                LBLBEHEER = "Andere"
            });
            return Task.CompletedTask;
        });

        await scenario.GivenAsync(new OrganizationWasModified
        {
            OrganizationId = organizationId,
            Name = "Gemeente Test",
            OvoCode = null,
            KboNumber = null,
            IsMaintainer = null,
            Provenance = Provenance
        });

        var derived = (await scenario.Query<DerivedRoadSegmentRecord>(q => q.Where(x => x.WS_OIDN == 2))).Single();
        Assert.Equal("Gemeente Test", derived.LBLLBEHEER); // stored maintainer names, so the view needs no join
        Assert.Equal("Gemeente Test", derived.LBLRBEHEER);
        Assert.Equal("Steden en gemeenten", derived.LBLBEHEER);
    }
}
