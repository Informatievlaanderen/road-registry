namespace RoadRegistry.Projections.Tests;

using FluentAssertions;
using Infrastructure.MartenDb.Setup;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadNetworkRepositoryTests
{
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
