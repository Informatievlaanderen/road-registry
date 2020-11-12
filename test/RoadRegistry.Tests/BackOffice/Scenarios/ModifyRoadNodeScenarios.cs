namespace RoadRegistry.BackOffice.Scenarios
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Messages;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NodaTime.Text;
    using Core;
    using RoadRegistry.Framework.Testing;
    using Xunit;

    public class ModifyRoadNodeScenarios : RoadRegistryFixture
    {
        public ModifyRoadNodeScenarios()
        {
            Fixture.CustomizePoint();
            Fixture.CustomizePolylineM();

            Fixture.CustomizeAttributeId();
            Fixture.CustomizeOrganizationId();
            Fixture.CustomizeOrganizationName();
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
            Fixture.CustomizeGradeSeparatedJunctionId();
            Fixture.CustomizeGradeSeparatedJunctionType();
            Fixture.CustomizeArchiveId();
            Fixture.CustomizeChangeRequestId();
            Fixture.CustomizeReason();
            Fixture.CustomizeOperatorName();
            Fixture.CustomizeTransactionId();

            Fixture.Customize<RoadSegmentEuropeanRoadAttributes>(composer =>
                composer.Do(instance =>
                    {
                        instance.AttributeId = Fixture.Create<AttributeId>();
                        instance.Number = Fixture.Create<EuropeanRoadNumber>();
                    })
                    .OmitAutoProperties());
            Fixture.Customize<RoadSegmentNationalRoadAttributes>(composer =>
                composer.Do(instance =>
                    {
                        instance.AttributeId = Fixture.Create<AttributeId>();
                        instance.Number = Fixture.Create<NationalRoadNumber>();
                    })
                    .OmitAutoProperties());
            Fixture.Customize<RoadSegmentNumberedRoadAttributes>(composer =>
                composer.Do(instance =>
                {
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.Number = Fixture.Create<NumberedRoadNumber>();
                    instance.Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>();
                    instance.Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentLaneAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Count = Fixture.Create<RoadSegmentLaneCount>();
                    instance.Direction = Fixture.Create<RoadSegmentLaneDirection>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentWidthAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Width = Fixture.Create<RoadSegmentWidth>();
                }).OmitAutoProperties());
            Fixture.Customize<RequestedRoadSegmentSurfaceAttribute>(composer =>
                composer.Do(instance =>
                {
                    var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
                    instance.AttributeId = Fixture.Create<AttributeId>();
                    instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                    instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                    instance.Type = Fixture.Create<RoadSegmentSurfaceType>();
                }).OmitAutoProperties());

            StartPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MiddlePoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(50.0, 50.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            EndPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(100.0, 100.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MultiLineString1 = new MultiLineString(
                new []
                {
                    new NetTopologySuite.Geometries.LineString(
                        new CoordinateArraySequence(new [] { StartPoint1.Coordinate, MiddlePoint1.Coordinate, EndPoint1.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

            StartPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 200.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MiddlePoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(50.0, 250.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            EndPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(100.0, 300.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MultiLineString2 = new MultiLineString(
                new []
                {
                    new NetTopologySuite.Geometries.LineString(
                        new CoordinateArraySequence(new [] { StartPoint2.Coordinate, MiddlePoint2.Coordinate, EndPoint2.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

            StartPoint3 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 500.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MiddlePoint3 = new NetTopologySuite.Geometries.Point(new CoordinateM(50.0, 550.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            EndPoint3 = new NetTopologySuite.Geometries.Point(new CoordinateM(100.0, 600.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
            MultiLineString3 = new MultiLineString(
                new []
                {
                    new NetTopologySuite.Geometries.LineString(
                        new CoordinateArraySequence(new [] { StartPoint3.Coordinate, MiddlePoint3.Coordinate, EndPoint3.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
                }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

            AddStartNode1 = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Geometry = Core.GeometryTranslator.Translate(StartPoint1),
                Type = RoadNodeType.EndNode
            };

            StartNode1Added = new RoadNodeAdded
            {
                Id = 1,
                TemporaryId = AddStartNode1.TemporaryId,
                Geometry = AddStartNode1.Geometry,
                Type = AddStartNode1.Type
            };

            ModifyStartNode1 = new Messages.ModifyRoadNode
            {
                Id = 1,
                Geometry = Core.GeometryTranslator.Translate(StartPoint1),
                Type = RoadNodeType.EndNode
            };

            StartNode1Modified = new RoadNodeModified
            {
                Id = 1,
                Geometry = AddStartNode1.Geometry,
                Type = AddStartNode1.Type
            };

            AddEndNode1 = new Messages.AddRoadNode
            {
                TemporaryId = AddStartNode1.TemporaryId + 1,
                Geometry = Core.GeometryTranslator.Translate(EndPoint1),
                Type = RoadNodeType.EndNode
            };

            EndNode1Added = new RoadNodeAdded
            {
                Id = 2,
                TemporaryId = AddEndNode1.TemporaryId,
                Geometry = AddEndNode1.Geometry,
                Type = AddEndNode1.Type
            };

            ModifyEndNode1 = new Messages.ModifyRoadNode
            {
                Id = 2,
                Geometry = Core.GeometryTranslator.Translate(StartPoint2),
                Type = RoadNodeType.EndNode
            };

            EndNode1Modified = new RoadNodeModified
            {
                Id = 2,
                Geometry = ModifyEndNode1.Geometry,
                Type = ModifyEndNode1.Type
            };

            AddStartNode2 = new Messages.AddRoadNode
            {
                TemporaryId = AddEndNode1.TemporaryId + 1,
                Geometry = Core.GeometryTranslator.Translate(StartPoint2),
                Type = RoadNodeType.EndNode
            };

            StartNode2Added = new RoadNodeAdded
            {
                Id = 3,
                TemporaryId = AddStartNode2.TemporaryId,
                Geometry = AddStartNode2.Geometry,
                Type = AddStartNode2.Type
            };

            AddEndNode2 = new Messages.AddRoadNode
            {
                TemporaryId = AddStartNode2.TemporaryId + 1,
                Geometry = Core.GeometryTranslator.Translate(EndPoint2),
                Type = RoadNodeType.EndNode
            };

            EndNode2Added = new RoadNodeAdded
            {
                Id = 4,
                TemporaryId = AddEndNode2.TemporaryId,
                Geometry = AddEndNode2.Geometry,
                Type = AddEndNode2.Type
            };

            AddStartNode3 = new Messages.AddRoadNode
            {
                TemporaryId = AddEndNode2.TemporaryId + 1,
                Geometry = Core.GeometryTranslator.Translate(StartPoint3),
                Type = RoadNodeType.EndNode
            };

            StartNode3Added = new RoadNodeAdded
            {
                Id = 5,
                TemporaryId = AddStartNode3.TemporaryId,
                Geometry = AddStartNode3.Geometry,
                Type = AddStartNode3.Type
            };

            AddEndNode3 = new Messages.AddRoadNode
            {
                TemporaryId = AddStartNode3.TemporaryId + 1,
                Geometry = Core.GeometryTranslator.Translate(EndPoint3),
                Type = RoadNodeType.EndNode
            };

            EndNode3Added = new RoadNodeAdded
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
                Geometry = Core.GeometryTranslator.Translate(MultiLineString1),
                MaintenanceAuthority = Fixture.Create<OrganizationId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                Lanes = Fixture
                    .CreateMany<RequestedRoadSegmentLaneAttribute>(laneCount1)
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
                    .CreateMany<RequestedRoadSegmentWidthAttribute>(widthCount1)
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
                    .CreateMany<RequestedRoadSegmentSurfaceAttribute>(surfaceCount1)
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

            Segment1Added = new RoadSegmentAdded
            {
                Id = 1,
                TemporaryId = AddSegment1.TemporaryId,
                StartNodeId = 1,
                EndNodeId = 2,
                Geometry = AddSegment1.Geometry,
                GeometryVersion = 0,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = AddSegment1.MaintenanceAuthority,
                    Name = null
                },
                GeometryDrawMethod = AddSegment1.GeometryDrawMethod,
                Morphology = AddSegment1.Morphology,
                Status = AddSegment1.Status,
                Category = AddSegment1.Category,
                AccessRestriction = AddSegment1.AccessRestriction,
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment1.LeftSideStreetNameId
                },
                RightSide = new RoadSegmentSideAttributes
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
                Geometry = Core.GeometryTranslator.Translate(MultiLineString2),
                MaintenanceAuthority = Fixture.Create<OrganizationId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                Lanes = Fixture
                    .CreateMany<RequestedRoadSegmentLaneAttribute>(laneCount2)
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
                    .CreateMany<RequestedRoadSegmentWidthAttribute>(widthCount2)
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
                    .CreateMany<RequestedRoadSegmentSurfaceAttribute>(surfaceCount2)
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

            Segment2Added = new RoadSegmentAdded
            {
                Id = 2,
                TemporaryId = AddSegment2.TemporaryId,
                StartNodeId = 3,
                EndNodeId = 4,
                Geometry = AddSegment2.Geometry,
                GeometryVersion = 0,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = AddSegment2.MaintenanceAuthority,
                    Name = null
                },
                GeometryDrawMethod = AddSegment2.GeometryDrawMethod,
                Morphology = AddSegment2.Morphology,
                Status = AddSegment2.Status,
                Category = AddSegment2.Category,
                AccessRestriction = AddSegment2.AccessRestriction,
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment2.LeftSideStreetNameId
                },
                RightSide = new RoadSegmentSideAttributes
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
                Geometry = Core.GeometryTranslator.Translate(MultiLineString3),
                MaintenanceAuthority = Fixture.Create<OrganizationId>(),
                GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = Fixture.Create<RoadSegmentMorphology>(),
                Status = Fixture.Create<RoadSegmentStatus>(),
                Category = Fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = Fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSideStreetNameId = Fixture.Create<int?>(),
                RightSideStreetNameId = Fixture.Create<int?>(),
                Lanes = Fixture
                    .CreateMany<RequestedRoadSegmentLaneAttribute>(laneCount3)
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
                    .CreateMany<RequestedRoadSegmentWidthAttribute>(widthCount3)
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
                    .CreateMany<RequestedRoadSegmentSurfaceAttribute>(surfaceCount3)
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

            Segment3Added = new RoadSegmentAdded
            {
                Id = 3,
                TemporaryId = AddSegment3.TemporaryId,
                StartNodeId = 5,
                EndNodeId = 6,
                Geometry = AddSegment3.Geometry,
                GeometryVersion = 0,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = AddSegment3.MaintenanceAuthority,
                    Name = null
                },
                GeometryDrawMethod = AddSegment3.GeometryDrawMethod,
                Morphology = AddSegment3.Morphology,
                Status = AddSegment3.Status,
                Category = AddSegment3.Category,
                AccessRestriction = AddSegment3.AccessRestriction,
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = AddSegment3.LeftSideStreetNameId
                },
                RightSide = new RoadSegmentSideAttributes
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

            ArchiveId = Fixture.Create<ArchiveId>();
            RequestId = ChangeRequestId.FromArchiveId(ArchiveId);
            ReasonForChange = Fixture.Create<Reason>();
            ChangedByOperator = Fixture.Create<OperatorName>();
            ChangedByOrganization = Fixture.Create<OrganizationId>();
            ChangedByOrganizationName = Fixture.Create<OrganizationName>();
            TransactionId = Fixture.Create<TransactionId>();
        }

        public ArchiveId ArchiveId { get; }
        public ChangeRequestId RequestId { get; }
        public Reason ReasonForChange { get; }
        public OperatorName ChangedByOperator { get; }
        public OrganizationId ChangedByOrganization { get; }
        public OrganizationName ChangedByOrganizationName { get; }
        public TransactionId TransactionId { get; }

        public NetTopologySuite.Geometries.Point StartPoint1 { get; }
        public NetTopologySuite.Geometries.Point MiddlePoint1 { get; }
        public NetTopologySuite.Geometries.Point EndPoint1 { get; }
        public MultiLineString MultiLineString1 { get; }

        public NetTopologySuite.Geometries.Point StartPoint2 { get; }
        public NetTopologySuite.Geometries.Point MiddlePoint2 { get; }
        public NetTopologySuite.Geometries.Point EndPoint2 { get; }
        public MultiLineString MultiLineString2 { get; }

        public NetTopologySuite.Geometries.Point StartPoint3 { get; }
        public NetTopologySuite.Geometries.Point MiddlePoint3 { get; }
        public NetTopologySuite.Geometries.Point EndPoint3 { get; }
        public MultiLineString MultiLineString3 { get; }

        public Messages.AddRoadNode AddStartNode1 { get; }
        public Messages.AddRoadNode AddEndNode1 { get; }
        public Messages.AddRoadSegment AddSegment1 { get; }

        public Messages.ModifyRoadNode ModifyStartNode1 { get; }
        public Messages.RoadNodeModified StartNode1Modified { get; }

        public Messages.ModifyRoadNode ModifyEndNode1 { get; }
        public Messages.RoadNodeModified EndNode1Modified { get; }

        public RoadNodeAdded StartNode1Added { get; }
        public RoadNodeAdded EndNode1Added { get; }
        public RoadSegmentAdded Segment1Added { get; }

        public Messages.AddRoadNode AddStartNode2 { get; }
        public Messages.AddRoadNode AddEndNode2 { get; }
        public Messages.AddRoadSegment AddSegment2 { get; }

        public RoadNodeAdded StartNode2Added { get; }
        public RoadNodeAdded EndNode2Added { get; }
        public RoadSegmentAdded Segment2Added { get; }

        public Messages.AddRoadNode AddStartNode3 { get; }
        public Messages.AddRoadNode AddEndNode3 { get; }
        public Messages.AddRoadSegment AddSegment3 { get; }

        public RoadNodeAdded StartNode3Added { get; }
        public RoadNodeAdded EndNode3Added { get; }
        public RoadSegmentAdded Segment3Added { get; }

        [Fact]
        public Task when_modifying_a_node_with_the_same_values()
        {
            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeModified = StartNode1Modified,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_modifying_a_start_node_connected_to_a_single_segment_to_a_type_other_than_end_node()
        {
            ModifyStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.EndNode)
                .ToString();
            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            ModifyRoadNode = ModifyStartNode1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = ModifyStartNode1.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "1"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = ModifyStartNode1.Type
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_modifying_an_end_node_connected_to_a_single_segment_to_a_type_other_than_end_node()
        {
            ModifyEndNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.EndNode)
                .ToString();
            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyEndNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            ModifyRoadNode = ModifyEndNode1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = ModifyEndNode1.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "1"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = ModifyEndNode1.Type
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_modifying_a_start_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            ModifyStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
                .ToString();
            ModifyStartNode1.Geometry = Core.GeometryTranslator.Translate(startPoint);

            StartNode1Added.Type = RoadNodeType.FakeNode;
            StartNode1Added.Geometry = Core.GeometryTranslator.Translate(startPoint);
            EndNode1Added.Type = RoadNodeType.EndNode;
            EndNode1Added.Geometry = Core.GeometryTranslator.Translate(endPoint1);
            EndNode2Added.Type = RoadNodeType.EndNode;
            EndNode2Added.Geometry = Core.GeometryTranslator.Translate(endPoint2);
            Segment1Added.StartNodeId = StartNode1Added.Id;
            Segment1Added.EndNodeId = EndNode1Added.Id;
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment1Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.StartNodeId = StartNode1Added.Id;
            Segment2Added.EndNodeId = EndNode2Added.Id;
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment2Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode2Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment2Added, Problems = new Messages.Problem[0]
                            }
                        }
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            ModifyRoadNode = ModifyStartNode1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = ModifyStartNode1.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = ModifyStartNode1.Type
                                        },
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
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_modifying_a_start_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            ModifyStartNode1.Type = RoadNodeType.FakeNode;
            ModifyStartNode1.Geometry = Core.GeometryTranslator.Translate(startPoint);

            StartNode1Added.Type = RoadNodeType.FakeNode;
            StartNode1Added.Geometry = Core.GeometryTranslator.Translate(startPoint);
            EndNode1Added.Type = RoadNodeType.EndNode;
            EndNode1Added.Geometry = Core.GeometryTranslator.Translate(endPoint1);
            EndNode2Added.Type = RoadNodeType.EndNode;
            EndNode2Added.Geometry = Core.GeometryTranslator.Translate(endPoint2);
            Segment1Added.StartNodeId = StartNode1Added.Id;
            Segment1Added.EndNodeId = EndNode1Added.Id;
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment1Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.StartNodeId = StartNode1Added.Id;
            Segment2Added.EndNodeId = EndNode2Added.Id;
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment2Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.Status = Segment1Added.Status;
            Segment2Added.Morphology = Segment1Added.Morphology;
            Segment2Added.Category = Segment1Added.Category;
            Segment2Added.MaintenanceAuthority.Code = Segment1Added.MaintenanceAuthority.Code;
            Segment2Added.MaintenanceAuthority.Name = Segment1Added.MaintenanceAuthority.Name;
            Segment2Added.AccessRestriction = Segment1Added.AccessRestriction;
            Segment2Added.LeftSide.StreetNameId = Segment1Added.LeftSide.StreetNameId;
            Segment2Added.RightSide.StreetNameId = Segment1Added.RightSide.StreetNameId;

            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode2Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment2Added, Problems = new Messages.Problem[0]
                            }
                        }
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            ModifyRoadNode = ModifyStartNode1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = ModifyStartNode1.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = Segment1Added.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = Segment2Added.Id.ToString()
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public async Task when_modifying_a_start_node_connecting_two_segments_as_a_fake_node_and_the_segments_differ_by_one_attribute()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            ModifyStartNode1.Type = RoadNodeType.FakeNode;
            ModifyStartNode1.Geometry = Core.GeometryTranslator.Translate(startPoint);
            StartNode1Modified.Type = RoadNodeType.FakeNode;
            StartNode1Modified.Geometry = Core.GeometryTranslator.Translate(startPoint);

            StartNode1Added.Type = RoadNodeType.FakeNode;
            StartNode1Added.Geometry = Core.GeometryTranslator.Translate(startPoint);
            EndNode1Added.Type = RoadNodeType.EndNode;
            EndNode1Added.Geometry = Core.GeometryTranslator.Translate(endPoint1);
            EndNode2Added.Type = RoadNodeType.EndNode;
            EndNode2Added.Geometry = Core.GeometryTranslator.Translate(endPoint2);
            Segment1Added.StartNodeId = StartNode1Added.Id;
            Segment1Added.EndNodeId = EndNode1Added.Id;
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment1Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.StartNodeId = StartNode1Added.Id;
            Segment2Added.EndNodeId = EndNode2Added.Id;
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment2Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.Status = Segment1Added.Status;
            Segment2Added.Morphology = Segment1Added.Morphology;
            Segment2Added.Category = Segment1Added.Category;
            Segment2Added.MaintenanceAuthority.Code = Segment1Added.MaintenanceAuthority.Code;
            Segment2Added.MaintenanceAuthority.Name = Segment1Added.MaintenanceAuthority.Name;
            Segment2Added.AccessRestriction = Segment1Added.AccessRestriction;
            Segment2Added.LeftSide.StreetNameId = Segment1Added.LeftSide.StreetNameId;
            Segment2Added.RightSide.StreetNameId = Segment1Added.RightSide.StreetNameId;

            var choice = new Random().Next(0, 7);
            switch (choice)
            {
                case 0:
                    Segment2Added.Status = new Generator<RoadSegmentStatus>(Fixture)
                        .First(candidate => candidate != RoadSegmentStatus.Parse(Segment1Added.Status));
                    break;
                case 1:
                    Segment2Added.Morphology = new Generator<RoadSegmentMorphology>(Fixture)
                        .First(candidate => candidate != RoadSegmentMorphology.Parse(Segment1Added.Morphology));
                    break;
                case 2:
                    Segment2Added.Category = new Generator<RoadSegmentCategory>(Fixture)
                        .First(candidate => candidate != RoadSegmentCategory.Parse(Segment1Added.Category));
                    break;
                case 3:
                    Segment2Added.MaintenanceAuthority.Code = new Generator<OrganizationId>(Fixture)
                        .First(candidate => candidate != new OrganizationId(Segment1Added.MaintenanceAuthority.Code));
                    break;
                case 4:
                    Segment2Added.AccessRestriction = new Generator<RoadSegmentAccessRestriction>(Fixture)
                        .First(candidate => candidate != RoadSegmentAccessRestriction.Parse(Segment1Added.AccessRestriction));
                    break;
                case 5:
                    Segment2Added.LeftSide.StreetNameId = new Generator<CrabStreetnameId?>(Fixture)
                        .First(candidate => candidate != (Segment1Added.LeftSide.StreetNameId.HasValue
                            ? new CrabStreetnameId(Segment1Added.LeftSide.StreetNameId.Value)
                            : new CrabStreetnameId?()));
                    break;
                case 6:
                    Segment2Added.RightSide.StreetNameId = new Generator<CrabStreetnameId?>(Fixture)
                        .First(candidate => candidate != (Segment1Added.RightSide.StreetNameId.HasValue
                            ? new CrabStreetnameId(Segment1Added.RightSide.StreetNameId.Value)
                            : new CrabStreetnameId?()));
                    break;
            }

            await Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode2Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment2Added, Problems = new Messages.Problem[0]
                            }
                        }
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeModified = StartNode1Modified,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_modifying_an_end_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
        {
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            ModifyEndNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
                .ToString();
            ModifyEndNode1.Geometry = Core.GeometryTranslator.Translate(endPoint);

            StartNode1Added.Type = RoadNodeType.EndNode;
            StartNode1Added.Geometry = Core.GeometryTranslator.Translate(startPoint1);
            StartNode2Added.Type = RoadNodeType.EndNode;
            StartNode2Added.Geometry = Core.GeometryTranslator.Translate(startPoint2);
            EndNode1Added.Type = RoadNodeType.FakeNode;
            EndNode1Added.Geometry = Core.GeometryTranslator.Translate(endPoint);

            Segment1Added.StartNodeId = StartNode1Added.Id;
            Segment1Added.EndNodeId = EndNode1Added.Id;
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment1Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint1.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.StartNodeId = StartNode2Added.Id;
            Segment2Added.EndNodeId = EndNode1Added.Id;
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment2Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint2.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode2Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment2Added, Problems = new Messages.Problem[0]
                            }
                        }
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyEndNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            ModifyRoadNode = ModifyEndNode1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTypeMismatch",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = ModifyEndNode1.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = ModifyEndNode1.Type
                                        },
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
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_modifying_an_end_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
        {
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            ModifyEndNode1.Type = RoadNodeType.FakeNode;
            ModifyEndNode1.Geometry = Core.GeometryTranslator.Translate(endPoint);

            StartNode1Added.Type = RoadNodeType.EndNode;
            StartNode1Added.Geometry = Core.GeometryTranslator.Translate(startPoint1);
            StartNode2Added.Type = RoadNodeType.EndNode;
            StartNode2Added.Geometry = Core.GeometryTranslator.Translate(startPoint2);
            EndNode1Added.Type = RoadNodeType.FakeNode;
            EndNode1Added.Geometry = Core.GeometryTranslator.Translate(endPoint);

            Segment1Added.StartNodeId = StartNode1Added.Id;
            Segment1Added.EndNodeId = EndNode1Added.Id;
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment1Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint1.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.StartNodeId = StartNode2Added.Id;
            Segment2Added.EndNodeId = EndNode1Added.Id;
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment2Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint2.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.Status = Segment1Added.Status;
            Segment2Added.Morphology = Segment1Added.Morphology;
            Segment2Added.Category = Segment1Added.Category;
            Segment2Added.MaintenanceAuthority.Code = Segment1Added.MaintenanceAuthority.Code;
            Segment2Added.MaintenanceAuthority.Name = Segment1Added.MaintenanceAuthority.Name;
            Segment2Added.AccessRestriction = Segment1Added.AccessRestriction;
            Segment2Added.LeftSide.StreetNameId = Segment1Added.LeftSide.StreetNameId;
            Segment2Added.RightSide.StreetNameId = Segment1Added.RightSide.StreetNameId;

            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode2Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment2Added, Problems = new Messages.Problem[0]
                            }
                        }
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyEndNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            ModifyRoadNode = ModifyEndNode1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = ModifyEndNode1.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = Segment1Added.Id.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = Segment2Added.Id.ToString()
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_modifying_an_end_node_connecting_two_segments_as_a_fake_node_and_the_segments_differ_by_one_attribute()
        {
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            ModifyEndNode1.Type = RoadNodeType.FakeNode;
            ModifyEndNode1.Geometry = Core.GeometryTranslator.Translate(endPoint);
            EndNode1Modified.Type = RoadNodeType.FakeNode;
            EndNode1Modified.Geometry = Core.GeometryTranslator.Translate(endPoint);

            StartNode1Added.Type = RoadNodeType.EndNode;
            StartNode1Added.Geometry = Core.GeometryTranslator.Translate(startPoint1);
            StartNode2Added.Type = RoadNodeType.EndNode;
            StartNode2Added.Geometry = Core.GeometryTranslator.Translate(startPoint2);
            EndNode1Added.Type = RoadNodeType.FakeNode;
            EndNode1Added.Geometry = Core.GeometryTranslator.Translate(endPoint);

            Segment1Added.StartNodeId = StartNode1Added.Id;
            Segment1Added.EndNodeId = EndNode1Added.Id;
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment1Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint1.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.StartNodeId = StartNode2Added.Id;
            Segment2Added.EndNodeId = EndNode1Added.Id;
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment2Added.Geometry = Core.GeometryTranslator.Translate(
                new MultiLineString(new[]
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new[]
                    {
                        startPoint2.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment2Added.Status = Segment1Added.Status;
            Segment2Added.Morphology = Segment1Added.Morphology;
            Segment2Added.Category = Segment1Added.Category;
            Segment2Added.MaintenanceAuthority.Code = Segment1Added.MaintenanceAuthority.Code;
            Segment2Added.MaintenanceAuthority.Name = Segment1Added.MaintenanceAuthority.Name;
            Segment2Added.AccessRestriction = Segment1Added.AccessRestriction;
            Segment2Added.LeftSide.StreetNameId = Segment1Added.LeftSide.StreetNameId;
            Segment2Added.RightSide.StreetNameId = Segment1Added.RightSide.StreetNameId;

            switch (new Random().Next(0, 7))
            {
                case 0:
                    Segment2Added.Status = new Generator<RoadSegmentStatus>(Fixture)
                        .First(candidate => candidate != RoadSegmentStatus.Parse(Segment1Added.Status));
                    break;
                case 1:
                    Segment2Added.Morphology = new Generator<RoadSegmentMorphology>(Fixture)
                        .First(candidate => candidate != RoadSegmentMorphology.Parse(Segment1Added.Morphology));
                    break;
                case 2:
                    Segment2Added.Category = new Generator<RoadSegmentCategory>(Fixture)
                        .First(candidate => candidate != RoadSegmentCategory.Parse(Segment1Added.Category));
                    break;
                case 3:
                    Segment2Added.MaintenanceAuthority.Code = new Generator<OrganizationId>(Fixture)
                        .First(candidate => candidate != new OrganizationId(Segment1Added.MaintenanceAuthority.Code));
                    break;
                case 4:
                    Segment2Added.AccessRestriction = new Generator<RoadSegmentAccessRestriction>(Fixture)
                        .First(candidate => candidate != RoadSegmentAccessRestriction.Parse(Segment1Added.AccessRestriction));
                    break;
                case 5:
                    Segment2Added.LeftSide.StreetNameId = new Generator<CrabStreetnameId?>(Fixture)
                        .First(candidate => candidate != (Segment1Added.LeftSide.StreetNameId.HasValue ? new CrabStreetnameId(Segment1Added.LeftSide.StreetNameId.Value) : new CrabStreetnameId?()));
                    break;
                case 6:
                    Segment2Added.RightSide.StreetNameId = new Generator<CrabStreetnameId?>(Fixture)
                        .First(candidate => candidate != (Segment1Added.RightSide.StreetNameId.HasValue ? new CrabStreetnameId(Segment1Added.RightSide.StreetNameId.Value) : new CrabStreetnameId?()));
                    break;
            }

            return Run(scenario => scenario
                .Given(Organizations.StreamNameFactory(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                    {
                        RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                        OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                        TransactionId = new TransactionId(1),
                        Changes = new[]
                        {
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = StartNode2Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadNodeAdded = EndNode1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment1Added, Problems = new Messages.Problem[0]
                            },
                            new Messages.AcceptedChange
                            {
                                RoadSegmentAdded = Segment2Added, Problems = new Messages.Problem[0]
                            }
                        }
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        ModifyRoadNode = ModifyEndNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeModified = EndNode1Modified,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }
    }
}
