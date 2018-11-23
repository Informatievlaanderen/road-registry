namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Aiv.Vbr.Shaperon;
    using Testing;
    using Xunit;
    using NetTopologySuite.Geometries;

    public class RoadNetworkScenarios : RoadRegistryFixture
    {
        public RoadNetworkScenarios()
        {
            Fixture.CustomizePointM();
            Fixture.CustomizePolylineM();

            Fixture.CustomizeAttributeId();
            Fixture.CustomizeMaintenanceAuthorityId();
            Fixture.CustomizeMaintenanceAuthorityName();
            Fixture.CustomizeRoadNodeId();
            Fixture.CustomizeRoadNodeType();
            Fixture.CustomizeRoadSegmentId();
            Fixture.CustomizeRoadSegmentCategory();
            Fixture.CustomizeRoadSegmentMorphology();
            Fixture.CustomizeRoadSegmentStatus();
            Fixture.CustomizeRoadSegmentAccessRestriction();
            Fixture.CustomizeRoadSegmentLaneCount();
            Fixture.CustomizeRoadSegmentLaneDirection();
            Fixture.CustomizeRoadSegmentNumberedRoadDirection();
            Fixture.CustomizeRoadSegmentGeometryDrawMethod();
            Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            Fixture.CustomizeRoadSegmentSurfaceType();
            Fixture.CustomizeRoadSegmentWidth();
            Fixture.CustomizeEuropeanRoadNumber();
            Fixture.CustomizeNationalRoadNumber();
            Fixture.CustomizeNumberedRoadNumber();
            Fixture.CustomizeOriginProperties();

            Fixture.Customize<Messages.RequestedRoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance => { instance.RoadNumber = Fixture.Create<EuropeanRoadNumber>(); })
                    .OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance => { instance.Ident2 = Fixture.Create<NationalRoadNumber>(); })
                    .OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.Ident8 = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentLaneAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentWidthAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentSurfaceAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
                }).OmitAutoProperties());

            AddStartNode1 = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Geometry = GeometryTranslator.Translate(Fixture.Create<PointM>()),
                Type = RoadNodeType.FakeNode
            };

            StartNode1Added = new Messages.RoadNodeAdded
            {
                Id = 1,
                TemporaryId = AddStartNode1.TemporaryId,
                Geometry = AddStartNode1.Geometry,
                Type = AddStartNode1.Type
            };

            AddStartNode2 = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Geometry = GeometryTranslator.Translate(Fixture.Create<PointM>()),
                Type = RoadNodeType.FakeNode
            };

            StartNode2Added = new Messages.RoadNodeAdded
            {
                Id = 3,
                TemporaryId = AddStartNode2.TemporaryId,
                Geometry = AddStartNode2.Geometry,
                Type = AddStartNode2.Type
            };

            AddEndNode1 = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Geometry = GeometryTranslator.Translate(Fixture.Create<PointM>()),
                Type = RoadNodeType.FakeNode
            };

            EndNode1Added = new Messages.RoadNodeAdded
            {
                Id = 2,
                TemporaryId = AddEndNode1.TemporaryId,
                Geometry = AddEndNode1.Geometry,
                Type = AddEndNode1.Type
            };

            AddEndNode2 = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Geometry = GeometryTranslator.Translate(Fixture.Create<PointM>()),
                Type = RoadNodeType.FakeNode
            };

            EndNode2Added = new Messages.RoadNodeAdded
            {
                Id = 4,
                TemporaryId = AddEndNode2.TemporaryId,
                Geometry = AddEndNode2.Geometry,
                Type = AddEndNode2.Type
            };

            var laneCount = new Random().Next(0, 10);
            var widthCount = new Random().Next(0, 10);
            var surfaceCount = new Random().Next(0, 10);
            var europeanRoadCount = new Random().Next(0, 10);
            var nationalRoadCount = new Random().Next(0, 10);
            var numberedRoadCount = new Random().Next(0, 10);
            AddSegment1 = new Messages.AddRoadSegment
            {
                TemporaryId = Fixture.Create<RoadSegmentId>(),
                StartNodeId = AddStartNode1.TemporaryId,
                EndNodeId = AddEndNode1.TemporaryId,
                Geometry = GeometryTranslator.Translate(Fixture.Create<MultiLineString>()),
                MaintenanceAuthority = Fixture.Create<MaintenanceAuthorityId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                PartOfEuropeanRoads = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentEuropeanRoadAttributes>(europeanRoadCount)
                    .ToArray(),
                PartOfNationalRoads = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentNationalRoadAttributes>(nationalRoadCount)
                    .ToArray(),
                PartOfNumberedRoads = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentNumberedRoadAttributes>(numberedRoadCount)
                    .ToArray(),
                Lanes = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentLaneAttributes>(laneCount)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * 5;
                        part.ToPosition = (index + 1) * 5;
                        return part;
                    })
                    .ToArray(),
                Widths = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentWidthAttributes>(widthCount)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * 5;
                        part.ToPosition = (index + 1) * 5;
                        return part;
                    })
                    .ToArray(),
                Surfaces = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentSurfaceAttributes>(surfaceCount)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * 5;
                        part.ToPosition = (index + 1) * 5;
                        return part;
                    })
                    .ToArray()
            };

            Segment1Added = new Messages.RoadSegmentAdded
            {
                Id = 1,
                TemporaryId = AddSegment1.TemporaryId,
                StartNodeId = 1,
                EndNodeId = 2,
                Geometry = AddSegment1.Geometry,
                GeometryVersion = 0,
                MaintenanceAuthority = new Messages.MaintenanceAuthority
                {
                    Code = AddSegment1.MaintenanceAuthority,
                    Name = null
                },
                GeometryDrawMethod = AddSegment1.GeometryDrawMethod,
                Morphology = AddSegment1.Morphology,
                Status = AddSegment1.Status,
                Category = AddSegment1.Category,
                AccessRestriction = AddSegment1.AccessRestriction,
                LeftSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment1.LeftSideStreetNameId
                },
                RightSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment1.RightSideStreetNameId
                },
                PartOfEuropeanRoads = AddSegment1.PartOfEuropeanRoads
                    .Select((part, index) => new Messages.RoadSegmentEuropeanRoadAttributes
                    {
                        AttributeId = index + 1, RoadNumber = part.RoadNumber
                    })
                    .ToArray(),
                PartOfNationalRoads = AddSegment1.PartOfNationalRoads
                    .Select((part, index) => new Messages.RoadSegmentNationalRoadAttributes
                    {
                        AttributeId = index + 1,
                        Ident2 = part.Ident2
                    })
                    .ToArray(),
                PartOfNumberedRoads = AddSegment1.PartOfNumberedRoads
                    .Select((part, index) => new Messages.RoadSegmentNumberedRoadAttributes
                    {
                        AttributeId = index + 1,
                        Ident8 = part.Ident8,
                        Direction = part.Direction,
                        Ordinal = part.Ordinal
                    })
                    .ToArray(),
                Lanes = AddSegment1.Lanes
                    .Select((lane, index) => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = index + 1,
                        Direction = lane.Direction,
                        Count = lane.Count,
                        FromPosition = lane.FromPosition,
                        ToPosition = lane.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Widths = AddSegment1.Widths
                    .Select((width, index) => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = index + 1,
                        Width = width.Width,
                        FromPosition = width.FromPosition,
                        ToPosition = width.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Surfaces = AddSegment1.Surfaces
                    .Select((surface, index) => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = index + 1,
                        Type = surface.Type,
                        FromPosition = surface.FromPosition,
                        ToPosition = surface.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Version = 0
            };

            AddSegment2 = new Messages.AddRoadSegment
            {
                TemporaryId = Fixture.Create<RoadSegmentId>(),
                StartNodeId = AddStartNode2.TemporaryId,
                EndNodeId = AddEndNode2.TemporaryId,
                Geometry = GeometryTranslator.Translate(Fixture.Create<MultiLineString>()),
                MaintenanceAuthority = Fixture.Create<MaintenanceAuthorityId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                PartOfEuropeanRoads = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentEuropeanRoadAttributes>(new Random().Next(0, 10))
                    .ToArray(),
                PartOfNationalRoads = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentNationalRoadAttributes>(new Random().Next(0, 10))
                    .ToArray(),
                PartOfNumberedRoads = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentNumberedRoadAttributes>(new Random().Next(0, 10))
                    .ToArray(),
                Lanes = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentLaneAttributes>(new Random().Next(0, 10))
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * 5;
                        part.ToPosition = (index + 1) * 5;
                        return part;
                    })
                    .ToArray(),
                Widths = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentWidthAttributes>(new Random().Next(0, 10))
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * 5;
                        part.ToPosition = (index + 1) * 5;
                        return part;
                    })
                    .ToArray(),
                Surfaces = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentSurfaceAttributes>(new Random().Next(0, 10))
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * 5;
                        part.ToPosition = (index + 1) * 5;
                        return part;
                    })
                    .ToArray()
            };

            Segment2Added = new Messages.RoadSegmentAdded
            {
                Id = 2,
                TemporaryId = AddSegment2.TemporaryId,
                StartNodeId = 3,
                EndNodeId = 4,
                Geometry = AddSegment2.Geometry,
                GeometryVersion = 0,
                MaintenanceAuthority = new Messages.MaintenanceAuthority
                {
                    Code = AddSegment2.MaintenanceAuthority,
                    Name = null
                },
                GeometryDrawMethod = AddSegment2.GeometryDrawMethod,
                Morphology = AddSegment2.Morphology,
                Status = AddSegment2.Status,
                Category = AddSegment2.Category,
                AccessRestriction = AddSegment2.AccessRestriction,
                LeftSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment2.LeftSideStreetNameId
                },
                RightSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment2.RightSideStreetNameId
                },
                PartOfEuropeanRoads = AddSegment2.PartOfEuropeanRoads
                    .Select((part, index) => new Messages.RoadSegmentEuropeanRoadAttributes
                    {
                        AttributeId = europeanRoadCount + index + 1, RoadNumber = part.RoadNumber
                    })
                    .ToArray(),
                PartOfNationalRoads = AddSegment2.PartOfNationalRoads
                    .Select((part, index) => new Messages.RoadSegmentNationalRoadAttributes
                    {
                        AttributeId = nationalRoadCount + index + 1,
                        Ident2 = part.Ident2
                    })
                    .ToArray(),
                PartOfNumberedRoads = AddSegment2.PartOfNumberedRoads
                    .Select((part, index) => new Messages.RoadSegmentNumberedRoadAttributes
                    {
                        AttributeId = numberedRoadCount + index + 1,
                        Ident8 = part.Ident8,
                        Direction = part.Direction,
                        Ordinal = part.Ordinal
                    })
                    .ToArray(),
                Lanes = AddSegment2.Lanes
                    .Select((lane, index) => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = laneCount + index + 1,
                        Direction = lane.Direction,
                        Count = lane.Count,
                        FromPosition = lane.FromPosition,
                        ToPosition = lane.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Widths = AddSegment2.Widths
                    .Select((width, index) => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = widthCount + index + 1,
                        Width = width.Width,
                        FromPosition = width.FromPosition,
                        ToPosition = width.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Surfaces = AddSegment2.Surfaces
                    .Select((surface, index) => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = surfaceCount + index + 1,
                        Type = surface.Type,
                        FromPosition = surface.FromPosition,
                        ToPosition = surface.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Version = 0
            };
        }

        public Messages.AddRoadNode AddStartNode1 { get; }
        public Messages.AddRoadNode AddEndNode1 { get; }
        public Messages.AddRoadSegment AddSegment1 { get; }

        public Messages.RoadNodeAdded StartNode1Added { get; }
        public Messages.RoadNodeAdded EndNode1Added { get; }
        public Messages.RoadSegmentAdded Segment1Added { get; }

        public Messages.AddRoadNode AddStartNode2 { get; }
        public Messages.AddRoadNode AddEndNode2 { get; }
        public Messages.AddRoadSegment AddSegment2 { get; }

        public Messages.RoadNodeAdded StartNode2Added { get; }
        public Messages.RoadNodeAdded EndNode2Added { get; }
        public Messages.RoadSegmentAdded Segment2Added { get; }

        [Fact]
        public Task when_adding_a_start_and_end_node_and_segment()
        {
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_start_node_with_a_geometry_that_has_been_taken()
        {
            AddStartNode2.Geometry = StartNode1Added.Geometry;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode2,
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeGeometryTaken",
                                    Parameters = new[]
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "ByOtherNode",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_an_end_node_with_a_geometry_that_has_been_taken()
        {
            AddEndNode2.Geometry = EndNode1Added.Geometry;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode2,
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeGeometryTaken",
                                    Parameters = new[]
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "ByOtherNode",
                                            Value = "2"
                                        }
                                    }
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
        {
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode2Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_start_node_that_is_within_two_meters_of_another_node()
        {
            var geometry1 = GeometryTranslator.Translate(StartNode1Added.Geometry);
            var random = new Random();
            var geometry2 = new PointM(
                geometry1.X + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                geometry1.Y + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                geometry1.Z + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance
            );
            AddStartNode2.Geometry = GeometryTranslator.Translate(geometry2);
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode2,
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTooClose",
                                    Parameters = new[]
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "ToOtherNode",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_an_end_node_that_is_within_two_meters_of_another_node()
        {
            var geometry1 = GeometryTranslator.Translate(EndNode1Added.Geometry);
            var random = new Random();
            var geometry2 = new PointM(
                geometry1.X + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                geometry1.Y + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                geometry1.Z + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance
            );
            AddEndNode2.Geometry = GeometryTranslator.Translate(geometry2);
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode2,
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTooClose",
                                    Parameters = new[]
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "ToOtherNode",
                                            Value = "2"
                                        }
                                    }
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_changes_are_out_of_order()
        {
            // TemporaryId influences order.
            AddStartNode1.TemporaryId = 1;
            StartNode1Added.TemporaryId = 1;
            AddEndNode1.TemporaryId = 2;
            EndNode1Added.TemporaryId = 2;
            AddSegment1.StartNodeId = 1;
            AddSegment1.EndNodeId = 2;

            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_segment_with_a_geometry_that_has_been_taken()
        {
            AddSegment2.Geometry = Segment1Added.Geometry;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadSegmentGeometryTaken",
                                    Parameters = new[]
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "ByOtherSegment",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }));
        }
    }
}
