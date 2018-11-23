namespace RoadRegistry.Model
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using NetTopologySuite.Geometries;
    using Aiv.Vbr.Shaperon;
    using Testing;
    using Xunit;
    using Messages;
    using System.Linq;

    public class RoadSegmentScenarios : RoadRegistryFixture
    {
        public RoadSegmentScenarios()
        {
            Fixture.CustomizePointM();
            Fixture.CustomizePolylineM();

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

            Fixture.Customize<RequestedRoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.RoadNumber =Fixture.Create<EuropeanRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.Ident2 = Fixture.Create<NationalRoadNumber>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.Ident8 = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentLaneAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentWidthAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentSurfaceAttributes>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
                }).OmitAutoProperties());
        }

        [Fact]
        public Task when_adding_a_segment_with_an_id_that_has_not_been_taken()
        {
            var temporaryId1 = Fixture.Create<RoadNodeId>();
            var temporaryId2 = Fixture.Create<RoadNodeId>();
            var temporaryId3 = Fixture.Create<RoadSegmentId>();
            var start = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var end = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var line = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
            return Run(scenario =>
            {
                var maintainer = Fixture.Create<MaintenanceAuthorityId>();
                var geometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>();
                var morphology = Fixture.Create<RoadSegmentMorphology>();
                var status = Fixture.Create<RoadSegmentStatus>();
                var category = Fixture.Create<RoadSegmentCategory>();
                var accessRestriction = Fixture.Create<RoadSegmentAccessRestriction>();
                var leftSideStreetNameId = Fixture.Create<int?>();
                var rightSideStreetNameId = Fixture.Create<int?>();
                var partOfEuropeanRoads = Fixture.CreateMany<RequestedRoadSegmentEuropeanRoadAttributes>()
                    .ToArray();
                var partOfNationalRoads = Fixture.CreateMany<RequestedRoadSegmentNationalRoadAttributes>()
                    .ToArray();
                var partOfNumberedRoads = Fixture.CreateMany<RequestedRoadSegmentNumberedRoadAttributes>()
                    .ToArray();
                var lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttributes>().ToArray();
                var widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttributes>().ToArray();
                var surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttributes>().ToArray();
                return scenario
                    .GivenNone()
                    .When(TheOperator.ChangesTheRoadNetwork(
                        new RequestedChange
                        {
                            AddRoadNode = new Messages.AddRoadNode
                            {
                                TemporaryId = temporaryId1,
                                Type = RoadNodeType.FakeNode,
                                Geometry = start
                            }
                        },
                        new RequestedChange
                        {
                            AddRoadNode = new Messages.AddRoadNode
                            {
                                TemporaryId = temporaryId2,
                                Type = RoadNodeType.FakeNode,
                                Geometry = end
                            }
                        },
                        new RequestedChange
                        {
                            AddRoadSegment = new Messages.AddRoadSegment
                            {
                                TemporaryId = temporaryId3,
                                StartNodeId = temporaryId1,
                                EndNodeId = temporaryId2,
                                Geometry = line,
                                MaintenanceAuthority = maintainer,
                                GeometryDrawMethod = geometryDrawMethod,
                                Morphology = morphology,
                                Status = status,
                                Category = category,
                                AccessRestriction = accessRestriction,
                                LeftSideStreetNameId = leftSideStreetNameId,
                                RightSideStreetNameId = rightSideStreetNameId,
                                PartOfEuropeanRoads = partOfEuropeanRoads,
                                PartOfNationalRoads = partOfNationalRoads,
                                PartOfNumberedRoads = partOfNumberedRoads,
                                Lanes = lanes,
                                Widths = widths,
                                Surfaces = surfaces
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
                                    TemporaryId = temporaryId1,
                                    Type = RoadNodeType.FakeNode,
                                    Geometry = start
                                }
                            },
                            new AcceptedChange
                            {
                                RoadNodeAdded = new RoadNodeAdded
                                {
                                    Id = 2,
                                    TemporaryId = temporaryId2,
                                    Type = RoadNodeType.FakeNode,
                                    Geometry = end
                                }
                            },
                            new AcceptedChange
                            {
                                RoadSegmentAdded = new RoadSegmentAdded
                                {
                                    Id = 1,
                                    TemporaryId = temporaryId3,
                                    StartNodeId = 1,
                                    EndNodeId = 2,
                                    Geometry = line,
                                    GeometryVersion = 0,
                                    MaintenanceAuthority = new MaintenanceAuthority
                                    {
                                        Code = maintainer,
                                        Name = null
                                    },
                                    GeometryDrawMethod = geometryDrawMethod,
                                    Morphology = morphology,
                                    Status = status,
                                    Category = category,
                                    AccessRestriction = accessRestriction,
                                    LeftSide = new RoadSegmentSideAttributes
                                    {
                                        StreetNameId = leftSideStreetNameId
                                    },
                                    RightSide = new RoadSegmentSideAttributes
                                    {
                                        StreetNameId = rightSideStreetNameId
                                    },
                                    PartOfEuropeanRoads = Array.ConvertAll(
                                        partOfEuropeanRoads,
                                        part => new RoadSegmentEuropeanRoadAttributes
                                        {
                                            AttributeId = 1, RoadNumber = part.RoadNumber
                                        }),
                                    PartOfNationalRoads = Array.ConvertAll(
                                        partOfNationalRoads,
                                        part => new RoadSegmentNationalRoadAttributes
                                        {
                                            AttributeId = 1,
                                            Ident2 = part.Ident2
                                        }),
                                    PartOfNumberedRoads = Array.ConvertAll(
                                        partOfNumberedRoads,
                                        part => new RoadSegmentNumberedRoadAttributes
                                        {
                                            AttributeId = 1,
                                            Ident8 = part.Ident8,
                                            Direction = part.Direction,
                                            Ordinal = part.Ordinal,
                                        }),
                                    Lanes = Array.ConvertAll(
                                        lanes,
                                        lane => new Messages.RoadSegmentLaneAttributes
                                        {
                                            AttributeId = 1,
                                            Direction = lane.Direction,
                                            Count = lane.Count,
                                            FromPosition = lane.FromPosition,
                                            ToPosition = lane.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Widths = Array.ConvertAll(
                                        widths,
                                        width => new Messages.RoadSegmentWidthAttributes
                                        {
                                            AttributeId = 1,
                                            Width = width.Width,
                                            FromPosition = width.FromPosition,
                                            ToPosition = width.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Surfaces = Array.ConvertAll(
                                        surfaces,
                                        surface => new Messages.RoadSegmentSurfaceAttributes
                                        {
                                            AttributeId = 1,
                                            Type = surface.Type,
                                            FromPosition = surface.FromPosition,
                                            ToPosition = surface.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Version = 0
                                }
                            }
                        }
                    });
            });
        }

        [Fact]
        public Task when_changes_are_added_out_of_order()
        {
            var temporaryId1 = Fixture.Create<RoadNodeId>();
            var temporaryId2 = Fixture.Create<RoadNodeId>();
            var temporaryId3 = Fixture.Create<RoadSegmentId>();
            var start = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var end = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var line = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
            return Run(scenario =>
            {
                var maintainer = Fixture.Create<MaintenanceAuthorityId>();
                var geometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>();
                var morphology = Fixture.Create<RoadSegmentMorphology>();
                var status = Fixture.Create<RoadSegmentStatus>();
                var category = Fixture.Create<RoadSegmentCategory>();
                var accessRestriction = Fixture.Create<RoadSegmentAccessRestriction>();
                var leftSideStreetNameId = Fixture.Create<int?>();
                var rightSideStreetNameId = Fixture.Create<int?>();
                var partOfEuropeanRoads = Fixture.CreateMany<RequestedRoadSegmentEuropeanRoadAttributes>()
                    .ToArray();
                var partOfNationalRoads = Fixture.CreateMany<RequestedRoadSegmentNationalRoadAttributes>()
                    .ToArray();
                var partOfNumberedRoads = Fixture.CreateMany<RequestedRoadSegmentNumberedRoadAttributes>()
                    .ToArray();
                var lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttributes>().ToArray();
                var widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttributes>().ToArray();
                var surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttributes>().ToArray();
                return scenario
                    .GivenNone()
                    .When(TheOperator.ChangesTheRoadNetwork(
                        new RequestedChange
                        {
                            AddRoadSegment = new Messages.AddRoadSegment
                            {
                                TemporaryId = temporaryId3,
                                StartNodeId = temporaryId1,
                                EndNodeId = temporaryId2,
                                Geometry = line,
                                MaintenanceAuthority = maintainer,
                                GeometryDrawMethod = geometryDrawMethod,
                                Morphology = morphology,
                                Status = status,
                                Category = category,
                                AccessRestriction = accessRestriction,
                                LeftSideStreetNameId = leftSideStreetNameId,
                                RightSideStreetNameId = rightSideStreetNameId,
                                PartOfEuropeanRoads = partOfEuropeanRoads,
                                PartOfNationalRoads = partOfNationalRoads,
                                PartOfNumberedRoads = partOfNumberedRoads,
                                Lanes = lanes,
                                Widths = widths,
                                Surfaces = surfaces
                            }
                        },
                        new RequestedChange
                        {
                            AddRoadNode = new Messages.AddRoadNode
                            {
                                TemporaryId = temporaryId1,
                                Type = RoadNodeType.FakeNode,
                                Geometry = start
                            }
                        },
                        new RequestedChange
                        {
                            AddRoadNode = new Messages.AddRoadNode
                            {
                                TemporaryId = temporaryId2,
                                Type = RoadNodeType.FakeNode,
                                Geometry = end
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
                                    TemporaryId = temporaryId1,
                                    Type = RoadNodeType.FakeNode,
                                    Geometry = start
                                }
                            },
                            new AcceptedChange
                            {
                                RoadNodeAdded = new RoadNodeAdded
                                {
                                    Id = 2,
                                    TemporaryId = temporaryId2,
                                    Type = RoadNodeType.FakeNode,
                                    Geometry = end
                                }
                            },
                            new AcceptedChange
                            {
                                RoadSegmentAdded = new RoadSegmentAdded
                                {
                                    Id = 1,
                                    TemporaryId = temporaryId3,
                                    StartNodeId = 1,
                                    EndNodeId = 2,
                                    Geometry = line,
                                    GeometryVersion = 0,
                                    MaintenanceAuthority = new MaintenanceAuthority
                                    {
                                        Code = maintainer,
                                        Name = null
                                    },
                                    GeometryDrawMethod = geometryDrawMethod,
                                    Morphology = morphology,
                                    Status = status,
                                    Category = category,
                                    AccessRestriction = accessRestriction,
                                    LeftSide = new RoadSegmentSideAttributes
                                    {
                                        StreetNameId = leftSideStreetNameId
                                    },
                                    RightSide = new RoadSegmentSideAttributes
                                    {
                                        StreetNameId = rightSideStreetNameId
                                    },
                                    PartOfEuropeanRoads = Array.ConvertAll(
                                        partOfEuropeanRoads,
                                        part => new RoadSegmentEuropeanRoadAttributes
                                        {
                                            AttributeId = 1, RoadNumber = part.RoadNumber
                                        }),
                                    PartOfNationalRoads = Array.ConvertAll(
                                        partOfNationalRoads,
                                        part => new RoadSegmentNationalRoadAttributes
                                        {
                                            AttributeId = 1,
                                            Ident2 = part.Ident2
                                        }),
                                    PartOfNumberedRoads = Array.ConvertAll(
                                        partOfNumberedRoads,
                                        part => new RoadSegmentNumberedRoadAttributes
                                        {
                                            AttributeId = 1,
                                            Ident8 = part.Ident8,
                                            Direction = part.Direction,
                                            Ordinal = part.Ordinal,
                                        }),
                                    Lanes = Array.ConvertAll(
                                        lanes,
                                        lane => new Messages.RoadSegmentLaneAttributes
                                        {
                                            AttributeId = 1,
                                            Direction = lane.Direction,
                                            Count = lane.Count,
                                            FromPosition = lane.FromPosition,
                                            ToPosition = lane.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Widths = Array.ConvertAll(
                                        widths,
                                        width => new Messages.RoadSegmentWidthAttributes
                                        {
                                            AttributeId = 1,
                                            Width = width.Width,
                                            FromPosition = width.FromPosition,
                                            ToPosition = width.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Surfaces = Array.ConvertAll(
                                        surfaces,
                                        surface => new Messages.RoadSegmentSurfaceAttributes
                                        {
                                            AttributeId = 1,
                                            Type = surface.Type,
                                            FromPosition = surface.FromPosition,
                                            ToPosition = surface.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Version = 0
                                }
                            }
                        }
                    });
            });
        }

        [Fact]
        public Task when_adding_a_segment_with_a_geometry_that_has_been_taken()
        {
            var temporaryId1 = Fixture.Create<RoadNodeId>();
            var temporaryId2 = Fixture.Create<RoadNodeId>();
            var temporaryId3 = Fixture.Create<RoadSegmentId>();
            var temporaryId4 = Fixture.Create<RoadSegmentId>();
            var start = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var end = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var line = GeometryTranslator.Translate(Fixture.Create<MultiLineString>());
            return Run(scenario =>
            {
                var maintainer = Fixture.Create<MaintenanceAuthorityId>();
                var geometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>();
                var morphology = Fixture.Create<RoadSegmentMorphology>();
                var status = Fixture.Create<RoadSegmentStatus>();
                var category = Fixture.Create<RoadSegmentCategory>();
                var accessRestriction = Fixture.Create<RoadSegmentAccessRestriction>();
                var leftSideStreetNameId = Fixture.Create<int?>();
                var rightSideStreetNameId = Fixture.Create<int?>();
                var partOfEuropeanRoads = Fixture.CreateMany<RequestedRoadSegmentEuropeanRoadAttributes>()
                    .ToArray();
                var partOfNationalRoads = Fixture.CreateMany<RequestedRoadSegmentNationalRoadAttributes>()
                    .ToArray();
                var partOfNumberedRoads = Fixture.CreateMany<RequestedRoadSegmentNumberedRoadAttributes>()
                    .ToArray();
                var lanes = Fixture.CreateMany<RequestedRoadSegmentLaneAttributes>().ToArray();
                var widths = Fixture.CreateMany<RequestedRoadSegmentWidthAttributes>().ToArray();
                var surfaces = Fixture.CreateMany<RequestedRoadSegmentSurfaceAttributes>().ToArray();
                return scenario
                    .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                    {
                        Changes = new[]
                        {
                            new AcceptedChange
                            {
                                RoadNodeAdded = new RoadNodeAdded
                                {
                                    Id = 1,
                                    TemporaryId = temporaryId1,
                                    Type = RoadNodeType.FakeNode,
                                    Geometry = start
                                }
                            },
                            new AcceptedChange
                            {
                                RoadNodeAdded = new RoadNodeAdded
                                {
                                    Id = 2,
                                    TemporaryId = temporaryId2,
                                    Type = RoadNodeType.FakeNode,
                                    Geometry = end
                                }
                            },
                            new AcceptedChange
                            {
                                RoadSegmentAdded = new RoadSegmentAdded
                                {
                                    Id = 1,
                                    TemporaryId = temporaryId3,
                                    StartNodeId = 1,
                                    EndNodeId = 2,
                                    Geometry = line,
                                    GeometryVersion = 0,
                                    MaintenanceAuthority = new MaintenanceAuthority
                                    {
                                        Code = maintainer,
                                        Name = null
                                    },
                                    GeometryDrawMethod = geometryDrawMethod,
                                    Morphology = morphology,
                                    Status = status,
                                    Category = category,
                                    AccessRestriction = accessRestriction,
                                    LeftSide = new RoadSegmentSideAttributes
                                    {
                                        StreetNameId = leftSideStreetNameId
                                    },
                                    RightSide = new RoadSegmentSideAttributes
                                    {
                                        StreetNameId = rightSideStreetNameId
                                    },
                                    PartOfEuropeanRoads = Array.ConvertAll(
                                        partOfEuropeanRoads,
                                        part => new RoadSegmentEuropeanRoadAttributes
                                        {
                                            AttributeId = 1, RoadNumber = part.RoadNumber
                                        }),
                                    PartOfNationalRoads = Array.ConvertAll(
                                        partOfNationalRoads,
                                        part => new RoadSegmentNationalRoadAttributes
                                        {
                                            AttributeId = 1,
                                            Ident2 = part.Ident2
                                        }),
                                    PartOfNumberedRoads = Array.ConvertAll(
                                        partOfNumberedRoads,
                                        part => new RoadSegmentNumberedRoadAttributes
                                        {
                                            AttributeId = 1,
                                            Ident8 = part.Ident8,
                                            Direction = part.Direction,
                                            Ordinal = part.Ordinal,
                                        }),
                                    Lanes = Array.ConvertAll(
                                        lanes,
                                        lane => new Messages.RoadSegmentLaneAttributes
                                        {
                                            AttributeId = 1,
                                            Direction = lane.Direction,
                                            Count = lane.Count,
                                            FromPosition = lane.FromPosition,
                                            ToPosition = lane.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Widths = Array.ConvertAll(
                                        widths,
                                        width => new Messages.RoadSegmentWidthAttributes
                                        {
                                            AttributeId = 1,
                                            Width = width.Width,
                                            FromPosition = width.FromPosition,
                                            ToPosition = width.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Surfaces = Array.ConvertAll(
                                        surfaces,
                                        surface => new Messages.RoadSegmentSurfaceAttributes
                                        {
                                            AttributeId = 1,
                                            Type = surface.Type,
                                            FromPosition = surface.FromPosition,
                                            ToPosition = surface.ToPosition,
                                            AsOfGeometryVersion = 1
                                        }),
                                    Version = 0
                                }
                            }
                        }
                    })
                    .When(TheOperator.ChangesTheRoadNetwork(
                        new RequestedChange
                        {
                            AddRoadSegment = new Messages.AddRoadSegment
                            {
                                TemporaryId = temporaryId4,
                                StartNodeId = 1,
                                EndNodeId = 2,
                                Geometry = line,
                                MaintenanceAuthority = maintainer,
                                GeometryDrawMethod = geometryDrawMethod,
                                Morphology = morphology,
                                Status = status,
                                Category = category,
                                AccessRestriction = accessRestriction,
                                LeftSideStreetNameId = leftSideStreetNameId,
                                RightSideStreetNameId = rightSideStreetNameId,
                                PartOfEuropeanRoads = partOfEuropeanRoads,
                                PartOfNationalRoads = partOfNationalRoads,
                                PartOfNumberedRoads = partOfNumberedRoads,
                                Lanes = lanes,
                                Widths = widths,
                                Surfaces = surfaces
                            }
                        }
                    ))
                    .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                    {
                        Changes = new[]
                        {
                            new RejectedChange
                            {
                                AddRoadSegment = new Messages.AddRoadSegment
                                {
                                    TemporaryId = temporaryId4,
                                    StartNodeId = 1,
                                    EndNodeId = 2,
                                    Geometry = line,
                                    MaintenanceAuthority = maintainer,
                                    GeometryDrawMethod = geometryDrawMethod,
                                    Morphology = morphology,
                                    Status = status,
                                    Category = category,
                                    AccessRestriction = accessRestriction,
                                    LeftSideStreetNameId = leftSideStreetNameId,
                                    RightSideStreetNameId = rightSideStreetNameId,
                                    PartOfEuropeanRoads = partOfEuropeanRoads,
                                    PartOfNationalRoads = partOfNationalRoads,
                                    PartOfNumberedRoads = partOfNumberedRoads,
                                    Lanes = lanes,
                                    Widths = widths,
                                    Surfaces = surfaces
                                },
                                Reasons = new[]
                                {
                                    new Reason
                                    {
                                        Because = "RoadSegmentGeometryTaken",
                                        Parameters = new[]
                                        {
                                            new ReasonParameter
                                            {
                                                Name = "ByOtherSegment",
                                                Value = "1"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
            });
        }
        // [Fact]
        // public Task when_adding_a_node_with_an_id_taken_after_an_import()
        // {
        //     var geometry1 = Fixture.Create<PointM>().ToBytes();
        //     var geometry2 = Fixture.Create<PointM>().ToBytes();
        //     var addRoadNode = new Messages.AddRoadNode
        //     {
        //         Id = 1,
        //         Type = Messages.RoadNodeType.FakeNode,
        //         Geometry = geometry1
        //     };
        //     return Run(scenario => scenario
        //         .Given(RoadNetworks.Stream, new ImportedRoadNode
        //         {
        //             Id = 1,
        //             Type = Messages.RoadNodeType.RealNode,
        //             Geometry = geometry2
        //         })
        //         .When(TheOperator.ChangesTheRoadNetwork(new RequestedChange
        //             {
        //                 AddRoadNode = addRoadNode
        //             }))
        //         .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
        //         {
        //             Changes = new[]
        //             {
        //                 new RejectedChange
        //                 {
        //                     AddRoadNode = addRoadNode,
        //                     Reasons = new []
        //                     {
        //                         new Reason
        //                         {
        //                             Because = "RoadNodeIdTaken",
        //                             Parameters = new ReasonParameter[0]
        //                         }
        //                     }
        //                 }
        //             }
        //         })
        //     );
        // }

        // [Fact]
        // public Task when_adding_a_node_with_an_id_taken_after_a_change()
        // {
        //     var geometry1 = Fixture.Create<PointM>().ToBytes();
        //     var geometry2 = Fixture.Create<PointM>().ToBytes();
        //     var addRoadNode = new Messages.AddRoadNode
        //     {
        //         Id = 1,
        //         Type = Messages.RoadNodeType.FakeNode,
        //         Geometry = geometry1
        //     };
        //     return Run(scenario => scenario
        //         .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        //         {
        //             Changes = new[]
        //             {
        //                 new AcceptedChange
        //                 {
        //                     RoadNodeAdded = new RoadNodeAdded
        //                     {
        //                         Id = 1,
        //                         Type = Messages.RoadNodeType.RealNode,
        //                         Geometry = geometry2
        //                     }
        //                 }
        //             }
        //         })
        //         .When(TheOperator.ChangesTheRoadNetwork(
        //             new RequestedChange
        //             {
        //                 AddRoadNode = addRoadNode
        //             }
        //         ))
        //         .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
        //         {
        //             Changes = new[]
        //             {
        //                 new RejectedChange
        //                 {
        //                     AddRoadNode = addRoadNode,
        //                     Reasons = new []
        //                     {
        //                         new Reason
        //                         {
        //                             Because = "RoadNodeIdTaken",
        //                             Parameters = new ReasonParameter[0]
        //                         }
        //                     }
        //                 }
        //             }
        //         })
        //     );
        // }

        // [Fact]
        // public Task when_adding_a_node_with_a_geometry_that_is_not_a_point()
        // {
        //     var geometry = Fixture.Create<MultiLineString>().ToBytes();
        //     return Run(scenario => scenario
        //         .GivenNone()
        //         .When(TheOperator.ChangesTheRoadNetwork(
        //             new RequestedChange
        //             {
        //                 AddRoadNode = new Messages.AddRoadNode
        //                 {
        //                     Id = 1,
        //                     Type = Messages.RoadNodeType.FakeNode,
        //                     Geometry = geometry
        //                 }
        //             }
        //         ))
        //         .Throws(new ValidationException(
        //             new []
        //             {
        //                 new ValidationFailure("Changes[0].AddRoadNode.Geometry", "The 'Geometry' is not a PointM.")
        //             })));
        // }

        // [Fact]
        // public Task when_adding_a_node_with_a_geometry_that_has_been_taken()
        // {
        //     var geometry = Fixture.Create<PointM>();
        //     var addRoadNode = new Messages.AddRoadNode
        //     {
        //         Id = 2,
        //         Type = Messages.RoadNodeType.FakeNode,
        //         Geometry = geometry.ToBytes()
        //     };
        //     return Run(scenario => scenario
        //         .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        //         {
        //             Changes = new[]
        //             {
        //                 new AcceptedChange
        //                 {
        //                     RoadNodeAdded = new RoadNodeAdded
        //                     {
        //                         Id = 1,
        //                         Type = Messages.RoadNodeType.RealNode,
        //                         Geometry = geometry.ToBytes()
        //                     }
        //                 }
        //             }
        //         })
        //         .When(TheOperator.ChangesTheRoadNetwork(
        //             new RequestedChange
        //             {
        //                 AddRoadNode = addRoadNode
        //             }
        //         ))
        //         .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
        //         {
        //             Changes = new[]
        //             {
        //                 new RejectedChange
        //                 {
        //                     AddRoadNode = addRoadNode,
        //                     Reasons = new []
        //                     {
        //                         new Reason
        //                         {
        //                             Because = "RoadNodeGeometryTaken",
        //                             Parameters = new[]
        //                             {
        //                                 new ReasonParameter
        //                                 {
        //                                     Name = "ByOtherNode",
        //                                     Value = "1"
        //                                 }
        //                             }
        //                         }
        //                     }
        //                 }
        //             }
        //         })
        //     );
        // }

        // [Fact]
        // public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
        // {
        //     var geometry1 = Fixture.Create<PointM>().ToBytes();
        //     var geometry2 = Fixture.Create<PointM>().ToBytes();
        //     return Run(scenario => scenario
        //         .GivenNone()
        //         .When(TheOperator.ChangesTheRoadNetwork(
        //             new RequestedChange
        //             {
        //                 AddRoadNode = new Messages.AddRoadNode
        //                 {
        //                     Id = 1,
        //                     Type = Messages.RoadNodeType.FakeNode,
        //                     Geometry = geometry1
        //                 }
        //             },
        //             new RequestedChange
        //             {
        //                 AddRoadNode = new Messages.AddRoadNode
        //                 {
        //                     Id = 2,
        //                     Type = Messages.RoadNodeType.FakeNode,
        //                     Geometry = geometry2
        //                 }
        //             }
        //         ))
        //         .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        //         {
        //             Changes = new[]
        //             {
        //                 new AcceptedChange
        //                 {
        //                     RoadNodeAdded = new RoadNodeAdded
        //                     {
        //                         Id = 1,
        //                         Type = Messages.RoadNodeType.FakeNode,
        //                         Geometry = geometry1
        //                     }
        //                 },
        //                 new AcceptedChange
        //                 {
        //                     RoadNodeAdded = new RoadNodeAdded
        //                     {
        //                         Id = 2,
        //                         Type = Messages.RoadNodeType.FakeNode,
        //                         Geometry = geometry2
        //                     }
        //                 }
        //             }
        //         }));
        // }

        // [Fact]
        // public Task when_adding_a_node_that_is_within_two_meters_of_another_node()
        // {
        //     var geometry1 = Fixture.Create<PointM>();
        //     var random = new Random();
        //     var geometry2 = new PointM(
        //         geometry1.X + random.NextDouble() * RoadNetwork.TooCloseDistance,
        //         geometry1.Y + random.NextDouble() * RoadNetwork.TooCloseDistance,
        //         geometry1.Z + random.NextDouble() * RoadNetwork.TooCloseDistance
        //     );
        //     var addRoadNode = new Messages.AddRoadNode
        //     {
        //         Id = 2,
        //         Type = Messages.RoadNodeType.FakeNode,
        //         Geometry = geometry2.ToBytes()
        //     };
        //     return Run(scenario => scenario
        //         .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        //         {
        //             Changes = new[]
        //             {
        //                 new AcceptedChange
        //                 {
        //                     RoadNodeAdded = new RoadNodeAdded
        //                     {
        //                         Id = 1,
        //                         Type = Messages.RoadNodeType.RealNode,
        //                         Geometry = geometry1.ToBytes()
        //                     }
        //                 }
        //             }
        //         })
        //         .When(TheOperator.ChangesTheRoadNetwork(
        //             new RequestedChange
        //             {
        //                 AddRoadNode = addRoadNode
        //             }
        //         ))
        //         .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
        //         {
        //             Changes = new[]
        //             {
        //                 new RejectedChange
        //                 {
        //                     AddRoadNode = addRoadNode,
        //                     Reasons = new []
        //                     {
        //                         new Reason
        //                         {
        //                             Because = "RoadNodeTooClose",
        //                             Parameters = new[]
        //                             {
        //                                 new ReasonParameter
        //                                 {
        //                                     Name = "ToOtherNode",
        //                                     Value = "1"
        //                                 }
        //                             }
        //                         }
        //                     }
        //                 }
        //             }
        //         })
        //     );
        // }
    }
}
