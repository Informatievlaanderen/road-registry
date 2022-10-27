namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using GeometryTranslator = RoadRegistry.BackOffice.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNetworkTestHelpers
{
    private RoadNetworkTestHelpers()
    {
        Fixture = new Fixture();

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

        StartPoint1 = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MiddlePoint1 = new Point(new CoordinateM(50.0, 50.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        EndPoint1 = new Point(new CoordinateM(100.0, 100.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MultiLineString1 = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { StartPoint1.Coordinate, MiddlePoint1.Coordinate, EndPoint1.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        StartPoint2 = new Point(new CoordinateM(0.0, 200.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MiddlePoint2 = new Point(new CoordinateM(50.0, 250.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        EndPoint2 = new Point(new CoordinateM(100.0, 300.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MultiLineString2 = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { StartPoint2.Coordinate, MiddlePoint2.Coordinate, EndPoint2.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        StartPoint3 = new Point(new CoordinateM(0.0, 500.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MiddlePoint3 = new Point(new CoordinateM(50.0, 550.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        EndPoint3 = new Point(new CoordinateM(100.0, 600.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MultiLineString3 = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { StartPoint3.Coordinate, MiddlePoint3.Coordinate, EndPoint3.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            }) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        AddStartNode1 = new AddRoadNode
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

        ModifyStartNode1 = new ModifyRoadNode
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

        AddEndNode1 = new AddRoadNode
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

        ModifyEndNode1 = new ModifyRoadNode
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

        AddStartNode2 = new AddRoadNode
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

        AddEndNode2 = new AddRoadNode
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

        AddStartNode3 = new AddRoadNode
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

        AddEndNode3 = new AddRoadNode
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
        AddSegment1 = new AddRoadSegment
        {
            TemporaryId = Fixture.Create<RoadSegmentId>(),
            StartNodeId = AddStartNode1.TemporaryId,
            EndNodeId = AddEndNode1.TemporaryId,
            Geometry = GeometryTranslator.Translate(MultiLineString1),
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
                        part.ToPosition = Convert.ToDecimal(MultiLineString1.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString1.Length) / laneCount1);

                    return part;
                })
                .ToArray(),
            Widths = Fixture
                .CreateMany<RequestedRoadSegmentWidthAttribute>(widthCount1)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(MultiLineString1.Length) / widthCount1);
                    if (index == widthCount1 - 1)
                        part.ToPosition = Convert.ToDecimal(MultiLineString1.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString1.Length) / widthCount1);

                    return part;
                })
                .ToArray(),
            Surfaces = Fixture
                .CreateMany<RequestedRoadSegmentSurfaceAttribute>(surfaceCount1)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(MultiLineString1.Length) / surfaceCount1);
                    if (index == surfaceCount1 - 1)
                        part.ToPosition = Convert.ToDecimal(MultiLineString1.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString1.Length) / surfaceCount1);

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
                .Select((lane, index) => new RoadSegmentLaneAttributes
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
                .Select((width, index) => new RoadSegmentWidthAttributes
                {
                    AttributeId = index + 1,
                    Width = width.Width,
                    FromPosition = width.FromPosition,
                    ToPosition = width.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray(),
            Surfaces = AddSegment1.Surfaces
                .Select((surface, index) => new RoadSegmentSurfaceAttributes
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
        AddSegment2 = new AddRoadSegment
        {
            TemporaryId = AddSegment1.TemporaryId + 1,
            StartNodeId = AddStartNode2.TemporaryId,
            EndNodeId = AddEndNode2.TemporaryId,
            Geometry = GeometryTranslator.Translate(MultiLineString2),
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
                        part.ToPosition = Convert.ToDecimal(MultiLineString2.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString2.Length) / laneCount2);

                    return part;
                })
                .ToArray(),
            Widths = Fixture
                .CreateMany<RequestedRoadSegmentWidthAttribute>(widthCount2)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(MultiLineString2.Length) / widthCount2);
                    if (index == widthCount2 - 1)
                        part.ToPosition = Convert.ToDecimal(MultiLineString2.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString2.Length) / widthCount2);

                    return part;
                })
                .ToArray(),
            Surfaces = Fixture
                .CreateMany<RequestedRoadSegmentSurfaceAttribute>(surfaceCount2)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(MultiLineString2.Length) / surfaceCount2);
                    if (index == surfaceCount2 - 1)
                        part.ToPosition = Convert.ToDecimal(MultiLineString2.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString2.Length) / surfaceCount2);

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
                .Select((lane, index) => new RoadSegmentLaneAttributes
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
                .Select((width, index) => new RoadSegmentWidthAttributes
                {
                    AttributeId = widthCount1 + index + 1,
                    Width = width.Width,
                    FromPosition = width.FromPosition,
                    ToPosition = width.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray(),
            Surfaces = AddSegment2.Surfaces
                .Select((surface, index) => new RoadSegmentSurfaceAttributes
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
        AddSegment3 = new AddRoadSegment
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
                        part.ToPosition = Convert.ToDecimal(MultiLineString3.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString3.Length) / laneCount3);

                    return part;
                })
                .ToArray(),
            Widths = Fixture
                .CreateMany<RequestedRoadSegmentWidthAttribute>(widthCount3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(MultiLineString3.Length) / widthCount3);
                    if (index == widthCount3 - 1)
                        part.ToPosition = Convert.ToDecimal(MultiLineString3.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString3.Length) / widthCount3);

                    return part;
                })
                .ToArray(),
            Surfaces = Fixture
                .CreateMany<RequestedRoadSegmentSurfaceAttribute>(surfaceCount3)
                .Select((part, index) =>
                {
                    part.FromPosition = index * (Convert.ToDecimal(MultiLineString3.Length) / surfaceCount3);
                    if (index == surfaceCount3 - 1)
                        part.ToPosition = Convert.ToDecimal(MultiLineString3.Length);
                    else
                        part.ToPosition = (index + 1) * (Convert.ToDecimal(MultiLineString3.Length) / surfaceCount3);

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
                .Select((lane, index) => new RoadSegmentLaneAttributes
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
                .Select((width, index) => new RoadSegmentWidthAttributes
                {
                    AttributeId = widthCount1 + widthCount2 + index + 1,
                    Width = width.Width,
                    FromPosition = width.FromPosition,
                    ToPosition = width.ToPosition,
                    AsOfGeometryVersion = 1
                })
                .ToArray(),
            Surfaces = AddSegment3.Surfaces
                .Select((surface, index) => new RoadSegmentSurfaceAttributes
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

    public AddRoadNode AddEndNode1 { get; }
    public AddRoadNode AddEndNode2 { get; }
    public AddRoadNode AddEndNode3 { get; }
    public AddRoadSegment AddSegment1 { get; }
    public AddRoadSegment AddSegment2 { get; }
    public AddRoadSegment AddSegment3 { get; }
    public AddRoadNode AddStartNode1 { get; }
    public AddRoadNode AddStartNode2 { get; }
    public AddRoadNode AddStartNode3 { get; }
    public ArchiveId ArchiveId { get; }
    public OperatorName ChangedByOperator { get; }
    public OrganizationId ChangedByOrganization { get; }
    public OrganizationName ChangedByOrganizationName { get; }
    public RoadNodeAdded EndNode1Added { get; }
    public RoadNodeModified EndNode1Modified { get; }
    public RoadNodeAdded EndNode2Added { get; }
    public RoadNodeAdded EndNode3Added { get; }
    public Point EndPoint1 { get; }
    public Point EndPoint2 { get; }
    public Point EndPoint3 { get; }
    public IFixture Fixture { get; }
    public Point MiddlePoint1 { get; }
    public Point MiddlePoint2 { get; }
    public Point MiddlePoint3 { get; }
    public ModifyRoadNode ModifyEndNode1 { get; }
    public ModifyRoadNode ModifyStartNode1 { get; }
    public MultiLineString MultiLineString1 { get; }
    public MultiLineString MultiLineString2 { get; }
    public MultiLineString MultiLineString3 { get; }
    public Reason ReasonForChange { get; }
    public ChangeRequestId RequestId { get; }
    public RoadSegmentAdded Segment1Added { get; }
    public RoadSegmentAdded Segment2Added { get; }
    public RoadSegmentAdded Segment3Added { get; }
    public RoadNodeAdded StartNode1Added { get; }
    public RoadNodeModified StartNode1Modified { get; }
    public RoadNodeAdded StartNode2Added { get; }
    public RoadNodeAdded StartNode3Added { get; }
    public Point StartPoint1 { get; }
    public Point StartPoint2 { get; }
    public Point StartPoint3 { get; }
    public TransactionId TransactionId { get; }

    public static RoadNetworkTestHelpers Create()
    {
        return new RoadNetworkTestHelpers();
    }
}