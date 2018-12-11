namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Aiv.Vbr.Shaperon;
    using GeoAPI.Geometries;
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

            Fixture.Customize<Messages.RoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                    {
                        instance.AttributeId = Fixture.Create<AttributeId>();
                        instance.RoadNumber = Fixture.Create<EuropeanRoadNumber>();
                    })
                    .OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                    {
                        instance.AttributeId = Fixture.Create<AttributeId>();
                        instance.Ident2 = Fixture.Create<NationalRoadNumber>();
                    })
                    .OmitAutoProperties());
            Fixture.Customize<Messages.RoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Ident8 = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentLaneAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentWidthAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<Messages.RequestedRoadSegmentSurfaceAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
                }).OmitAutoProperties());

            StartPoint1 = new PointM(0.0, 0.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MiddlePoint1 = new PointM(50.0, 50.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            EndPoint1 = new PointM(100.0, 100.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MultiLineString1 = new MultiLineString(
                new ILineString[]
                {
                    new LineString(
                        new PointSequence(new [] { StartPoint1, MiddlePoint1, EndPoint1 }),
                        GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

            StartPoint2 = new PointM(0.0, 200.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MiddlePoint2 = new PointM(50.0, 250.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            EndPoint2 = new PointM(100.0, 300.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MultiLineString2 = new MultiLineString(
                new ILineString[]
                {
                    new LineString(
                        new PointSequence(new [] { StartPoint2, MiddlePoint2, EndPoint2 }),
                        GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

            StartPoint3 = new PointM(0.0, 500.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MiddlePoint3 = new PointM(50.0, 550.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            EndPoint3 = new PointM(100.0, 600.0) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MultiLineString3 = new MultiLineString(
                new ILineString[]
                {
                    new LineString(
                        new PointSequence(new [] { StartPoint3, MiddlePoint3, EndPoint3 }),
                        GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

            AddStartNode1 = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Geometry = GeometryTranslator.Translate(StartPoint1),
                Type = RoadNodeType.EndNode
            };

            StartNode1Added = new Messages.RoadNodeAdded
            {
                Id = 1,
                TemporaryId = AddStartNode1.TemporaryId,
                Geometry = AddStartNode1.Geometry,
                Type = AddStartNode1.Type
            };

            AddEndNode1 = new Messages.AddRoadNode
            {
                TemporaryId = AddStartNode1.TemporaryId + 1,
                Geometry = GeometryTranslator.Translate(EndPoint1),
                Type = RoadNodeType.EndNode
            };

            EndNode1Added = new Messages.RoadNodeAdded
            {
                Id = 2,
                TemporaryId = AddEndNode1.TemporaryId,
                Geometry = AddEndNode1.Geometry,
                Type = AddEndNode1.Type
            };

            AddStartNode2 = new Messages.AddRoadNode
            {
                TemporaryId = AddEndNode1.TemporaryId + 1,
                Geometry = GeometryTranslator.Translate(StartPoint2),
                Type = RoadNodeType.EndNode
            };

            StartNode2Added = new Messages.RoadNodeAdded
            {
                Id = 3,
                TemporaryId = AddStartNode2.TemporaryId,
                Geometry = AddStartNode2.Geometry,
                Type = AddStartNode2.Type
            };

            AddEndNode2 = new Messages.AddRoadNode
            {
                TemporaryId = AddStartNode2.TemporaryId + 1,
                Geometry = GeometryTranslator.Translate(EndPoint2),
                Type = RoadNodeType.EndNode
            };

            EndNode2Added = new Messages.RoadNodeAdded
            {
                Id = 4,
                TemporaryId = AddEndNode2.TemporaryId,
                Geometry = AddEndNode2.Geometry,
                Type = AddEndNode2.Type
            };

            AddStartNode3 = new Messages.AddRoadNode
            {
                TemporaryId = AddEndNode2.TemporaryId + 1,
                Geometry = GeometryTranslator.Translate(StartPoint3),
                Type = RoadNodeType.EndNode
            };

            StartNode3Added = new Messages.RoadNodeAdded
            {
                Id = 5,
                TemporaryId = AddStartNode3.TemporaryId,
                Geometry = AddStartNode3.Geometry,
                Type = AddStartNode3.Type
            };

            AddEndNode3 = new Messages.AddRoadNode
            {
                TemporaryId = AddStartNode3.TemporaryId + 1,
                Geometry = GeometryTranslator.Translate(EndPoint3),
                Type = RoadNodeType.EndNode
            };

            EndNode3Added = new Messages.RoadNodeAdded
            {
                Id = 6,
                TemporaryId = AddEndNode3.TemporaryId,
                Geometry = AddEndNode3.Geometry,
                Type = AddEndNode3.Type
            };

            var laneCount1 = new Random().Next(1, 10);
            var widthCount1 = new Random().Next(1, 10);
            var surfaceCount1 = new Random().Next(1, 10);
            AddSegment1 = new Messages.AddRoadSegment
            {
                TemporaryId = Fixture.Create<RoadSegmentId>(),
                StartNodeId = AddStartNode1.TemporaryId,
                EndNodeId = AddEndNode1.TemporaryId,
                Geometry = GeometryTranslator.Translate(MultiLineString1),
                MaintenanceAuthority = Fixture.Create<MaintenanceAuthorityId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                Lanes = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentLaneAttribute>(laneCount1)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString1.Length) / laneCount1);
                        if (index == laneCount1 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString1.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString1.Length) / laneCount1);
                        }

                        return part;
                    })
                    .ToArray(),
                Widths = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentWidthAttribute>(widthCount1)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString1.Length) / widthCount1);
                        if (index == widthCount1 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString1.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString1.Length) / widthCount1);
                        }

                        return part;
                    })
                    .ToArray(),
                Surfaces = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentSurfaceAttribute>(surfaceCount1)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString1.Length) / surfaceCount1);
                        if (index == surfaceCount1 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString1.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString1.Length) / surfaceCount1);
                        }

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

            var laneCount2 = new Random().Next(1, 10);
            var widthCount2 = new Random().Next(1, 10);
            var surfaceCount2 = new Random().Next(1, 10);
            AddSegment2 = new Messages.AddRoadSegment
            {
                TemporaryId = AddSegment1.TemporaryId + 1,
                StartNodeId = AddStartNode2.TemporaryId,
                EndNodeId = AddEndNode2.TemporaryId,
                Geometry = GeometryTranslator.Translate(MultiLineString2),
                MaintenanceAuthority = Fixture.Create<MaintenanceAuthorityId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                Lanes = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentLaneAttribute>(laneCount2)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString2.Length) / laneCount2);
                        if (index == laneCount2 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString2.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString2.Length) / laneCount2);
                        }

                        return part;
                    })
                    .ToArray(),
                Widths = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentWidthAttribute>(widthCount2)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString2.Length) / widthCount2);
                        if (index == widthCount2 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString2.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString2.Length) / widthCount2);
                        }

                        return part;
                    })
                    .ToArray(),
                Surfaces = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentSurfaceAttribute>(surfaceCount2)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString2.Length) / surfaceCount2);
                        if (index == surfaceCount2 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString2.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString2.Length) / surfaceCount2);
                        }

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
                Lanes = AddSegment2.Lanes
                    .Select((lane, index) => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = laneCount1 + index + 1,
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
                        AttributeId = widthCount1 + index + 1,
                        Width = width.Width,
                        FromPosition = width.FromPosition,
                        ToPosition = width.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Surfaces = AddSegment2.Surfaces
                    .Select((surface, index) => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = surfaceCount1 + index + 1,
                        Type = surface.Type,
                        FromPosition = surface.FromPosition,
                        ToPosition = surface.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Version = 0
            };

            var laneCount3 = new Random().Next(1, 10);
            var widthCount3 = new Random().Next(1, 10);
            var surfaceCount3 = new Random().Next(1, 10);
            AddSegment3 = new Messages.AddRoadSegment
            {
                TemporaryId = AddSegment2.TemporaryId + 1,
                StartNodeId = AddStartNode3.TemporaryId,
                EndNodeId = AddEndNode3.TemporaryId,
                Geometry = GeometryTranslator.Translate(MultiLineString3),
                MaintenanceAuthority = Fixture.Create<MaintenanceAuthorityId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                Lanes = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentLaneAttribute>(laneCount3)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString3.Length) / laneCount3);
                        if (index == laneCount3 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString3.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString3.Length) / laneCount3);
                        }

                        return part;
                    })
                    .ToArray(),
                Widths = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentWidthAttribute>(widthCount3)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString3.Length) / widthCount3);
                        if (index == widthCount3 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString3.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString3.Length) / widthCount3);
                        }

                        return part;
                    })
                    .ToArray(),
                Surfaces = Fixture
                    .CreateMany<Messages.RequestedRoadSegmentSurfaceAttribute>(surfaceCount3)
                    .Select((part, index) =>
                    {
                        part.FromPosition = index * (Convert.ToDecimal(MultiLineString3.Length) / surfaceCount3);
                        if (index == surfaceCount3 - 1)
                        {
                            part.ToPosition = Convert.ToDecimal(MultiLineString3.Length);
                        }
                        else
                        {
                            part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString3.Length) / surfaceCount3);
                        }

                        return part;
                    })
                    .ToArray()
            };

            Segment3Added = new Messages.RoadSegmentAdded
            {
                Id = 3,
                TemporaryId = AddSegment3.TemporaryId,
                StartNodeId = 5,
                EndNodeId = 6,
                Geometry = AddSegment3.Geometry,
                GeometryVersion = 0,
                MaintenanceAuthority = new Messages.MaintenanceAuthority
                {
                    Code = AddSegment3.MaintenanceAuthority,
                    Name = null
                },
                GeometryDrawMethod = AddSegment3.GeometryDrawMethod,
                Morphology = AddSegment3.Morphology,
                Status = AddSegment3.Status,
                Category = AddSegment3.Category,
                AccessRestriction = AddSegment3.AccessRestriction,
                LeftSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment3.LeftSideStreetNameId
                },
                RightSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment3.RightSideStreetNameId
                },
                Lanes = AddSegment3.Lanes
                    .Select((lane, index) => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = laneCount1 + laneCount2 + index + 1,
                        Direction = lane.Direction,
                        Count = lane.Count,
                        FromPosition = lane.FromPosition,
                        ToPosition = lane.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Widths = AddSegment3.Widths
                    .Select((width, index) => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = widthCount1 + widthCount2 + index + 1,
                        Width = width.Width,
                        FromPosition = width.FromPosition,
                        ToPosition = width.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Surfaces = AddSegment3.Surfaces
                    .Select((surface, index) => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = surfaceCount1 + surfaceCount2 + index + 1,
                        Type = surface.Type,
                        FromPosition = surface.FromPosition,
                        ToPosition = surface.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Version = 0
            };
        }


        public PointM StartPoint1 { get; }
        public PointM MiddlePoint1 { get; }
        public PointM EndPoint1 { get; }
        public MultiLineString MultiLineString1 { get; }

        public PointM StartPoint2 { get; }
        public PointM MiddlePoint2 { get; }
        public PointM EndPoint2 { get; }
        public MultiLineString MultiLineString2 { get; }

        public PointM StartPoint3 { get; }
        public PointM MiddlePoint3 { get; }
        public PointM EndPoint3 { get; }
        public MultiLineString MultiLineString3 { get; }

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

        public Messages.AddRoadNode AddStartNode3 { get; }
        public Messages.AddRoadNode AddEndNode3 { get; }
        public Messages.AddRoadSegment AddSegment3 { get; }

        public Messages.RoadNodeAdded StartNode3Added { get; }
        public Messages.RoadNodeAdded EndNode3Added { get; }
        public Messages.RoadSegmentAdded Segment3Added { get; }

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
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_disconnected_node()
        {
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeNotConnectedToAnySegment",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_connected_to_a_single_segment_as_a_node_other_than_an_end_node()
        {
            AddStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.EndNode)
                .ToString();

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_an_end_node_connected_to_a_single_segment_as_a_node_other_than_an_end_node()
        {
            AddEndNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.EndNode)
                .ToString();

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
        {
            AddStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
                .ToString();

            var startPoint = new PointM(10.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new PointM(10.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new PointM(20.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            AddSegment1.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint1
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint2
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

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
                            AddRoadNode = AddStartNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "FakeNode"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "TurningLoopNode"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
        {
            var startPoint = new PointM(10.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new PointM(10.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new PointM(20.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            AddSegment1.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint1
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint2
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            AddStartNode1.Type = RoadNodeType.FakeNode.ToString();
            AddSegment2.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment2.Status = AddSegment1.Status;
            AddSegment2.Morphology = AddSegment1.Morphology;
            AddSegment2.Category = AddSegment1.Category;
            AddSegment2.MaintenanceAuthority = AddSegment1.MaintenanceAuthority;
            AddSegment2.AccessRestriction = AddSegment1.AccessRestriction;
            AddSegment2.LeftSideStreetNameId = AddSegment1.LeftSideStreetNameId;
            AddSegment2.RightSideStreetNameId = AddSegment1.RightSideStreetNameId;

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
                            AddRoadNode = AddStartNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_connecting_two_segments_as_a_fake_node_and_the_segments_differ_by_one_attribute()
        {
            var startPoint = new PointM(10.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new PointM(10.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new PointM(20.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            StartNode1Added.Geometry = AddStartNode1.Geometry;
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            EndNode1Added.Geometry = AddEndNode1.Geometry;
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            EndNode2Added.Geometry = AddEndNode2.Geometry;
            AddSegment1.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint1
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment1Added.Geometry = AddSegment1.Geometry;
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint2
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.Geometry = AddSegment2.Geometry;
            AddStartNode1.Type = RoadNodeType.FakeNode.ToString();
            StartNode1Added.Type = AddStartNode1.Type;
            AddSegment2.StartNodeId = AddStartNode1.TemporaryId;
            Segment2Added.StartNodeId = StartNode1Added.Id;
            EndNode2Added.Id = 3;
            Segment2Added.EndNodeId = EndNode2Added.Id;

            foreach (var index in Enumerable.Range(0, 7).Except(new[] {new Random().Next(0, 7)}))
            {
                switch (index)
                {
                    case 0:
                        AddSegment2.Status = AddSegment1.Status;
                        Segment2Added.Status = AddSegment1.Status;
                        break;
                    case 1:
                        AddSegment2.Morphology = AddSegment1.Morphology;
                        Segment2Added.Morphology = AddSegment1.Morphology;
                        break;
                    case 2:
                        AddSegment2.Category = AddSegment1.Category;
                        Segment2Added.Category = AddSegment1.Category;
                        break;
                    case 3:
                        AddSegment2.MaintenanceAuthority = AddSegment1.MaintenanceAuthority;
                        Segment2Added.MaintenanceAuthority.Code = AddSegment1.MaintenanceAuthority;
                        break;
                    case 4:
                        AddSegment2.AccessRestriction = AddSegment1.AccessRestriction;
                        Segment2Added.AccessRestriction = AddSegment1.AccessRestriction;
                        break;
                    case 5:
                        AddSegment2.LeftSideStreetNameId = AddSegment1.LeftSideStreetNameId;
                        Segment2Added.LeftSide.StreetNameId = AddSegment1.LeftSideStreetNameId;
                        break;
                    case 6:
                        AddSegment2.RightSideStreetNameId = AddSegment1.RightSideStreetNameId;
                        Segment2Added.RightSide.StreetNameId = AddSegment1.RightSideStreetNameId;
                        break;
                }
            }

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
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added,
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }
        [Fact]
        public Task when_adding_an_end_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
        {
            AddEndNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
                .ToString();

            var endPoint = new PointM(10.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint1 = new PointM(10.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint2 = new PointM(20.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint1);
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
            AddSegment1.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint1, endPoint
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.EndNodeId = AddEndNode1.TemporaryId;
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint2, endPoint
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

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
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "FakeNode"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "TurningLoopNode"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
        {
            AddStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.RealNode && type != RoadNodeType.MiniRoundabout)
                .ToString();

            var startPoint = new PointM(10.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new PointM(10.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new PointM(20.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint3 = new PointM(0.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            AddEndNode3.Geometry = GeometryTranslator.Translate(endPoint3);
            AddSegment1.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint1
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint2
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment3.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment3.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment3Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment3.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment3Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment3.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment3Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment3.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint, endPoint3
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

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
                        AddRoadNode = AddEndNode2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode3
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment3
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "RealNode"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "MiniRoundabout"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_an_end_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
        {
            AddEndNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.RealNode && type != RoadNodeType.MiniRoundabout)
                .ToString();

            var endPoint = new PointM(10.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint1 = new PointM(10.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint2 = new PointM(20.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint3 = new PointM(0.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint1);
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
            AddStartNode3.Geometry = GeometryTranslator.Translate(startPoint3);
            AddSegment1.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint1, endPoint
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.EndNodeId = AddEndNode1.TemporaryId;
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint2, endPoint
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment3.EndNodeId = AddEndNode1.TemporaryId;
            AddSegment3.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment3Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment3.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment3Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment3.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment3Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment3.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new ILineString[]
                {
                    new LineString(new PointSequence(new []
                    {
                        startPoint3, endPoint
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

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
                        AddRoadSegment = AddSegment2
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddStartNode3
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment3
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "RealNode"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "MiniRoundabout"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_with_a_geometry_that_has_been_taken()
        {
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                    new ILineString[]
                    {
                        new LineString(
                            new PointSequence(new[] {StartPoint1, MiddlePoint2, EndPoint2}),
                            GeometryConfiguration.GeometryFactory
                        )
                    }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
            );
            AddStartNode2.Geometry = StartNode1Added.Geometry;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
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
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeGeometryTaken",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ByOtherNode",
                                            Value = "1"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_an_end_node_with_a_geometry_that_has_been_taken()
        {
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                    new ILineString[]
                    {
                        new LineString(
                            new PointSequence(new[] {StartPoint2, MiddlePoint2, EndPoint1}),
                            GeometryConfiguration.GeometryFactory
                        )
                    }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
            );
            AddEndNode2.Geometry = EndNode1Added.Geometry;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
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
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeGeometryTaken",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ByOtherNode",
                                            Value = "2"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
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
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode2Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added,
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact(Skip = "This test should be about being within two meters of another segment")]
        public Task when_adding_a_start_node_that_is_within_two_meters_of_another_node()
        {
            var random = new Random();
            var startPoint = new PointM(
                StartPoint1.X + random.NextDouble() * RoadNetwork.TooCloseDistance,
                StartPoint1.Y + random.NextDouble() * RoadNetwork.TooCloseDistance
            );
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                    new ILineString[]
                    {
                        new LineString(
                            new PointSequence(new[] {startPoint, MiddlePoint2, EndPoint2}),
                            GeometryConfiguration.GeometryFactory
                        )
                    }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
            );
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint);
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
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
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTooClose",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ToOtherNode",
                                            Value = "1"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact(Skip = "This test should be about being within two meters of another segment")]
        public Task when_adding_an_end_node_that_is_within_two_meters_of_another_node()
        {
            var random = new Random();
            var endPoint = new PointM(
                EndPoint1.X + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                EndPoint1.Y + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                EndPoint1.Z + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance
            );
            AddSegment2.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                    new ILineString[]
                    {
                        new LineString(
                            new PointSequence(new[] {StartPoint2, MiddlePoint2, endPoint}),
                            GeometryConfiguration.GeometryFactory
                        )
                    }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
            );
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint);
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
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
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTooClose",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ToOtherNode",
                                            Value = "2"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_changes_are_out_of_order()
        {
            // Permanent identity assignment is influenced by the order in which the change
            // appears in the list of changes (determinism allows for easier testing).
            StartNode1Added.Id = 2;
            EndNode1Added.Id = 1;
            Segment1Added.StartNodeId = 2;
            Segment1Added.EndNodeId = 1;

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
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_segment_with_a_geometry_that_has_been_taken()
        {
            AddSegment2.StartNodeId = StartNode1Added.Id;
            AddSegment2.EndNodeId = EndNode1Added.Id;
            AddSegment2.Geometry = Segment1Added.Geometry;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
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
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentGeometryTaken",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ByOtherSegment",
                                            Value = "1"
                                        }
                                    }
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        public static IEnumerable<object[]> SelfOverlapsCases
        {
            get
            {
                var startPoint1 = new PointM(0.0, 0.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint1 = new PointM(10.0, 0.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endPoint1 = new PointM(5.0, 0.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var multiLineString1 = new MultiLineString(
                    new ILineString[]
                    {
                        new LineString(
                            new PointSequence(new[] { startPoint1, middlePoint1, endPoint1 }),
                            GeometryConfiguration.GeometryFactory
                        ),
                    })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };

                //covers itself
                yield return new object[] {startPoint1, endPoint1, multiLineString1};

                var startPoint2 = new PointM(5.0, 0.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2A = new PointM(20.0, 0.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2B = new PointM(20.0, 10.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2C = new PointM(0.0, 10.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2D = new PointM(0.0, 0.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endPoint2 = new PointM(8.0, 0.0)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var multiLineString2 = new MultiLineString(
                    new ILineString[]
                    {
                        new LineString(
                            new PointSequence(new[] { startPoint2, middlePoint2A, middlePoint2B, middlePoint2C, middlePoint2D, endPoint2 }),
                            GeometryConfiguration.GeometryFactory
                        ),
                    })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };

                //overlaps itself
                yield return new object[] {startPoint2, endPoint2, multiLineString2};
            }
        }

        [Theory]
        [MemberData(nameof(SelfOverlapsCases))]
        public Task when_adding_a_segment_with_a_geometry_that_self_overlaps(
            PointM startPoint,
            PointM endPoint,
            MultiLineString multiLineString)
        {

            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddSegment1.Geometry = GeometryTranslator.Translate(multiLineString);
            AddSegment1.Lanes = AddSegment1.Lanes.Select((part, index) =>
            {
                part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Lanes.Length);
                if (index == AddSegment1.Lanes.Length - 1)
                {
                    part.ToPosition = Convert.ToDecimal(multiLineString.Length);
                }
                else
                {
                    part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Lanes.Length);
                }

                return part;
            }).ToArray();
            Segment1Added.Lanes = AddSegment1.Lanes
                .Select((lane, index) => new Messages.RoadSegmentLaneAttributes
                {
                    AttributeId = index + 1,
                    Direction = lane.Direction,
                    Count = lane.Count,
                    FromPosition = lane.FromPosition,
                    ToPosition = lane.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray();

            AddSegment1.Widths = AddSegment1.Widths.Select((part, index) =>
            {
                part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Widths.Length);
                if (index == AddSegment1.Widths.Length - 1)
                {
                    part.ToPosition = Convert.ToDecimal(multiLineString.Length);
                }
                else
                {
                    part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Widths.Length);
                }

                return part;
            }).ToArray();
            Segment1Added.Widths = AddSegment1.Widths
                .Select((width, index) => new Messages.RoadSegmentWidthAttributes
                {
                    AttributeId = index + 1,
                    Width = width.Width,
                    FromPosition = width.FromPosition,
                    ToPosition = width.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray();

            AddSegment1.Surfaces = AddSegment1.Surfaces.Select((part, index) =>
            {
                part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Surfaces.Length);
                if (index == AddSegment1.Surfaces.Length - 1)
                {
                    part.ToPosition = Convert.ToDecimal(multiLineString.Length);
                }
                else
                {
                    part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Surfaces.Length);
                }

                return part;
            }).ToArray();
            Segment1Added.Surfaces = AddSegment1.Surfaces
                .Select((surface, index) => new Messages.RoadSegmentSurfaceAttributes
                {
                    AttributeId = index + 1,
                    Type = surface.Type,
                    FromPosition = surface.FromPosition,
                    ToPosition = surface.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray();

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentGeometrySelfOverlaps",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_geometry_that_self_intersects()
        {
            var startPoint = new PointM(0.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint1 = new PointM(10.0, 10.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint2 = new PointM(5.0, 20.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint = new PointM(5.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var multiLineString = new MultiLineString(
                new ILineString[]
                {
                    new LineString(
                        new PointSequence(new[] { startPoint, middlePoint1, middlePoint2, endPoint }),
                        GeometryConfiguration.GeometryFactory
                    )
                })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddSegment1.Geometry = GeometryTranslator.Translate(multiLineString);
            AddSegment1.Lanes = AddSegment1.Lanes.Select((part, index) =>
            {
                part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Lanes.Length);
                if (index == AddSegment1.Lanes.Length - 1)
                {
                    part.ToPosition = Convert.ToDecimal(multiLineString.Length);
                }
                else
                {
                    part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Lanes.Length);
                }

                return part;
            }).ToArray();
            Segment1Added.Lanes = AddSegment1.Lanes
                .Select((lane, index) => new Messages.RoadSegmentLaneAttributes
                {
                    AttributeId = index + 1,
                    Direction = lane.Direction,
                    Count = lane.Count,
                    FromPosition = lane.FromPosition,
                    ToPosition = lane.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray();

            AddSegment1.Widths = AddSegment1.Widths.Select((part, index) =>
            {
                part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Widths.Length);
                if (index == AddSegment1.Widths.Length - 1)
                {
                    part.ToPosition = Convert.ToDecimal(multiLineString.Length);
                }
                else
                {
                    part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Widths.Length);
                }

                return part;
            }).ToArray();
            Segment1Added.Widths = AddSegment1.Widths
                .Select((width, index) => new Messages.RoadSegmentWidthAttributes
                {
                    AttributeId = index + 1,
                    Width = width.Width,
                    FromPosition = width.FromPosition,
                    ToPosition = width.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray();

            AddSegment1.Surfaces = AddSegment1.Surfaces.Select((part, index) =>
            {
                part.FromPosition = index * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Surfaces.Length);
                if (index == AddSegment1.Surfaces.Length - 1)
                {
                    part.ToPosition = Convert.ToDecimal(multiLineString.Length);
                }
                else
                {
                    part.ToPosition = (index + 1) * (Convert.ToDecimal(multiLineString.Length) / AddSegment1.Surfaces.Length);
                }

                return part;
            }).ToArray();
            Segment1Added.Surfaces = AddSegment1.Surfaces
                .Select((surface, index) => new Messages.RoadSegmentSurfaceAttributes
                {
                    AttributeId = index + 1,
                    Type = surface.Type,
                    FromPosition = surface.FromPosition,
                    ToPosition = surface.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray();


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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentGeometrySelfIntersects",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_missing_start_node()
        {
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new Messages.RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new Messages.RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentStartNodeMissing",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_missing_end_node()
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
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentEndNodeMissing",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_start_point_does_not_match_its_start_node_geometry()
        {
            AddStartNode1.Geometry = GeometryTranslator.Translate(MiddlePoint1);

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_start_point_does_not_match_its_existing_start_node_geometry()
        {
            AddSegment2.StartNodeId = StartNode1Added.Id;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new []
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
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
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_end_point_does_not_match_its_end_node_geometry()
        {
            AddEndNode1.Geometry = GeometryTranslator.Translate(MiddlePoint1);

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_end_point_does_not_match_its_existing_end_node_geometry()
        {
            AddSegment2.EndNodeId = EndNode1Added.Id;

            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new Messages.RoadNetworkChangesAccepted
                {
                    Changes = new []
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Warnings = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Warnings = new Messages.Problem[0]
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
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_line_string_with_length_0()
        {
            var geometry = new MultiLineString(new ILineString[] {new LineString(new Coordinate[0])})
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddSegment1.Geometry = GeometryTranslator.Translate(geometry);
            AddSegment1.Lanes = new Messages.RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new Messages.RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new Messages.RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentGeometryLengthIsZero",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        public static IEnumerable<object[]> NonAdjacentLaneAttributesCases
        {
            get
            {
                var fixture = new Fixture();
                fixture.CustomizeAttributeId();
                fixture.CustomizeRoadSegmentLaneCount();
                fixture.CustomizeRoadSegmentLaneDirection();

                var attributeId = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentLaneAttribute
                        {
                            AttributeId = attributeId,
                            FromPosition = 2.0m,
                            ToPosition = 100m * Convert.ToDecimal(Math.Sqrt(2.0)),
                            Count = fixture.Create<RoadSegmentLaneCount>(),
                            Direction = fixture.Create<RoadSegmentLaneDirection>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentLaneAttributeFromPositionNotEqualToZero",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId.ToInt32().ToString()
                            }
                        }
                    }
                };

                var attributeId1 = fixture.Create<AttributeId>();
                var attributeId2 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentLaneAttribute
                        {
                            AttributeId = attributeId1,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Count = fixture.Create<RoadSegmentLaneCount>(),
                            Direction = fixture.Create<RoadSegmentLaneDirection>()
                        },
                        new Messages.RequestedRoadSegmentLaneAttribute
                        {
                            AttributeId = attributeId2,
                            FromPosition = 55.0m,
                            ToPosition = 100m * Convert.ToDecimal(Math.Sqrt(2.0)),
                            Count = fixture.Create<RoadSegmentLaneCount>(),
                            Direction = fixture.Create<RoadSegmentLaneDirection>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentLaneAttributesNotAdjacent",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId1.ToInt32().ToString()
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId2.ToInt32().ToString()
                            }
                        }
                    }
                };

                var attributeId3 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentLaneAttribute
                        {
                            AttributeId = attributeId3,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Count = fixture.Create<RoadSegmentLaneCount>(),
                            Direction = fixture.Create<RoadSegmentLaneDirection>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentLaneAttributeToPositionNotEqualToLength",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId3.ToInt32().ToString()
                            },
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonAdjacentLaneAttributesCases))]
        public Task when_adding_a_segment_with_non_adjacent_lane_attributes(Messages.RequestedRoadSegmentLaneAttribute[] attributes, Messages.Problem problem)
        {
            AddSegment1.Lanes = attributes;

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[] { problem },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        public static IEnumerable<object[]> NonAdjacentWidthAttributesCases
        {
            get
            {
                var fixture = new Fixture();
                fixture.CustomizeAttributeId();
                fixture.CustomizeRoadSegmentWidth();

                var attributeId = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentWidthAttribute
                        {
                            AttributeId = attributeId,
                            FromPosition = 2.0m,
                            ToPosition = 100m * Convert.ToDecimal(Math.Sqrt(2.0)),
                            Width = fixture.Create<RoadSegmentWidth>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentWidthAttributeFromPositionNotEqualToZero",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId.ToInt32().ToString()
                            }
                        }
                    }
                };

                var attributeId1 = fixture.Create<AttributeId>();
                var attributeId2 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentWidthAttribute
                        {
                            AttributeId = attributeId1,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Width = fixture.Create<RoadSegmentWidth>()
                        },
                        new Messages.RequestedRoadSegmentWidthAttribute
                        {
                            AttributeId = attributeId2,
                            FromPosition = 55.0m,
                            ToPosition = 100m * Convert.ToDecimal(Math.Sqrt(2.0)),
                            Width = fixture.Create<RoadSegmentWidth>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentWidthAttributesNotAdjacent",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId1.ToInt32().ToString()
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId2.ToInt32().ToString()
                            }
                        }
                    }
                };

                var attributeId3 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentWidthAttribute
                        {
                            AttributeId = attributeId3,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Width = fixture.Create<RoadSegmentWidth>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentWidthAttributeToPositionNotEqualToLength",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId3.ToInt32().ToString()
                            },
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonAdjacentWidthAttributesCases))]
        public Task when_adding_a_segment_with_non_adjacent_width_attributes(Messages.RequestedRoadSegmentWidthAttribute[] attributes, Messages.Problem problem)
        {
            AddSegment1.Widths = attributes;

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[] { problem },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }

        public static IEnumerable<object[]> NonAdjacentSurfaceAttributesCases
        {
            get
            {
                var fixture = new Fixture();
                fixture.CustomizeAttributeId();
                fixture.CustomizeRoadSegmentSurfaceType();

                var attributeId = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentSurfaceAttribute
                        {
                            AttributeId = attributeId,
                            FromPosition = 2.0m,
                            ToPosition = 100m * Convert.ToDecimal(Math.Sqrt(2.0)),
                            Type = fixture.Create<RoadSegmentSurfaceType>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentSurfaceAttributeFromPositionNotEqualToZero",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId.ToInt32().ToString()
                            }
                        }
                    }
                };

                var attributeId1 = fixture.Create<AttributeId>();
                var attributeId2 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentSurfaceAttribute
                        {
                            AttributeId = attributeId1,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Type = fixture.Create<RoadSegmentSurfaceType>()
                        },
                        new Messages.RequestedRoadSegmentSurfaceAttribute
                        {
                            AttributeId = attributeId2,
                            FromPosition = 55.0m,
                            ToPosition = 100m * Convert.ToDecimal(Math.Sqrt(2.0)),
                            Type = fixture.Create<RoadSegmentSurfaceType>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentSurfaceAttributesNotAdjacent",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId1.ToInt32().ToString()
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId2.ToInt32().ToString()
                            }
                        }
                    }
                };

                var attributeId3 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new Messages.RequestedRoadSegmentSurfaceAttribute
                        {
                            AttributeId = attributeId3,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Type = fixture.Create<RoadSegmentSurfaceType>()
                        }
                    },
                    new Messages.Problem
                    {
                        Reason =  "RoadSegmentSurfaceAttributeToPositionNotEqualToLength",
                        Parameters = new []
                        {
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId3.ToInt32().ToString()
                            },
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonAdjacentSurfaceAttributesCases))]
        public Task when_adding_a_segment_with_non_adjacent_surface_attributes(Messages.RequestedRoadSegmentSurfaceAttribute[] attributes, Messages.Problem problem)
        {
            AddSegment1.Surfaces = attributes;

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
                .Then(RoadNetworks.Stream, new Messages.RoadNetworkChangesRejected
                {
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Errors = new[] { problem },
                            Warnings = new Messages.Problem[0]
                        }
                    }
                }));
        }
    }
}
