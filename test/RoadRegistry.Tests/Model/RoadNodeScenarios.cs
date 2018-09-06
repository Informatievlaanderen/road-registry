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
        public Task when_adding_a_node_with_an_id_that_has_not_been_taken()
        {
            //TODO: Make sure we use a point geometry
            var geometry = Fixture.CreateMany<byte>().ToArray();
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new[]
                    {
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new Commands.AddRoadNode
                            {
                                Id = 1,
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = geometry
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
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = geometry
                            }
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_taken_after_an_import()
        {
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1,
                    Type = Shared.RoadNodeType.RealNode,
                    Geometry = Fixture.CreateMany<byte>().ToArray()
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new[]
                    {
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new Commands.AddRoadNode
                            {
                                Id = 1,
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                ))
                .Throws(new RoadNodeIdTakenException(new RoadNodeId(1))));
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_taken_after_a_change()
        {
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new RoadNetworkChanged
                {
                    Changeset = new[]
                    {
                        new Events.RoadNetworkChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Shared.RoadNodeType.RealNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new[]
                    {
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new Commands.AddRoadNode
                            {
                                Id = 1,
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                ))
                .Throws(new RoadNodeIdTakenException(new RoadNodeId(1))));
        }

        [Fact]
        public Task when_adding_a_node_with_a_geometry_that_is_not_a_point()
        {
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new[]
                    {
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new Commands.AddRoadNode
                            {
                                Id = 1,
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                ))
                .Throws(new RoadNodeGeometryMismatchException(new RoadNodeId(1))));
        }

        [Fact]
        public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
        {
            //TODO: Make sure we use a point geometry
            var geometry1 = Fixture.CreateMany<byte>().ToArray();
            var geometry2 = Fixture.CreateMany<byte>().ToArray();
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new[]
                    {
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new Commands.AddRoadNode
                            {
                                Id = 1,
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = geometry1
                            }
                        },
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new Commands.AddRoadNode
                            {
                                Id = 2,
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = geometry2
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
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = geometry1
                            }
                        },
                        new Events.RoadNetworkChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 2,
                                Type = Shared.RoadNodeType.FakeNode,
                                Geometry = geometry2
                            }
                        }
                    }
                }));
        }
    }
}
