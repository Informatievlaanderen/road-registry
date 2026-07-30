namespace RoadRegistry.Projections.Tests;

using System.Linq;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using Infrastructure.MartenDb.Setup;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadNetworkRepositoryTests
{
    [Fact]
    public async Task Save_PersistsLeafSnapshotsButOnlyEventsForTheScopedRoadNetwork()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var repository = new RoadNetworkRepository(store);
        var testData = new RoadNetworkTestDataV2();

        // A changed leaf aggregate is persisted as a snapshot document (in addition to its events).
        var segment = RoadSegment.Create(testData.Segment1Added);
        repository.Save(store, new ScopedRoadNetwork(new ScopedRoadNetworkId(Guid.NewGuid()), [], [segment], [], []), "test-command");

        // The scoped road network itself is persisted as events only: its summary event is appended to the stream, but
        // the aggregate is never stored as a snapshot document (it is rebuilt from its event stream on load instead).
        var networkId = new ScopedRoadNetworkId(Guid.NewGuid());
        var roadNetwork = new ScopedRoadNetwork(networkId);
        roadNetwork.Apply(new RoadNetworkWasChanged
        {
            RoadNetworkId = networkId,
            Summary = new RoadNetworkChangedSummary(new RoadNetworkChangesSummary()),
            Provenance = new ProvenanceData(testData.Provenance)
        });
        repository.Save(store, roadNetwork, "test-command");

        await store.SaveChangesAsync();

        store.AllRecords().Should().Contain(x => x is RoadSegment); // leaf snapshot document was stored
        store.AllRecords().Should().NotContain(x => x is ScopedRoadNetwork); // no snapshot document for the network
        store.AllEvents().Should().Contain(x => x is RoadNetworkWasChanged); // but the network's events were appended
    }


    [Fact]
    public void Save_WhenEventOrdinalsAreNotUnique_ThenThrows()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var repository = new RoadNetworkRepository(store);

        // Two aggregates whose RoadSegmentWasAdded were stamped by EventOrdinalProvider.None (ordinal 0), i.e. created
        // without a change's ordinal provider threaded through - the wiring bug the safeguard must catch.
        var testData = new RoadNetworkTestDataV2();
        var segment1 = RoadSegment.Create(testData.Segment1Added);
        var segment2 = RoadSegment.Create(testData.Segment1Added with
        {
            RoadSegmentId = new RoadSegmentId(testData.Segment1Added.RoadSegmentId.ToInt32() + 1)
        });
        var roadNetwork = new ScopedRoadNetwork(new ScopedRoadNetworkId(Guid.NewGuid()), [], [segment1, segment2], [], []);

        var act = () => repository.Save(store, roadNetwork, "test-command");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Duplicate event ordinal*");
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        return storeOptions;
    }
}
