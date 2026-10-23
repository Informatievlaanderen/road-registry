namespace RoadRegistry.Tests.AggregateTests.RoadNode.MigrateRoadNode;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;

// A municipality uploads its road nodes under the WK_OIDN it knows them by, but the feature compare matches them on
// geometry: the road node a problem ends up being about can carry the identifier from the extract instead. Both have
// to be reported, or the uploader has nothing to look the road node up by.
public class OriginalIdTests : RoadNetworkTestBase
{
    private static readonly RoadNodeId RegisterId = new(1052854);
    private static readonly RoadNodeId UploadedId = new(123);

    [Fact]
    public void WhenTheUploadKnowsTheRoadNodeByAnotherIdentifier_ThenTheProblemCarriesBoth()
    {
        var result = Migrate(new ModifyRoadNodeChange
        {
            RoadNodeId = RegisterId,
            OriginalId = UploadedId,
            Geometry = Node(),
            Grensknoop = false
        });

        var problem = result.Problems.Should().ContainSingle(x => x.Reason == "RoadNodeNotFound").Subject;
        problem.Parameters.Should().ContainSingle(x => x.Name == "WegknoopId")
            .Which.Value.Should().Be("1052854");
        problem.Parameters.Should().ContainSingle(x => x.Name == "WegknoopOorspronkelijkeId")
            .Which.Value.Should().Be("123");
    }

    [Fact]
    public void WhenTheUploadKnowsTheRoadNodeByTheSameIdentifier_ThenThereIsNothingToAdd()
    {
        var result = Migrate(new ModifyRoadNodeChange
        {
            RoadNodeId = RegisterId,
            OriginalId = RegisterId,
            Geometry = Node(),
            Grensknoop = false
        });

        var problem = result.Problems.Should().ContainSingle(x => x.Reason == "RoadNodeNotFound").Subject;
        problem.Parameters.Should().NotContain(x => x.Name == "WegknoopOorspronkelijkeId");
    }

    [Fact]
    public void WhenTheUploadCarriedNoIdentifierOfItsOwn_ThenThereIsNothingToAdd()
    {
        var result = Migrate(new ModifyRoadNodeChange
        {
            RoadNodeId = RegisterId,
            Geometry = Node(),
            Grensknoop = false
        });

        var problem = result.Problems.Should().ContainSingle(x => x.Reason == "RoadNodeNotFound").Subject;
        problem.Parameters.Should().NotContain(x => x.Name == "WegknoopOorspronkelijkeId");
    }

    [Fact]
    public void WhenTheChangeUploadKnowsTheRoadNodeByAnotherIdentifier_ThenTheProblemCarriesBoth()
    {
        // Not only the inwinning upload: the identifier travels the same way through a change of the road network.
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>());

        var result = roadNetwork.Change(
            RoadNetworkChanges.Start().WithProvenance(new FakeProvenance()).Add(new ModifyRoadNodeChange
            {
                RoadNodeId = RegisterId,
                OriginalId = UploadedId,
                Geometry = Node(),
                Grensknoop = false
            }),
            null,
            new InMemoryRoadNetworkIdGenerator(initialValue: 1000));

        var problem = result.Problems.Should().ContainSingle(x => x.Reason == "RoadNodeNotFound").Subject;
        problem.Parameters.Should().ContainSingle(x => x.Name == "WegknoopOorspronkelijkeId")
            .Which.Value.Should().Be("123");
    }

    // The road network does not have the road node, so migrating it is refused - which is the shortest way to a
    // problem that names it.
    private RoadNetworkChangeResult Migrate(ModifyRoadNodeChange change)
    {
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>());

        return roadNetwork.Migrate(
            RoadNetworkChanges.Start().WithProvenance(new FakeProvenance()).Add(change),
            null,
            new InMemoryRoadNetworkIdGenerator(initialValue: 1000));
    }

    private static RoadNodeGeometry Node()
    {
        return new Point(new Coordinate(0, 0)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry();
    }
}
