namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Aiv.Vbr.Shaperon;
    using GeoAPI.Geometries;
    using Testing;
    using Xunit;
    using NetTopologySuite.Geometries;
    using LineString = NetTopologySuite.Geometries.LineString;

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
                Geometry = GeometryTranslator.Translate(MultiLineString1),
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
                        AttributeId = europeanRoadCount + index + 1,
                        RoadNumber = part.RoadNumber
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
                PartOfEuropeanRoads = AddSegment3.PartOfEuropeanRoads
                    .Select((part, index) => new Messages.RoadSegmentEuropeanRoadAttributes
                    {
                        AttributeId = europeanRoadCount * 2 + index + 1,
                        RoadNumber = part.RoadNumber
                    })
                    .ToArray(),
                PartOfNationalRoads = AddSegment3.PartOfNationalRoads
                    .Select((part, index) => new Messages.RoadSegmentNationalRoadAttributes
                    {
                        AttributeId = nationalRoadCount * 2 + index + 1,
                        Ident2 = part.Ident2
                    })
                    .ToArray(),
                PartOfNumberedRoads = AddSegment3.PartOfNumberedRoads
                    .Select((part, index) => new Messages.RoadSegmentNumberedRoadAttributes
                    {
                        AttributeId = numberedRoadCount * 2 + index + 1,
                        Ident8 = part.Ident8,
                        Direction = part.Direction,
                        Ordinal = part.Ordinal
                    })
                    .ToArray(),
                Lanes = AddSegment3.Lanes
                    .Select((lane, index) => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = laneCount * 2 + index + 1,
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
                        AttributeId = widthCount * 2 + index + 1,
                        Width = width.Width,
                        FromPosition = width.FromPosition,
                        ToPosition = width.ToPosition,
                        AsOfGeometryVersion = 1
                    })
                    .ToArray(),
                Surfaces = AddSegment3.Surfaces
                    .Select((surface, index) => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = surfaceCount * 2 + index + 1,
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeNotConnectedToAnySegment",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "FakeNode"
                                        },
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "TurningLoopNode"
                                        }
                                    }
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact(Skip = "Need to complete attribute hash and hash code testing first.")]
        public Task when_adding_a_start_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
        {
            AddStartNode1.Type = RoadNodeType.FakeNode.ToString();

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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                    Parameters = new []
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "RoadSegmentId",
                                            Value = "1"
                                        },
                                        new Messages.ReasonParameter
                                        {
                                            Name = "RoadSegmentId",
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "FakeNode"
                                        },
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "TurningLoopNode"
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
        public Task when_adding_a_start_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
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
            var endPoint3 = new PointM(0.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            AddEndNode3.Geometry = GeometryTranslator.Translate(endPoint3);
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "RealNode"
                                        },
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "MiniRoundabout"
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
        public Task when_adding_an_end_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
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
            var startPoint3 = new PointM(0.0, 0.0)
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint1);
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
            AddStartNode3.Geometry = GeometryTranslator.Translate(startPoint3);
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "RealNode"
                                        },
                                        new Messages.ReasonParameter
                                        {
                                            Name = "Expected",
                                            Value = "MiniRoundabout"
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
        public Task when_adding_a_start_node_with_a_geometry_that_has_been_taken()
        {
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
            var random = new Random();
            var startPoint = new PointM(
                StartPoint1.X + random.NextDouble() * RoadNetwork.TooCloseDistance,
                StartPoint1.Y + random.NextDouble() * RoadNetwork.TooCloseDistance
            );
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
            var random = new Random();
            var endPoint = new PointM(
                EndPoint1.X + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                EndPoint1.Y + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance,
                EndPoint1.Z + random.NextDouble() / 2.0 * RoadNetwork.TooCloseDistance
            );
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadSegmentStartNodeMissing",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadSegmentEndNodeMissing",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
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
                                    Because = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
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
                                    Because = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
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
                            Reasons = new[]
                            {
                                new Messages.Reason
                                {
                                    Because = "RoadSegmentGeometryLengthIsZero",
                                    Parameters = new Messages.ReasonParameter[0]
                                }
                            }
                        }
                    }
                }));
        }
    }
}
