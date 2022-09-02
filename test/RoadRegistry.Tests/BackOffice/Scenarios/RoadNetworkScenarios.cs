namespace RoadRegistry.BackOffice.Scenarios
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
    using Xunit.Abstractions;
    using GeometryTranslator = BackOffice.GeometryTranslator;

    public class RoadNetworkScenarios : RoadRegistryFixture
    {

        public RoadNetworkScenarios()
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

            ArchiveId = Fixture.Create<ArchiveId>();
            RequestId = ChangeRequestId.FromArchiveId(ArchiveId);
            ReasonForChange = Fixture.Create<Reason>();
            ChangedByOperator = Fixture.Create<OperatorName>();
            ChangedByOrganization = Fixture.Create<OrganizationId>();
            ChangedByOrganizationName = Fixture.Create<OrganizationName>();
            TransactionId = Fixture.Create<TransactionId>();

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
                Geometry = GeometryTranslator.Translate(StartPoint1),
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
                Geometry = GeometryTranslator.Translate(StartPoint1),
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
                Geometry = GeometryTranslator.Translate(EndPoint1),
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
                Geometry = GeometryTranslator.Translate(StartPoint2),
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
                Geometry = GeometryTranslator.Translate(StartPoint2),
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
                Geometry = GeometryTranslator.Translate(EndPoint2),
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
                Geometry = GeometryTranslator.Translate(StartPoint3),
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
                Geometry = GeometryTranslator.Translate(EndPoint3),
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
                Geometry = GeometryTranslator.Translate(MultiLineString1),
                MaintenanceAuthority = ChangedByOrganization,
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
                Version = 1,
                TemporaryId = AddSegment1.TemporaryId,
                StartNodeId = 1,
                EndNodeId = 2,
                Geometry = AddSegment1.Geometry,
                GeometryVersion = 1,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = ChangedByOrganization,
                    Name = ChangedByOrganizationName
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
                    .ToArray()
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
                MaintenanceAuthority = ChangedByOrganization,
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
                Version = 1,
                TemporaryId = AddSegment2.TemporaryId,
                StartNodeId = 3,
                EndNodeId = 4,
                Geometry = AddSegment2.Geometry,
                GeometryVersion = 1,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = ChangedByOrganization,
                    Name = ChangedByOrganizationName
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
                    .ToArray()
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
                Version = 1,
                TemporaryId = AddSegment3.TemporaryId,
                StartNodeId = 5,
                EndNodeId = 6,
                Geometry = AddSegment3.Geometry,
                GeometryVersion = 1,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = AddSegment3.MaintenanceAuthority,
                    Name = ""
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
                    .ToArray()
            };

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
        public Task when_adding_a_start_and_end_node_and_segment()
        {
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                }));
        }

        [Fact]
        public Task when_adding_a_start_and_end_node_and_segment_to_an_existing_segment()
        {
            var nextWidthsAttributeId = AddSegment1.Widths.Length + 1;
            var nextSurfacesAttributeId = AddSegment1.Surfaces.Length + 1;

            StartNode1Added.Geometry = new RoadNodeGeometry {Point = new Messages.Point {X = 0, Y = 0}};
            EndNode1Added.Geometry = new RoadNodeGeometry {Point = new Messages.Point {X = 0, Y = 10}};
            Segment1Added.Geometry = new RoadSegmentGeometry
            {
                MultiLineString = new[]
                {
                    new Messages.LineString
                    {
                        Measures = new[] {0.0, 10},
                        Points = new[] {StartNode1Added.Geometry.Point, EndNode1Added.Geometry.Point}
                    }
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            Segment1Added.Lanes = new[]
            {
                new Messages.RoadSegmentLaneAttributes
                {
                    FromPosition = 0,
                    ToPosition = 10
                }
            };

            ModifyEndNode1.Type = RoadNodeType.FakeNode;
            ModifyEndNode1.Geometry = EndNode1Added.Geometry;
            AddEndNode2.Geometry= new RoadNodeGeometry {Point = new Messages.Point {X = 0, Y = 20}};
            AddSegment2.Geometry = new RoadSegmentGeometry
            {
                MultiLineString = new[]
                {
                    new Messages.LineString
                    {
                        Measures = new[] {0.0, 10},
                        Points = new[] {ModifyEndNode1.Geometry.Point, AddEndNode2.Geometry.Point}
                    }
                },
                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddSegment2.Lanes = new[]
            {
                new Messages.RequestedRoadSegmentLaneAttribute
                {
                    FromPosition = 0,
                    ToPosition = 10,
                    Direction = RoadSegmentLaneDirection.Forward
                }
            };
            AddSegment2.Widths = new[]
            {
                new Messages.RequestedRoadSegmentWidthAttribute
                {
                    FromPosition = 0,
                    ToPosition = 10
                }
            };
            AddSegment2.Surfaces = new[]
            {
                new Messages.RequestedRoadSegmentSurfaceAttribute
                {
                    FromPosition = 0,
                    ToPosition = 10,
                    Type = RoadSegmentSurfaceType.Unknown
                }
            };

            AddSegment2.StartNodeId = ModifyEndNode1.Id;
            AddSegment2.EndNodeId = AddEndNode2.TemporaryId;

            EndNode2Added.Geometry = AddEndNode2.Geometry;
            EndNode2Added.Id = 3;

            Segment2Added.Geometry = AddSegment2.Geometry;
            Segment2Added.Lanes = new[]
            {
                new Messages.RoadSegmentLaneAttributes
                {
                    FromPosition = AddSegment2.Lanes[0].FromPosition,
                    ToPosition = AddSegment2.Lanes[0].ToPosition,
                    Direction = AddSegment2.Lanes[0].Direction,
                    AsOfGeometryVersion = 1,
                    AttributeId = 1
                }
            };
            Segment2Added.Widths = new[]
            {
                new Messages.RoadSegmentWidthAttributes
                {
                    FromPosition = AddSegment2.Widths[0].FromPosition,
                    ToPosition = AddSegment2.Widths[0].ToPosition,
                    AsOfGeometryVersion = 1,
                    AttributeId = nextWidthsAttributeId
                }
            };
            Segment2Added.Surfaces = new[]
            {
                new Messages.RoadSegmentSurfaceAttributes
                {
                    FromPosition = AddSegment2.Surfaces[0].FromPosition,
                    ToPosition = AddSegment2.Surfaces[0].ToPosition,
                    Type = AddSegment2.Surfaces[0].Type,
                    AsOfGeometryVersion = 1,
                    AttributeId = nextSurfacesAttributeId
                }
            };
            Segment2Added.StartNodeId = ModifyEndNode1.Id;
            Segment2Added.EndNodeId = EndNode2Added.Id;

            EndNode1Modified.Geometry = ModifyEndNode1.Geometry;
            EndNode1Modified.Type = ModifyEndNode1.Type;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
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
                            RoadNodeAdded = EndNode2Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeModified = EndNode1Modified,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_disconnected_node()
        {
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeNotConnectedToAnySegment",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = AddStartNode1.TemporaryId.ToString()
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
        public Task when_adding_a_start_node_connected_to_a_single_segment_as_a_node_other_than_an_end_node()
        {
            AddStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.EndNode)
                .ToString();

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode1,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "1"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
                                        }
                                    }
                                }
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "1"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
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
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode1,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "1"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Expected",
                                            Value = "EndNode"
                                        }
                                    }
                                }
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "1"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
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
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
        {
            AddStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
                .ToString();

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
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode1,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
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
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
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
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
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
        public Task when_adding_a_start_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
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
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint1);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            EndNode2Added.Geometry = GeometryTranslator.Translate(endPoint2);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment1Added.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });


            AddStartNode1.Type = RoadNodeType.FakeNode.ToString();
            StartNode1Added.Type = RoadNodeType.FakeNode.ToString();
            AddSegment2.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment2.Status = AddSegment1.Status;
            AddSegment2.Morphology = AddSegment1.Morphology;
            AddSegment2.Category = AddSegment1.Category;
            AddSegment2.MaintenanceAuthority = AddSegment1.MaintenanceAuthority;
            AddSegment2.AccessRestriction = AddSegment1.AccessRestriction;
            AddSegment2.LeftSideStreetNameId = AddSegment1.LeftSideStreetNameId;
            AddSegment2.RightSideStreetNameId = AddSegment1.RightSideStreetNameId;

            Segment2Added.StartNodeId = StartNode1Added.Id;
            Segment2Added.Status = Segment1Added.Status;
            Segment2Added.Morphology = Segment1Added.Morphology;
            Segment2Added.Category = Segment1Added.Category;
            Segment2Added.MaintenanceAuthority = Segment1Added.MaintenanceAuthority;
            Segment2Added.AccessRestriction = Segment1Added.AccessRestriction;
            Segment2Added.LeftSide = Segment1Added.LeftSide;
            Segment2Added.RightSide = Segment1Added.RightSide;
            Segment2Added.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            EndNode2Added.Id = 3;
            Segment2Added.EndNodeId = 3;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                    Severity = ProblemSeverity.Warning,
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
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
                            }
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Problems = Array.Empty<Messages.Problem>()
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added,
                            Problems = Array.Empty<Messages.Problem>()
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                    Severity = ProblemSeverity.Warning,
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
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
                            }
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                    Severity = ProblemSeverity.Warning,
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "RoadNodeId",
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
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
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public Task when_adding_a_start_node_connecting_two_segments_as_a_fake_node_and_the_segments_differ_by_one_attribute(int testCase)
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
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            StartNode1Added.Geometry = AddStartNode1.Geometry;
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            EndNode1Added.Geometry = AddEndNode1.Geometry;
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            EndNode2Added.Geometry = AddEndNode2.Geometry;
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            Segment1Added.Geometry = AddSegment1.Geometry;
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
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

            var addGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                Type = Fixture.Create<GradeSeparatedJunctionType>(),
                UpperSegmentId = Segment1Added.Id,
                LowerSegmentId = Segment2Added.Id
            };

            AddSegment2.Status = AddSegment1.Status;
            Segment2Added.Status = AddSegment1.Status;
            AddSegment2.Morphology = AddSegment1.Morphology;
            Segment2Added.Morphology = AddSegment1.Morphology;
            AddSegment2.Category = AddSegment1.Category;
            Segment2Added.Category = AddSegment1.Category;
            AddSegment2.MaintenanceAuthority = AddSegment1.MaintenanceAuthority;
            Segment2Added.MaintenanceAuthority.Code = AddSegment1.MaintenanceAuthority;
            AddSegment2.AccessRestriction = AddSegment1.AccessRestriction;
            Segment2Added.AccessRestriction = AddSegment1.AccessRestriction;
            AddSegment2.LeftSideStreetNameId = AddSegment1.LeftSideStreetNameId;
            Segment2Added.LeftSide.StreetNameId = AddSegment1.LeftSideStreetNameId;
            AddSegment2.RightSideStreetNameId = AddSegment1.RightSideStreetNameId;
            Segment2Added.RightSide.StreetNameId = AddSegment1.RightSideStreetNameId;

            switch (testCase)
            {
                case 0:
                    AddSegment2.Status = new Generator<RoadSegmentStatus>(Fixture)
                        .First(candidate => candidate != AddSegment1.Status);
                    Segment2Added.Status = AddSegment2.Status;
                    break;
                case 1:
                    AddSegment2.Morphology = new Generator<RoadSegmentMorphology>(Fixture)
                        .First(candidate => candidate != AddSegment1.Morphology);
                    Segment2Added.Morphology = AddSegment2.Morphology;
                    break;
                case 2:
                    AddSegment2.Category = new Generator<RoadSegmentCategory>(Fixture)
                        .First(candidate => candidate != AddSegment1.Category);;
                    Segment2Added.Category = AddSegment2.Category;
                    break;
                case 3:
                    AddSegment2.MaintenanceAuthority = new Generator<OrganizationId>(Fixture)
                        .First(candidate => candidate != AddSegment1.MaintenanceAuthority);
                    Segment2Added.MaintenanceAuthority.Code = AddSegment2.MaintenanceAuthority;
                    Segment2Added.MaintenanceAuthority.Name = Fixture.Create<OrganizationName>();
                    //AddSegment2.MaintenanceAuthority = ChangedByOrganization;
                    //Segment2Added.MaintenanceAuthority.Code = ChangedByOrganization;
                    //Segment2Added.MaintenanceAuthority.Name = ChangedByOrganizationName;
                    break;
                case 4:
                    AddSegment2.AccessRestriction = new Generator<RoadSegmentAccessRestriction>(Fixture)
                        .First(candidate => candidate != AddSegment1.AccessRestriction);
                    Segment2Added.AccessRestriction = AddSegment2.AccessRestriction;
                    break;
                case 5:
                    AddSegment2.LeftSideStreetNameId = new Generator<CrabStreetnameId?>(Fixture)
                        .First(candidate => candidate != AddSegment1.LeftSideStreetNameId);
                    Segment2Added.LeftSide.StreetNameId = AddSegment2.LeftSideStreetNameId;
                    break;
                case 6:
                    AddSegment2.RightSideStreetNameId = new Generator<CrabStreetnameId?>(Fixture)
                        .First(candidate => candidate != AddSegment1.RightSideStreetNameId);
                    Segment2Added.RightSide.StreetNameId = AddSegment2.RightSideStreetNameId;
                    break;
            }

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .Given(Organizations.ToStreamName(new OrganizationId(AddSegment2.MaintenanceAuthority)),
                    new ImportedOrganization
                    {
                        Code = Segment2Added.MaintenanceAuthority.Code,
                        Name = Segment2Added.MaintenanceAuthority.Name,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    },
                    new RequestedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                            RoadNodeAdded = EndNode2Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            GradeSeparatedJunctionAdded = new GradeSeparatedJunctionAdded
                            {
                                Id = 1,
                                TemporaryId = addGradeSeparatedJunction.TemporaryId,
                                UpperRoadSegmentId = Segment1Added.Id,
                                LowerRoadSegmentId = Segment2Added.Id,
                                Type = addGradeSeparatedJunction.Type
                            },
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }
        [Fact]
        public Task when_adding_an_end_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
        {
            AddEndNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
                .ToString();

            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint1);
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint1.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.EndNodeId = AddEndNode1.TemporaryId;
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint2.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode1,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
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
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
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
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "2"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
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
        public Task when_adding_a_start_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
        {
            AddStartNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.RealNode && type != RoadNodeType.MiniRoundabout)
                .ToString();

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
            var endPoint3 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint1);
            AddEndNode1.Type = RoadNodeType.EndNode.ToString();
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);
            AddEndNode2.Type = RoadNodeType.EndNode.ToString();
            AddEndNode3.Geometry = GeometryTranslator.Translate(endPoint3);
            AddEndNode3.Type = RoadNodeType.EndNode.ToString();
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint1.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint2.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment3.StartNodeId = AddStartNode1.TemporaryId;
            AddSegment3.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment3Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment3.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment3Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment3.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment3Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment3.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate, endPoint3.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode3
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment3
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode1,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
                                        },
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
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
                                        },
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
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
                                        },
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
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment3,
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
                                            Value = AddStartNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddStartNode1.Type
                                        },
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
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_adding_an_end_node_connecting_more_than_two_segments_as_a_node_other_than_a_real_node_or_mini_roundabout()
        {
            AddEndNode1.Type = new Generator<RoadNodeType>(Fixture)
                .First(type => type != RoadNodeType.RealNode && type != RoadNodeType.MiniRoundabout)
                .ToString();

            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var startPoint3 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint1);
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
            AddStartNode3.Geometry = GeometryTranslator.Translate(startPoint3);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint1.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment2.EndNodeId = AddEndNode1.TemporaryId;
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint2.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });
            AddSegment3.EndNodeId = AddEndNode1.TemporaryId;
            AddSegment3.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment3Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment3.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment3Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment3.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment3Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment3.Geometry = GeometryTranslator.Translate(
                new MultiLineString(new []
                {
                    new NetTopologySuite.Geometries.LineString(new CoordinateArraySequence(new []
                    {
                        startPoint3.Coordinate, endPoint.Coordinate
                    }), GeometryConfiguration.GeometryFactory)
                })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                });

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode3
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment3
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode1,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
                                        },
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
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
                                        },
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
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
                                        },
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
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment3,
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
                                            Value = AddEndNode1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentCount",
                                            Value = "3"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment1.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ConnectedSegmentId",
                                            Value = AddSegment3.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Actual",
                                            Value = AddEndNode1.Type
                                        },
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
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_adding_a_start_node_with_a_geometry_that_has_been_taken()
        {
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                    new []
                    {
                        new NetTopologySuite.Geometries.LineString(
                            new CoordinateArraySequence(new Coordinate[]
                            {
                                new CoordinateM(StartPoint1.X, StartPoint1.Y, 0.0),
                                new CoordinateM(MiddlePoint2.X, MiddlePoint2.Y, StartPoint1.Distance(MiddlePoint2)),
                                new CoordinateM(EndPoint2.X, EndPoint2.Y,
                                    StartPoint1.Distance(MiddlePoint2) +
                                    MiddlePoint2.Distance(EndPoint2))
                            }),
                            GeometryConfiguration.GeometryFactory
                        )
                    }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
            );
            AddStartNode2.Geometry = StartNode1Added.Geometry;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddStartNode2,
                            Problems = new[]
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
                                },
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTooClose",
                                    Severity = ProblemSeverity.Warning,
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ToOtherSegment",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "modifiedRoadSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "intersectingRoadSegmentId",
                                            Value = Segment1Added.Id.ToString()
                                        },
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
        public Task when_adding_an_end_node_with_a_geometry_that_has_been_taken()
        {
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                    new []
                    {
                        new NetTopologySuite.Geometries.LineString(
                            new CoordinateArraySequence(new Coordinate[]
                            {
                                new CoordinateM(StartPoint2.X, StartPoint2.Y, 0.0),
                                new CoordinateM(MiddlePoint2.X, MiddlePoint2.Y, 70.7107),
                                new CoordinateM(EndPoint1.X, EndPoint1.Y, 228.8245)
                            }),
                            GeometryConfiguration.GeometryFactory
                        )
                    }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
            );
            AddEndNode2.Geometry = EndNode1Added.Geometry;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode2,
                            Problems = new[]
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
                                },
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTooClose",
                                    Severity = ProblemSeverity.Warning,
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ToOtherSegment",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        },
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "modifiedRoadSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "intersectingRoadSegmentId",
                                            Value = Segment1Added.Id.ToString()
                                        },
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
        public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
        {
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                            RoadNodeAdded = StartNode2Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added,
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_start_node_that_is_within_two_meters_of_another_segment()
        {
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];

            do
            {
                var random = new Random();
                var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(
                    StartPoint1.X + random.Next(1, 1000) / 1000.0 * Distances.TooClose,
                    StartPoint1.Y + random.Next(1, 1000) / 1000.0 * Distances.TooClose
                )) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()};
                AddSegment2.Geometry = GeometryTranslator.Translate(
                    new MultiLineString(
                        new[]
                        {
                            new NetTopologySuite.Geometries.LineString(
                                new CoordinateArraySequence(new Coordinate[]
                                {
                                    new CoordinateM(startPoint.X, startPoint.Y, 0.0),
                                    new CoordinateM(MiddlePoint2.X, MiddlePoint2.Y, startPoint.Distance(MiddlePoint2)),
                                    new CoordinateM(EndPoint2.X, EndPoint2.Y,
                                        startPoint.Distance(MiddlePoint2) + MiddlePoint2.Distance(EndPoint2))
                                }),
                                GeometryConfiguration.GeometryFactory
                            )
                        }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
                );
                AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint);

                StartNode2Added.Geometry = AddStartNode2.Geometry;
                Segment2Added.Geometry = AddSegment2.Geometry;
            } while (GeometryTranslator.Translate(Segment1Added.Geometry).Intersects(GeometryTranslator.Translate(AddSegment2.Geometry)));

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode2Added,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadNodeTooClose",
                                    Severity = ProblemSeverity.Warning,
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "ToOtherSegment",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        },
                        new Messages.AcceptedChange()
                        {
                            RoadNodeAdded = EndNode2Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange()
                        {
                            RoadSegmentAdded = Segment2Added,
                            Problems = new Messages.Problem[0]
                        },
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact(Skip = "This test should be about being within two meters of another segment")]
        public Task when_adding_an_end_node_that_is_within_two_meters_of_another_node()
        {
            var random = new Random();
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(
                EndPoint1.X + random.NextDouble() / 2.0 * Distances.TooClose,
                EndPoint1.Y + random.NextDouble() / 2.0 * Distances.TooClose
            ));
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment2.Geometry = GeometryTranslator.Translate(
                new MultiLineString(
                    new []
                    {
                        new NetTopologySuite.Geometries.LineString(
                            new CoordinateArraySequence(new[] {StartPoint2.Coordinate, MiddlePoint2.Coordinate, endPoint.Coordinate}),
                            GeometryConfiguration.GeometryFactory
                        )
                    }) {SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()}
            );
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint);
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadNode = AddEndNode2,
                            Problems = new[]
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
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
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
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added,
                            Problems = new Messages.Problem[0]
                        },
                        new Messages.AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added,
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
            );
        }

        [Fact]
        public Task when_adding_a_segment_with_a_geometry_that_has_been_taken()
        {
            StartNode1Added.Type = RoadNodeType.FakeNode.ToString();
            EndNode1Added.Type = RoadNodeType.FakeNode.ToString();
            AddSegment2.StartNodeId = StartNode1Added.Id;
            AddSegment2.EndNodeId = EndNode1Added.Id;
            AddSegment2.Geometry = Segment1Added.Geometry;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(2),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
                            Problems = new[]
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
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        public static IEnumerable<object[]> SelfOverlapsCases
        {
            get
            {
                var startPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 10.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endPoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(5.0, 0.0, 15.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var multiLineString1 = new MultiLineString(
                    new []
                    {
                        new NetTopologySuite.Geometries.LineString(
                            new CoordinateArraySequence(new[] { startPoint1.Coordinate, middlePoint1.Coordinate, endPoint1.Coordinate }),
                            GeometryConfiguration.GeometryFactory
                        )
                    })
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };

                //covers itself
                yield return new object[] {startPoint1, endPoint1, multiLineString1};

                var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(5.0, 0.0, 0.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2A = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 0.0, 15.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2B = new NetTopologySuite.Geometries.Point(new CoordinateM(20.0, 10.0, 25.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2C = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 10.0, 45.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var middlePoint2D = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 55.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var endPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(8.0, 0.0, 63.0))
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                };
                var multiLineString2 = new MultiLineString(
                    new []
                    {
                        new NetTopologySuite.Geometries.LineString(
                            new CoordinateArraySequence(new[] { startPoint2.Coordinate, middlePoint2A.Coordinate, middlePoint2B.Coordinate, middlePoint2C.Coordinate, middlePoint2D.Coordinate, endPoint2.Coordinate }),
                            GeometryConfiguration.GeometryFactory
                        )
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
            NetTopologySuite.Geometries.Point startPoint,
            NetTopologySuite.Geometries.Point endPoint,
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
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentGeometrySelfOverlaps",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_geometry_that_self_intersects()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 10.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(5.0, 20.0, 21.1803))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(5.0, 0.0, 41.1803))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var lineString = new NetTopologySuite.Geometries.LineString(
                new CoordinateArraySequence(new[] { startPoint.Coordinate, middlePoint1.Coordinate, middlePoint2.Coordinate, endPoint.Coordinate }),
                GeometryConfiguration.GeometryFactory
            );
            var multiLineString = new MultiLineString(
                new []
                {
                    lineString
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
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentGeometrySelfIntersects",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_missing_start_node()
        {
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentStartNodeMissing",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_missing_end_node()
        {
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentEndNodeMissing",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_start_point_does_not_match_its_start_node_geometry()
        {
            AddStartNode1.Geometry = GeometryTranslator.Translate(MiddlePoint1);

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_start_point_does_not_match_its_existing_start_node_geometry()
        {
            AddSegment2.StartNodeId = StartNode1Added.Id;
            StartNode1Added.Type = RoadNodeType.FakeNode.ToString();
            EndNode1Added.Type = RoadNodeType.EndNode.ToString();
            AddEndNode2.Type = RoadNodeType.EndNode.ToString();

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                    Changes = new []
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
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_end_point_does_not_match_its_end_node_geometry()
        {
            AddEndNode1.Geometry = GeometryTranslator.Translate(MiddlePoint1);

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_whose_end_point_does_not_match_its_existing_end_node_geometry()
        {
            AddSegment2.EndNodeId = EndNode1Added.Id;
            StartNode1Added.Type = RoadNodeType.EndNode.ToString();
            EndNode1Added.Type = RoadNodeType.FakeNode.ToString();
            AddStartNode2.Type = RoadNodeType.EndNode.ToString();

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                    Changes = new []
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
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment2,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_with_a_line_string_with_length_0()
        {
            var geometry = new MultiLineString(new [] {new NetTopologySuite.Geometries.LineString(new Coordinate[0])})
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddSegment1.Geometry = GeometryTranslator.Translate(geometry);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentGeometryLengthIsZero",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
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
                        new RequestedRoadSegmentLaneAttribute
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
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "FromPosition",
                                Value = "2.0"
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
                        new RequestedRoadSegmentLaneAttribute
                        {
                            AttributeId = attributeId1,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Count = fixture.Create<RoadSegmentLaneCount>(),
                            Direction = fixture.Create<RoadSegmentLaneDirection>()
                        },
                        new RequestedRoadSegmentLaneAttribute
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
                                Name = "ToPosition",
                                Value =  "50.0"
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId2.ToInt32().ToString()
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "FromPosition",
                                Value =  "55.0"
                            }
                        }
                    }
                };

                var attributeId3 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new RequestedRoadSegmentLaneAttribute
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
                            new Messages.ProblemParameter
                            {
                                Name = "ToPosition",
                                Value = "50.0"
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "Length",
                                Value = "141.4213562373095"
                            }
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonAdjacentLaneAttributesCases))]
        public Task when_adding_a_segment_with_non_adjacent_lane_attributes(RequestedRoadSegmentLaneAttribute[] attributes, Messages.Problem problem)
        {
            AddSegment1.Lanes = attributes;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[] { problem }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
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
                        new RequestedRoadSegmentWidthAttribute
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
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "FromPosition",
                                Value = "2.0"
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
                        new RequestedRoadSegmentWidthAttribute
                        {
                            AttributeId = attributeId1,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Width = fixture.Create<RoadSegmentWidth>()
                        },
                        new RequestedRoadSegmentWidthAttribute
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
                                Name = "ToPosition",
                                Value = "50.0"
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId2.ToInt32().ToString()
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "FromPosition",
                                Value = "55.0"
                            }
                        }
                    }
                };

                var attributeId3 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new RequestedRoadSegmentWidthAttribute
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
                            new Messages.ProblemParameter
                            {
                                Name = "ToPosition",
                                Value = "50.0"
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "Length",
                                Value = "141.4213562373095"
                            }
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonAdjacentWidthAttributesCases))]
        public Task when_adding_a_segment_with_non_adjacent_width_attributes(RequestedRoadSegmentWidthAttribute[] attributes, Messages.Problem problem)
        {
            AddSegment1.Widths = attributes;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[] { problem }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
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
                        new RequestedRoadSegmentSurfaceAttribute
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
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "FromPosition",
                                Value = "2.0"
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
                        new RequestedRoadSegmentSurfaceAttribute
                        {
                            AttributeId = attributeId1,
                            FromPosition = 0.0m,
                            ToPosition = 50.0m,
                            Type = fixture.Create<RoadSegmentSurfaceType>()
                        },
                        new RequestedRoadSegmentSurfaceAttribute
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
                                Name = "ToPosition",
                                Value = "50.0"
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "AttributeId",
                                Value = attributeId2.ToInt32().ToString()
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "FromPosition",
                                Value = "55.0"
                            }
                        }
                    }
                };

                var attributeId3 = fixture.Create<AttributeId>();
                yield return new object[]
                {
                    new[]
                    {
                        new RequestedRoadSegmentSurfaceAttribute
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
                            new Messages.ProblemParameter
                            {
                                Name = "ToPosition",
                                Value = "50.0"
                            },
                            new Messages.ProblemParameter
                            {
                                Name = "Length",
                                Value = "141.4213562373095"
                            }
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonAdjacentSurfaceAttributesCases))]
        public Task when_adding_a_segment_with_non_adjacent_surface_attributes(RequestedRoadSegmentSurfaceAttribute[] attributes, Messages.Problem problem)
        {
            AddSegment1.Surfaces = attributes;

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[] { problem }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_to_a_european_road()
        {
            var addRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
            {
                TemporaryAttributeId = Fixture.Create<AttributeId>(),
                Number = Fixture.Create<EuropeanRoadNumber>(),
                SegmentId = AddSegment1.TemporaryId
            };

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadSegmentToEuropeanRoad = addRoadSegmentToEuropeanRoad
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
                            {
                                AttributeId = 1,
                                TemporaryAttributeId = addRoadSegmentToEuropeanRoad.TemporaryAttributeId,
                                Number = addRoadSegmentToEuropeanRoad.Number,
                                SegmentId = Segment1Added.Id
                            },
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_missing_segment_to_a_european_road()
        {
            var addRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
            {
                TemporaryAttributeId = Fixture.Create<AttributeId>(),
                Number = Fixture.Create<EuropeanRoadNumber>(),
                SegmentId = AddSegment1.TemporaryId
            };

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadSegmentToEuropeanRoad = addRoadSegmentToEuropeanRoad
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegmentToEuropeanRoad = addRoadSegmentToEuropeanRoad,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentMissing",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = addRoadSegmentToEuropeanRoad.SegmentId.ToString()
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
        public Task when_adding_a_segment_to_a_national_road()
        {
            var addRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
            {
                TemporaryAttributeId = Fixture.Create<AttributeId>(),
                Number = Fixture.Create<NationalRoadNumber>(),
                SegmentId = AddSegment1.TemporaryId
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadSegmentToNationalRoad = addRoadSegmentToNationalRoad
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
                            {
                                AttributeId = 1,
                                TemporaryAttributeId = addRoadSegmentToNationalRoad.TemporaryAttributeId,
                                Number = addRoadSegmentToNationalRoad.Number,
                                SegmentId = Segment1Added.Id
                            },
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_missing_segment_to_a_national_road()
        {
            var addRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
            {
                TemporaryAttributeId = Fixture.Create<AttributeId>(),
                Number = Fixture.Create<NationalRoadNumber>(),
                SegmentId = AddSegment1.TemporaryId
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadSegmentToNationalRoad = addRoadSegmentToNationalRoad
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegmentToNationalRoad = addRoadSegmentToNationalRoad,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentMissing",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = addRoadSegmentToNationalRoad.SegmentId.ToString()
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
        public Task when_adding_a_segment_to_a_numbered_road()
        {
            var addRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
            {
                TemporaryAttributeId = Fixture.Create<AttributeId>(),
                Number = Fixture.Create<NumberedRoadNumber>(),
                Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>(),
                Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>(),
                SegmentId = AddSegment1.TemporaryId
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    },
                    new RequestedChange
                    {
                        AddRoadSegmentToNumberedRoad = addRoadSegmentToNumberedRoad
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
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
                        },
                        new Messages.AcceptedChange
                        {
                            RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                            {
                                AttributeId = 1,
                                TemporaryAttributeId = addRoadSegmentToNumberedRoad.TemporaryAttributeId,
                                Number = addRoadSegmentToNumberedRoad.Number,
                                Direction = addRoadSegmentToNumberedRoad.Direction,
                                Ordinal = addRoadSegmentToNumberedRoad.Ordinal,
                                SegmentId = Segment1Added.Id
                            },
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_missing_segment_to_a_numbered_road()
        {
            var addRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
            {
                TemporaryAttributeId = Fixture.Create<AttributeId>(),
                Number = Fixture.Create<NumberedRoadNumber>(),
                Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>(),
                Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>(),
                SegmentId = AddSegment1.TemporaryId
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadSegmentToNumberedRoad = addRoadSegmentToNumberedRoad
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegmentToNumberedRoad = addRoadSegmentToNumberedRoad,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentMissing",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "SegmentId",
                                            Value = addRoadSegmentToNumberedRoad.SegmentId.ToString()
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
        public Task when_adding_a_grade_separated_junction_with_segments_that_intersect()
        {
            Segment1Added.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new Coordinate[] {new CoordinateM(0.0, 0.0), new CoordinateM(50.0, 50.0)}),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            }));
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            Segment2Added.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new Coordinate[] {new CoordinateM(0.0, 50.0), new CoordinateM(50.0, 0.0)}),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            }));
            Segment2Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment2Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment2Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];
            var addGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                Type = Fixture.Create<GradeSeparatedJunctionType>(),
                UpperSegmentId = Segment1Added.Id,
                LowerSegmentId = Segment2Added.Id
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                            RoadSegmentAdded = Segment2Added
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.AcceptedChange
                        {
                            GradeSeparatedJunctionAdded = new GradeSeparatedJunctionAdded
                            {
                                Id = 1,
                                TemporaryId = addGradeSeparatedJunction.TemporaryId,
                                UpperRoadSegmentId = Segment1Added.Id,
                                LowerRoadSegmentId = Segment2Added.Id,
                                Type = addGradeSeparatedJunction.Type
                            },
                            Problems = new Messages.Problem[0]
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_that_intersects_without_grade_separated_junction()
        {
            Segment1Added.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new Coordinate[] {new CoordinateM(0.0, 0.0), new CoordinateM(50.0, 50.0)}),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            }));
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];

            var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 50.0, 0.0));
            var endPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(50.0, 0.0, 70.71067));
            AddSegment2.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new[] {startPoint2.Coordinate, endPoint2.Coordinate}),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange()
                        {
                            AddRoadSegment = AddSegment2,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "modifiedRoadSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "intersectingRoadSegmentId",
                                            Value = Segment1Added.Id.ToString()
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
        public Task when_modifying_a_segment_that_intersects_without_grade_separated_junction()
        {
            Segment1Added.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new Coordinate[] {new CoordinateM(0.0, 0.0), new CoordinateM(50.0, 50.0)}),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            }));
            Segment1Added.Lanes = new Messages.RoadSegmentLaneAttributes[0];
            Segment1Added.Widths = new Messages.RoadSegmentWidthAttributes[0];
            Segment1Added.Surfaces = new Messages.RoadSegmentSurfaceAttributes[0];

            var startPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 50.0, 0.0));
            var endPoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(50.0, 0.0, 70.71067));
            AddSegment2.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new[] {startPoint2.Coordinate, endPoint2.Coordinate}),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
            AddSegment2.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            AddSegment2.Widths = new RequestedRoadSegmentWidthAttribute[0];
            AddSegment2.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            AddStartNode2.Geometry = GeometryTranslator.Translate(startPoint2);
            AddEndNode2.Geometry = GeometryTranslator.Translate(endPoint2);

            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode2
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode2
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment2
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new []
                    {
                        new Messages.RejectedChange()
                        {
                            AddRoadSegment = AddSegment2,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "modifiedRoadSegmentId",
                                            Value = AddSegment2.TemporaryId.ToString()
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "intersectingRoadSegmentId",
                                            Value = Segment1Added.Id.ToString()
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
        public Task when_adding_a_grade_separated_junction_with_a_missing_upper_segment()
        {
            var addGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                Type = Fixture.Create<GradeSeparatedJunctionType>(),
                UpperSegmentId = Fixture.Create<RoadSegmentId>(),
                LowerSegmentId = Segment1Added.Id
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddGradeSeparatedJunction = addGradeSeparatedJunction,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "UpperRoadSegmentMissing",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_grade_separated_junction_with_a_missing_lower_segment()
        {
            var addGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                Type = Fixture.Create<GradeSeparatedJunctionType>(),
                UpperSegmentId = Segment1Added.Id,
                LowerSegmentId = Fixture.Create<RoadSegmentId>()
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddGradeSeparatedJunction = addGradeSeparatedJunction,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "LowerRoadSegmentMissing",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_grade_separated_junction_with_segments_that_do_not_intersect()
        {
            var addGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = Fixture.Create<GradeSeparatedJunctionId>(),
                Type = Fixture.Create<GradeSeparatedJunctionType>(),
                UpperSegmentId = Segment1Added.Id,
                LowerSegmentId = Segment2Added.Id
            };
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
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
                            RoadSegmentAdded = Segment2Added
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddGradeSeparatedJunction = addGradeSeparatedJunction
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddGradeSeparatedJunction = addGradeSeparatedJunction,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "UpperAndLowerRoadSegmentDoNotIntersect",
                                    Parameters = new Messages.ProblemParameter[0]
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }

        [Fact]
        public Task when_adding_a_segment_where_first_measure_is_not_zero()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(14.0, 14.0, Math.Sqrt(Math.Pow(14.0, 2.0) + Math.Pow(14.0, 2.0))))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new []
                    {
                        startPoint.Coordinate,
                        new CoordinateM(11.0, 11.0, Math.Sqrt(Math.Pow(11.0, 2.0) + Math.Pow(11.0, 2.0))),
                        new CoordinateM(12.0, 12.0, Math.Sqrt(Math.Pow(12.0, 2.0) + Math.Pow(12.0, 2.0))),
                        new CoordinateM(13.0, 13.0, Math.Sqrt(Math.Pow(13.0, 2.0) + Math.Pow(13.0, 2.0))),
                        endPoint.Coordinate
                    }),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new []
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentStartPointMeasureValueNotEqualToZero",
                                    Parameters = new []
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointX", Value = "0"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointY", Value = "0"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Measure", Value = "10"
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
        public Task when_adding_a_segment_where_last_measure_is_not_equal_to_length()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var length = Math.Sqrt(Math.Pow(14.0, 2.0) + Math.Pow(14.0, 2.0));
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(14.0, 14.0, 100.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate,
                        new CoordinateM(11.0, 11.0, Math.Sqrt(Math.Pow(11.0, 2.0) + Math.Pow(11.0, 2.0))),
                        new CoordinateM(12.0, 12.0, Math.Sqrt(Math.Pow(12.0, 2.0) + Math.Pow(12.0, 2.0))),
                        new CoordinateM(13.0, 13.0,  Math.Sqrt(Math.Pow(13.0, 2.0) + Math.Pow(13.0, 2.0))),
                        endPoint.Coordinate
                    }),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentEndPointMeasureValueNotEqualToLength",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointX", Value = "14"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointY", Value = "14"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Measure", Value = "100"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Length",
                                            Value = length.ToString(CultureInfo.InvariantCulture)
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
        public Task when_adding_a_segment_where_measure_is_out_of_range()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var length = Math.Sqrt(Math.Pow(14.0, 2.0) + Math.Pow(14.0, 2.0));
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(14.0, 14.0, length))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate,
                        new CoordinateM(11.0, 11.0, -1.0),
                        new CoordinateM(12.0, 12.0, Math.Sqrt(Math.Pow(12.0, 2.0) + Math.Pow(12.0, 2.0))),
                        new CoordinateM(13.0, 13.0, 100.0),
                        endPoint.Coordinate
                    }),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentPointMeasureValueOutOfRange",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointX", Value = "11"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointY", Value = "11"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Measure", Value = "-1"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "MeasureLowerBoundary",
                                            Value = "0"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "MeasureUpperBoundary",
                                            Value = length.ToString(CultureInfo.InvariantCulture)
                                        }
                                    }
                                },
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentPointMeasureValueOutOfRange",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointX", Value = "13"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointY", Value = "13"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Measure", Value = "100"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "MeasureLowerBoundary",
                                            Value = "0"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "MeasureUpperBoundary",
                                            Value = length.ToString(CultureInfo.InvariantCulture)
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
        public Task when_adding_a_segment_where_measure_is_not_an_increasing_distance_from_start_point()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var length = Math.Sqrt(Math.Pow(14.0, 2.0) + Math.Pow(14.0, 2.0));
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(14.0, 14.0, length))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            AddStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
            AddEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
            AddSegment1.Lanes = new RequestedRoadSegmentLaneAttribute[0];
            AddSegment1.Widths = new RequestedRoadSegmentWidthAttribute[0];
            AddSegment1.Surfaces = new RequestedRoadSegmentSurfaceAttribute[0];
            AddSegment1.Geometry = GeometryTranslator.Translate(new MultiLineString(new []
            {
                new NetTopologySuite.Geometries.LineString(
                    new CoordinateArraySequence(new[]
                    {
                        startPoint.Coordinate,
                        new CoordinateM(11.0, 11.0, Math.Sqrt(Math.Pow(11.0, 2.0) + Math.Pow(11.0, 2.0))),
                        new CoordinateM(12.0, 12.0, Math.Sqrt(Math.Pow(10.0, 2.0) + Math.Pow(10.0, 2.0))),
                        new CoordinateM(13.0, 13.0, Math.Sqrt(Math.Pow(13.0, 2.0) + Math.Pow(13.0, 2.0))),
                        endPoint.Coordinate
                    }),
                    GeometryConfiguration.GeometryFactory)
                {
                    SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                }
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
            return Run(scenario => scenario
                .Given(Organizations.ToStreamName(ChangedByOrganization),
                    new ImportedOrganization
                    {
                        Code = ChangedByOrganization,
                        Name = ChangedByOrganizationName,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    }
                )
                .When(TheOperator.ChangesTheRoadNetwork(
                    RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                    new RequestedChange
                    {
                        AddRoadNode = AddStartNode1
                    },
                    new RequestedChange
                    {
                        AddRoadNode = AddEndNode1
                    },
                    new RequestedChange
                    {
                        AddRoadSegment = AddSegment1
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    RequestId = RequestId, Reason = ReasonForChange, Operator = ChangedByOperator, OrganizationId = ChangedByOrganization, Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new Messages.RejectedChange
                        {
                            AddRoadSegment = AddSegment1,
                            Problems = new[]
                            {
                                new Messages.Problem
                                {
                                    Reason = "RoadSegmentPointMeasureValueDoesNotIncrease",
                                    Parameters = new[]
                                    {
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointX", Value = "12"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PointY", Value = "12"
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "Measure",
                                            Value = Math.Sqrt(Math.Pow(10.0, 2.0) + Math.Pow(10.0, 2.0)).ToString(CultureInfo.InvariantCulture)
                                        },
                                        new Messages.ProblemParameter
                                        {
                                            Name = "PreviousMeasure",
                                            Value = Math.Sqrt(Math.Pow(11.0, 2.0) + Math.Pow(11.0, 2.0)).ToString(CultureInfo.InvariantCulture)
                                        }
                                    }
                                }
                            }
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }));
        }
    }
}
