namespace RoadRegistry.Model
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using NetTopologySuite.Geometries;
    using Aiv.Vbr.Shaperon;
    using Testing;
    using Xunit;
    using FluentValidation;
    using FluentValidation.Results;

    public class RoadNodeScenarios : RoadRegistryFixture
    {
        public RoadNodeScenarios()
        {
            Fixture.CustomizePointM();
            Fixture.CustomizePolylineM();
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_that_has_not_been_taken()
        {
            var pointM = Fixture.Create<PointM>();
            var geometry = pointM.ToBytes();
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
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
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
            var geometry = Fixture.Create<PointM>().ToBytes();
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1,
                    Type = Shared.RoadNodeType.RealNode,
                    Geometry = geometry
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
                                Geometry = geometry
                            }
                        }
                    }
                ))
                .Throws(new RoadNodeIdTakenException(new RoadNodeId(1))));
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_taken_after_a_change()
        {
            var geometry = Fixture.Create<PointM>().ToBytes();
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Events.RoadNetworkChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Shared.RoadNodeType.RealNode,
                                Geometry = geometry
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
                                Geometry = geometry
                            }
                        }
                    }
                ))
                .Throws(new RoadNodeIdTakenException(new RoadNodeId(1))));
        }

        [Fact]
        public Task when_adding_a_node_with_a_geometry_that_is_not_a_point()
        {
            var geometry = Fixture.Create<MultiLineString>().ToBytes();
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
                .Throws(new ValidationException(
                    new []
                    {
                        new ValidationFailure("Changes[0].AddRoadNode.Geometry", "The 'Geometry' is not a PointM.")
                    })));
        }

        [Fact]
        public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
        {
            var geometry1 = Fixture.Create<PointM>().ToBytes();
            var geometry2 = Fixture.Create<PointM>().ToBytes();
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
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
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
