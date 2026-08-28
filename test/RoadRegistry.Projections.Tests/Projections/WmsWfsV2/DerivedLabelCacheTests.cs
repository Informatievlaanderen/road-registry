namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema;
using RoadRegistry.WmsWfsV2.Schema.Records;

// The cache is only consulted once loaded, and everything that writes the two label tables writes here too. These cover
// that contract: an unloaded cache stays unloaded whatever is written to it - so a partially filled one can never be
// mistaken for a complete one - and a loaded one answers from memory.
public class DerivedLabelCacheTests
{
    private static WmsWfsV2Context CreateContext()
    {
        return new WmsWfsV2Context(new DbContextOptionsBuilder<WmsWfsV2Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .Options);
    }

    private static async Task<DerivedLabelCache> LoadedAsync(WmsWfsV2Context context)
    {
        var cache = new DerivedLabelCache();
        await cache.LoadAsync(context, CancellationToken.None);
        return cache;
    }

    [Fact]
    public void ANewCacheIsNotLoaded()
    {
        new DerivedLabelCache().IsLoaded.Should().BeFalse();
    }

    [Fact]
    public void WritingToAnUnloadedCacheDoesNotLoadIt()
    {
        var cache = new DerivedLabelCache();

        cache.SetStreetName(1, "Kerkstraat");
        cache.SetOrganization("AWV", "Agentschap Wegen en Verkeer");
        cache.RemoveStreetName(2);
        cache.RemoveOrganization("MOW");

        cache.IsLoaded.Should().BeFalse();
    }

    [Fact]
    public async Task LoadingSeedsFromTheDatabase()
    {
        await using var context = CreateContext();
        context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = 1, Naam = "Kerkstraat" });
        context.OrganizationCache.Add(new OrganizationCacheRecord { OrganisatieId = "AWV", Naam = "Agentschap Wegen en Verkeer" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var cache = await LoadedAsync(context);

        cache.IsLoaded.Should().BeTrue();
        cache.GetStreetNames([1])[1].Should().Be("Kerkstraat");
        cache.GetOrganizations(["AWV"])["AWV"].Should().Be("Agentschap Wegen en Verkeer");
    }

    // A street name created earlier in the same batch is not committed yet. Seeding from the database alone would
    // resolve its label to null, which is exactly the case the change-tracker overlay exists for.
    [Fact]
    public async Task LoadingOverlaysWhatTheCurrentBatchHasNotCommittedYet()
    {
        await using var context = CreateContext();
        context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = 1, Naam = "Kerkstraat" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Pending, uncommitted work of the batch in flight.
        context.StreetNameCache.Add(new StreetNameCacheRecord { StraatnaamId = 2, Naam = "Dorpsstraat" });

        var cache = await LoadedAsync(context);

        cache.GetStreetNames([1, 2])[2].Should().Be("Dorpsstraat");
    }

    [Fact]
    public async Task UnknownIdsResolveToNullRatherThanThrowing()
    {
        await using var context = CreateContext();
        var cache = await LoadedAsync(context);

        cache.GetStreetNames([999]).Should().ContainKey(999).WhoseValue.Should().BeNull();
        cache.GetOrganizations(["NOPE"]).Should().ContainKey("NOPE").WhoseValue.Should().BeNull();
    }

    [Fact]
    public async Task WriteThroughIsVisibleToTheNextLookup()
    {
        await using var context = CreateContext();
        var cache = await LoadedAsync(context);

        cache.SetStreetName(1, "Kerkstraat");
        cache.SetOrganization("AWV", "Agentschap Wegen en Verkeer");

        cache.GetStreetNames([1])[1].Should().Be("Kerkstraat");
        cache.GetOrganizations(["AWV"])["AWV"].Should().Be("Agentschap Wegen en Verkeer");

        cache.SetStreetName(1, "Kerkstraat West");
        cache.GetStreetNames([1])[1].Should().Be("Kerkstraat West");
    }

    [Fact]
    public async Task RemovedEntriesResolveToNullAgain()
    {
        await using var context = CreateContext();
        var cache = await LoadedAsync(context);
        cache.SetStreetName(1, "Kerkstraat");
        cache.SetOrganization("AWV", "Agentschap Wegen en Verkeer");

        cache.RemoveStreetName(1);
        cache.RemoveOrganization("AWV");

        cache.GetStreetNames([1])[1].Should().BeNull();
        cache.GetOrganizations(["AWV"])["AWV"].Should().BeNull();
    }

    [Fact]
    public async Task ClearingSendsLookupsBackToTheDatabase()
    {
        await using var context = CreateContext();
        var cache = await LoadedAsync(context);
        cache.SetStreetName(1, "Kerkstraat");

        cache.Clear();

        cache.IsLoaded.Should().BeFalse();
    }
}
