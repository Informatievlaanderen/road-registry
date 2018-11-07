namespace RoadRegistry.Model
{
    using System.Threading.Tasks;
    using AutoFixture;
    using NetTopologySuite.Geometries;
    using Aiv.Vbr.Shaperon;
    using Testing;
    using Xunit;
    using FluentValidation;
    using FluentValidation.Results;
    using Messages;

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
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 1,
                            Type = Messages.RoadNodeType.FakeNode,
                            Geometry = geometry
                        }
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Messages.RoadNodeType.FakeNode,
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
            var addRoadNode = new Messages.AddRoadNode
            {
                Id = 1,
                Type = Messages.RoadNodeType.FakeNode,
                Geometry = geometry
            };
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1,
                    Type = Messages.RoadNodeType.RealNode,
                    Geometry = geometry
                })
                .When(TheOperator.ChangesTheRoadNetwork(new RequestedChange
                    {
                        AddRoadNode = addRoadNode
                    }))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    Rejections = new[]
                    {
                        new RejectedChange
                        {
                            AddRoadNode = addRoadNode,
                            Reason = "RodeNodeIdTaken",
                            Parameters = new ReasonParameter[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_taken_after_a_change()
        {
            var geometry = Fixture.Create<PointM>().ToBytes();
            var addRoadNode = new Messages.AddRoadNode
            {
                Id = 1,
                Type = Messages.RoadNodeType.FakeNode,
                Geometry = geometry
            };
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Messages.RoadNodeType.RealNode,
                                Geometry = geometry
                            }
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = addRoadNode
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    Rejections = new[]
                    {
                        new RejectedChange
                        {
                            AddRoadNode = addRoadNode,
                            Reason = "RodeNodeIdTaken",
                            Parameters = new ReasonParameter[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_node_with_a_geometry_that_is_not_a_point()
        {
            var geometry = Fixture.Create<MultiLineString>().ToBytes();
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 1,
                            Type = Messages.RoadNodeType.FakeNode,
                            Geometry = geometry
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
        public Task when_adding_a_node_with_a_geometry_that_has_been_taken()
        {
            var geometry = Fixture.Create<PointM>();
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Messages.RoadNodeType.RealNode,
                                Geometry = geometry.ToBytes()
                            }
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 2,
                            Type = Messages.RoadNodeType.FakeNode,
                            Geometry = geometry.ToBytes()
                        }
                    }
                ))
                .Throws(new RoadNodeGeometryTakenException(new RoadNodeId(2), new RoadNodeId(1), geometry)));
        }

        [Fact]
        public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
        {
            var geometry1 = Fixture.Create<PointM>().ToBytes();
            var geometry2 = Fixture.Create<PointM>().ToBytes();
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 1,
                            Type = Messages.RoadNodeType.FakeNode,
                            Geometry = geometry1
                        }
                    },
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 2,
                            Type = Messages.RoadNodeType.FakeNode,
                            Geometry = geometry2
                        }
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Messages.RoadNodeType.FakeNode,
                                Geometry = geometry1
                            }
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 2,
                                Type = Messages.RoadNodeType.FakeNode,
                                Geometry = geometry2
                            }
                        }
                    }
                }));
        }
    }
}
