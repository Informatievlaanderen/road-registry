namespace RoadRegistry.Model
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Commands;
    using Testing;
    using Xunit;

    public class RoadNodeScenarios : RoadRegistryFixture
    {

        [Fact]
        public Task when_adding_a_non_existent_node()
        {
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new[]
                    {
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new AddRoadNode
                            {
                                Id = 1,
                                Type = Events.RoadNodeType.FakeNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChanged
                {
                    Changeset = new[]
                    {
                        new Events.RoadNetworkChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Events.RoadNodeType.FakeNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                }));
        }
    }
}
