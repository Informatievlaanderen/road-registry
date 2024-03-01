namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Framework.Testing;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using AcceptedChange = RoadRegistry.BackOffice.Messages.AcceptedChange;
using AddRoadNode = RoadRegistry.BackOffice.Messages.AddRoadNode;
using AddRoadSegment = RoadRegistry.BackOffice.Messages.AddRoadSegment;
using GeometryTranslator = RoadRegistry.BackOffice.GeometryTranslator;
using LineString = NetTopologySuite.Geometries.LineString;
using ModifyRoadNode = RoadRegistry.BackOffice.Messages.ModifyRoadNode;
using Point = NetTopologySuite.Geometries.Point;
using Problem = RoadRegistry.BackOffice.Messages.Problem;
using ProblemParameter = RoadRegistry.BackOffice.Messages.ProblemParameter;
using ProblemSeverity = RoadRegistry.BackOffice.Messages.ProblemSeverity;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RoadSegmentEuropeanRoadAttribute = RoadRegistry.BackOffice.Messages.RoadSegmentEuropeanRoadAttribute;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentNationalRoadAttribute = RoadRegistry.BackOffice.Messages.RoadSegmentNationalRoadAttribute;
using RoadSegmentNumberedRoadAttribute = RoadRegistry.BackOffice.Messages.RoadSegmentNumberedRoadAttribute;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class ModifyRoadNodeScenarios : RoadRegistryTestBase
{
    public ModifyRoadNodeScenarios(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        ObjectProvider.CustomizePoint();
        ObjectProvider.CustomizePolylineM();

        ObjectProvider.CustomizeAttributeId();
        ObjectProvider.CustomizeOrganizationId();
        ObjectProvider.CustomizeOrganizationName();
        ObjectProvider.CustomizeRoadNodeId();
        ObjectProvider.CustomizeRoadNodeType();
        ObjectProvider.CustomizeRoadSegmentId();
        ObjectProvider.CustomizeRoadSegmentCategory();
        ObjectProvider.CustomizeRoadSegmentMorphology();
        ObjectProvider.CustomizeRoadSegmentStatus();
        ObjectProvider.CustomizeRoadSegmentAccessRestriction();
        ObjectProvider.CustomizeRoadSegmentLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadDirection();
        ObjectProvider.CustomizeRoadSegmentGeometryDrawMethod();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadOrdinal();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentWidth();
        ObjectProvider.CustomizeEuropeanRoadNumber();
        ObjectProvider.CustomizeNationalRoadNumber();
        ObjectProvider.CustomizeNumberedRoadNumber();
        ObjectProvider.CustomizeOriginProperties();
        ObjectProvider.CustomizeGradeSeparatedJunctionId();
        ObjectProvider.CustomizeGradeSeparatedJunctionType();
        ObjectProvider.CustomizeArchiveId();
        ObjectProvider.CustomizeRoadSegmentGeometryDrawMethod();
        ObjectProvider.CustomizeChangeRequestId();
        ObjectProvider.CustomizeReason();
        ObjectProvider.CustomizeOperatorName();
        ObjectProvider.CustomizeTransactionId();

        ObjectProvider.Customize<RoadSegmentEuropeanRoadAttribute>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<EuropeanRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNationalRoadAttribute>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<NationalRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNumberedRoadAttribute>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.Number = ObjectProvider.Create<NumberedRoadNumber>();
                instance.Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>();
            }).OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentLaneAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(ObjectProvider);
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Count = ObjectProvider.Create<RoadSegmentLaneCount>();
                instance.Direction = ObjectProvider.Create<RoadSegmentLaneDirection>();
            }).OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentWidthAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(ObjectProvider);
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Width = ObjectProvider.Create<RoadSegmentWidth>();
            }).OmitAutoProperties());
        ObjectProvider.Customize<RequestedRoadSegmentSurfaceAttribute>(composer =>
            composer.Do(instance =>
            {
                var positionGenerator = new Generator<RoadSegmentPosition>(ObjectProvider);
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.FromPosition = positionGenerator.First(candidate => candidate >= 0.0m);
                instance.ToPosition = positionGenerator.First(candidate => candidate > instance.FromPosition);
                instance.Type = ObjectProvider.Create<RoadSegmentSurfaceType>();
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
                })
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

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
                })
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

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
                })
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        AddStartNode1 = new AddRoadNode
        {
            TemporaryId = ObjectProvider.Create<RoadNodeId>(),
            Geometry = GeometryTranslator.Translate(StartPoint1),
            Type = RoadNodeType.EndNode
        };

        StartNode1Added = new RoadNodeAdded
        {
            Id = 1,
            TemporaryId = AddStartNode1.TemporaryId,
            Geometry = AddStartNode1.Geometry,
            Type = AddStartNode1.Type,
            Version = 1
        };

        ModifyStartNode1 = new ModifyRoadNode
        {
            Id = 1,
            Geometry = GeometryTranslator.Translate(StartPoint1),
            Type = RoadNodeType.EndNode
        };

        StartNode1Modified = new RoadNodeModified
        {
            Id = StartNode1Added.Id,
            Geometry = StartNode1Added.Geometry,
            Type = StartNode1Added.Type,
            Version = 2
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
            Type = AddEndNode1.Type,
            Version = 1
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
            Type = ModifyEndNode1.Type,
            Version = 2
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
            Type = AddStartNode2.Type,
            Version = 1
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
            Type = AddEndNode2.Type,
            Version = 1
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
            Type = AddStartNode3.Type,
            Version = 1
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
            Type = AddEndNode3.Type,
            Version = 1
        };

        var laneCount1 = new Random().Next(1, 10);
        var widthCount1 = new Random().Next(1, 10);
        var surfaceCount1 = new Random().Next(1, 10);
        AddSegment1 = new AddRoadSegment
        {
            TemporaryId = ObjectProvider.Create<RoadSegmentId>(),
            StartNodeId = AddStartNode1.TemporaryId,
            EndNodeId = AddEndNode1.TemporaryId,
            Geometry = GeometryTranslator.Translate(MultiLineString1),
            MaintenanceAuthority = ObjectProvider.Create<OrganizationId>(),
            GeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>(),
            Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
            Status = ObjectProvider.Create<RoadSegmentStatus>(),
            Category = ObjectProvider.Create<RoadSegmentCategory>(),
            AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
            LeftSideStreetNameId = ObjectProvider.Create<int?>(),
            RightSideStreetNameId = ObjectProvider.Create<int?>(),
            Lanes = ObjectProvider
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
            Widths = ObjectProvider
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
            Surfaces = ObjectProvider
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
            GeometryVersion = 1,
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
            Lanes = AddSegment1.Lanes.ToRoadSegmentLaneAttributes(startAttributeId: 0),
            Widths = AddSegment1.Widths.ToRoadSegmentWidthAttributes(startAttributeId: 0),
            Surfaces = AddSegment1.Surfaces.ToRoadSegmentSurfaceAttributes(startAttributeId: 0),
            Version = 1
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
            MaintenanceAuthority = ObjectProvider.Create<OrganizationId>(),
            GeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>(),
            Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
            Status = ObjectProvider.Create<RoadSegmentStatus>(),
            Category = ObjectProvider.Create<RoadSegmentCategory>(),
            AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
            LeftSideStreetNameId = ObjectProvider.Create<int?>(),
            RightSideStreetNameId = ObjectProvider.Create<int?>(),
            Lanes = ObjectProvider
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
            Widths = ObjectProvider
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
            Surfaces = ObjectProvider
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
            GeometryVersion = 1,
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
            Lanes = AddSegment2.Lanes.ToRoadSegmentLaneAttributes(startAttributeId: laneCount1),
            Widths = AddSegment2.Widths.ToRoadSegmentWidthAttributes(startAttributeId: widthCount1),
            Surfaces = AddSegment2.Surfaces.ToRoadSegmentSurfaceAttributes(startAttributeId: surfaceCount1),
            Version = 1
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
            MaintenanceAuthority = ObjectProvider.Create<OrganizationId>(),
            GeometryDrawMethod = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>(),
            Morphology = ObjectProvider.Create<RoadSegmentMorphology>(),
            Status = ObjectProvider.Create<RoadSegmentStatus>(),
            Category = ObjectProvider.Create<RoadSegmentCategory>(),
            AccessRestriction = ObjectProvider.Create<RoadSegmentAccessRestriction>(),
            LeftSideStreetNameId = ObjectProvider.Create<int?>(),
            RightSideStreetNameId = ObjectProvider.Create<int?>(),
            Lanes = ObjectProvider
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
            Widths = ObjectProvider
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
            Surfaces = ObjectProvider
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
            GeometryVersion = 1,
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
            Lanes = AddSegment3.Lanes.ToRoadSegmentLaneAttributes(startAttributeId: laneCount1 + laneCount2),
            Widths = AddSegment3.Widths.ToRoadSegmentWidthAttributes(startAttributeId: widthCount1 + widthCount2),
            Surfaces = AddSegment3.Surfaces.ToRoadSegmentSurfaceAttributes(startAttributeId: surfaceCount1 + surfaceCount2),
            Version = 1
        };

        ArchiveId = ObjectProvider.Create<ArchiveId>();
        RequestId = ChangeRequestId.FromArchiveId(ArchiveId);
        ReasonForChange = ObjectProvider.Create<Reason>();
        ChangedByOperator = ObjectProvider.Create<OperatorName>();
        ChangedByOrganization = ObjectProvider.Create<OrganizationId>();
        ChangedByOrganizationName = ObjectProvider.Create<OrganizationName>();
        TransactionId = ObjectProvider.Create<TransactionId>();
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

    [Fact]
    public Task when_modifying_a_node_with_the_same_values()
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
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = Segment1Added,
                        Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = StartNode1Modified,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_modifying_a_node_with_the_same_values_with_renamed_organization()
    {
        return Run(scenario => scenario
            .Given(Organizations.ToStreamName(ChangedByOrganization),
                new ImportedOrganization
                {
                    Code = ChangedByOrganization,
                    Name = ChangedByOrganizationName.WithMaxLength(50) + "DUMMY_AFFIX",
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new RenameOrganizationAccepted
                {
                    Code = ChangedByOrganization,
                    Name = ChangedByOrganizationName,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }
            )
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = Segment1Added,
                        Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = StartNode1Modified,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }

    [Fact]
    public Task when_modifying_a_start_node_connected_to_a_single_segment_to_a_type_other_than_end_node()
    {
        ModifyStartNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
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
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = Segment1Added,
                        Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        ModifyRoadNode = ModifyStartNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = ModifyStartNode1.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "1"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = Segment1Added.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = ModifyStartNode1.Type
                                    },
                                    new ProblemParameter
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
    public async Task when_modifying_a_start_node_connecting_two_segments_as_a_fake_node_and_the_segments_differ_by_one_attribute()
    {
        var startPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        ModifyStartNode1.Type = RoadNodeType.FakeNode;
        ModifyStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        StartNode1Modified.Type = RoadNodeType.FakeNode;
        StartNode1Modified.Geometry = GeometryTranslator.Translate(startPoint);

        StartNode1Added.Type = RoadNodeType.FakeNode;
        StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint);
        EndNode1Added.Type = RoadNodeType.EndNode;
        EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint1);
        EndNode2Added.Type = RoadNodeType.EndNode;
        EndNode2Added.Geometry = GeometryTranslator.Translate(endPoint2);
        Segment1Added.StartNodeId = StartNode1Added.Id;
        Segment1Added.EndNodeId = EndNode1Added.Id;
        Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment1Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint.Coordinate, endPoint1.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        Segment2Added.StartNodeId = StartNode1Added.Id;
        Segment2Added.EndNodeId = EndNode2Added.Id;
        Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment2Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
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
                Segment2Added.Status = new Generator<RoadSegmentStatus>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentStatus.Parse(Segment1Added.Status));
                break;
            case 1:
                Segment2Added.Morphology = new Generator<RoadSegmentMorphology>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentMorphology.Parse(Segment1Added.Morphology));
                break;
            case 2:
                Segment2Added.Category = new Generator<RoadSegmentCategory>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentCategory.Parse(Segment1Added.Category));
                break;
            case 3:
                Segment2Added.MaintenanceAuthority.Code = new Generator<OrganizationId>(ObjectProvider)
                    .First(candidate => candidate != new OrganizationId(Segment1Added.MaintenanceAuthority.Code));
                break;
            case 4:
                Segment2Added.AccessRestriction = new Generator<RoadSegmentAccessRestriction>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentAccessRestriction.Parse(Segment1Added.AccessRestriction));
                break;
            case 5:
                Segment2Added.LeftSide.StreetNameId = new Generator<CrabStreetNameId?>(ObjectProvider)
                    .First(candidate => candidate != (Segment1Added.LeftSide.StreetNameId.HasValue
                        ? new CrabStreetNameId(Segment1Added.LeftSide.StreetNameId.Value)
                        : new CrabStreetNameId?()));
                break;
            case 6:
                Segment2Added.RightSide.StreetNameId = new Generator<CrabStreetNameId?>(ObjectProvider)
                    .First(candidate => candidate != (Segment1Added.RightSide.StreetNameId.HasValue
                        ? new CrabStreetNameId(Segment1Added.RightSide.StreetNameId.Value)
                        : new CrabStreetNameId?()));
                break;
        }

        await Run(scenario => scenario
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
                    RequestId = RequestId,
                    Reason = ReasonForChange,
                    Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization,
                    Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added, Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = StartNode1Modified,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_modifying_a_start_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
    {
        var startPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        ModifyStartNode1.Type = RoadNodeType.FakeNode;
        ModifyStartNode1.Geometry = GeometryTranslator.Translate(startPoint);
        StartNode1Modified.Type = RoadNodeType.FakeNode;
        StartNode1Modified.Geometry = GeometryTranslator.Translate(startPoint);

        StartNode1Added.Type = RoadNodeType.FakeNode;
        StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint);
        EndNode1Added.Type = RoadNodeType.EndNode;
        EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint1);
        EndNode2Added.Type = RoadNodeType.EndNode;
        EndNode2Added.Geometry = GeometryTranslator.Translate(endPoint2);
        Segment1Added.StartNodeId = StartNode1Added.Id;
        Segment1Added.EndNodeId = EndNode1Added.Id;
        Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment1Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint.Coordinate, endPoint1.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        Segment2Added.StartNodeId = StartNode1Added.Id;
        Segment2Added.EndNodeId = EndNode2Added.Id;
        Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment2Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
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
        Segment2Added.GeometryDrawMethod = Segment1Added.GeometryDrawMethod;

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
                    RequestId = RequestId,
                    Reason = ReasonForChange,
                    Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization,
                    Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added, Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = StartNode1Modified,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = ModifyStartNode1.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = Segment1Added.Id.ToString()
                                    },
                                    new ProblemParameter
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
    public Task when_modifying_a_start_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
    {
        var startPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        ModifyStartNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
            .ToString();
        ModifyStartNode1.Geometry = GeometryTranslator.Translate(startPoint);

        StartNode1Added.Type = RoadNodeType.FakeNode;
        StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint);
        EndNode1Added.Type = RoadNodeType.EndNode;
        EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint1);
        EndNode2Added.Type = RoadNodeType.EndNode;
        EndNode2Added.Geometry = GeometryTranslator.Translate(endPoint2);
        Segment1Added.StartNodeId = StartNode1Added.Id;
        Segment1Added.EndNodeId = EndNode1Added.Id;
        Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment1Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint.Coordinate, endPoint1.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        Segment2Added.StartNodeId = StartNode1Added.Id;
        Segment2Added.EndNodeId = EndNode2Added.Id;
        Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment2Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
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
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId,
                    Reason = ReasonForChange,
                    Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization,
                    Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode2Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added, Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        ModifyRoadNode = ModifyStartNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = ModifyStartNode1.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = Segment1Added.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = Segment2Added.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = ModifyStartNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
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
    public Task when_modifying_an_end_node_connected_to_a_single_segment_to_a_type_other_than_end_node()
    {
        ModifyEndNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
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
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = Segment1Added,
                        Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        ModifyRoadNode = ModifyEndNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = ModifyEndNode1.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "1"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = Segment1Added.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = ModifyEndNode1.Type
                                    },
                                    new ProblemParameter
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
    public Task when_modifying_an_end_node_connecting_two_segments_as_a_fake_node_and_the_segments_differ_by_one_attribute()
    {
        var endPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        ModifyEndNode1.Type = RoadNodeType.FakeNode;
        ModifyEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
        EndNode1Modified.Type = RoadNodeType.FakeNode;
        EndNode1Modified.Geometry = GeometryTranslator.Translate(endPoint);

        StartNode1Added.Type = RoadNodeType.EndNode;
        StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint1);
        StartNode2Added.Type = RoadNodeType.EndNode;
        StartNode2Added.Geometry = GeometryTranslator.Translate(startPoint2);
        EndNode1Added.Type = RoadNodeType.FakeNode;
        EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint);

        Segment1Added.StartNodeId = StartNode1Added.Id;
        Segment1Added.EndNodeId = EndNode1Added.Id;
        Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment1Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint1.Coordinate, endPoint.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        Segment2Added.StartNodeId = StartNode2Added.Id;
        Segment2Added.EndNodeId = EndNode1Added.Id;
        Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment2Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
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
                Segment2Added.Status = new Generator<RoadSegmentStatus>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentStatus.Parse(Segment1Added.Status));
                break;
            case 1:
                Segment2Added.Morphology = new Generator<RoadSegmentMorphology>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentMorphology.Parse(Segment1Added.Morphology));
                break;
            case 2:
                Segment2Added.Category = new Generator<RoadSegmentCategory>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentCategory.Parse(Segment1Added.Category));
                break;
            case 3:
                Segment2Added.MaintenanceAuthority.Code = new Generator<OrganizationId>(ObjectProvider)
                    .First(candidate => candidate != new OrganizationId(Segment1Added.MaintenanceAuthority.Code));
                break;
            case 4:
                Segment2Added.AccessRestriction = new Generator<RoadSegmentAccessRestriction>(ObjectProvider)
                    .First(candidate => candidate != RoadSegmentAccessRestriction.Parse(Segment1Added.AccessRestriction));
                break;
            case 5:
                Segment2Added.LeftSide.StreetNameId = new Generator<CrabStreetNameId?>(ObjectProvider)
                    .First(candidate => candidate != (Segment1Added.LeftSide.StreetNameId.HasValue ? new CrabStreetNameId(Segment1Added.LeftSide.StreetNameId.Value) : new CrabStreetNameId?()));
                break;
            case 6:
                Segment2Added.RightSide.StreetNameId = new Generator<CrabStreetNameId?>(ObjectProvider)
                    .First(candidate => candidate != (Segment1Added.RightSide.StreetNameId.HasValue ? new CrabStreetNameId(Segment1Added.RightSide.StreetNameId.Value) : new CrabStreetNameId?()));
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
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId,
                    Reason = ReasonForChange,
                    Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization,
                    Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode2Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added, Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = EndNode1Modified,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_modifying_an_end_node_connecting_two_segments_as_a_fake_node_but_the_segments_do_not_differ_by_any_attribute()
    {
        var endPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        ModifyEndNode1.Type = RoadNodeType.FakeNode;
        ModifyEndNode1.Geometry = GeometryTranslator.Translate(endPoint);
        EndNode1Modified.Type = RoadNodeType.FakeNode;
        EndNode1Modified.Geometry = GeometryTranslator.Translate(endPoint);

        StartNode1Added.Type = RoadNodeType.EndNode;
        StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint1);
        StartNode2Added.Type = RoadNodeType.EndNode;
        StartNode2Added.Geometry = GeometryTranslator.Translate(startPoint2);
        EndNode1Added.Type = RoadNodeType.FakeNode;
        EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint);

        Segment1Added.StartNodeId = StartNode1Added.Id;
        Segment1Added.EndNodeId = EndNode1Added.Id;
        Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment1Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint1.Coordinate, endPoint.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        Segment2Added.StartNodeId = StartNode2Added.Id;
        Segment2Added.EndNodeId = EndNode1Added.Id;
        Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment2Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
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
        Segment2Added.GeometryDrawMethod = Segment1Added.GeometryDrawMethod;

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
                    RequestId = RequestId,
                    Reason = ReasonForChange,
                    Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization,
                    Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode2Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added, Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = EndNode1Modified,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "FakeRoadNodeConnectedSegmentsDoNotDiffer",
                                Severity = ProblemSeverity.Warning,
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = ModifyEndNode1.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "SegmentId",
                                        Value = Segment1Added.Id.ToString()
                                    },
                                    new ProblemParameter
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
    public Task when_modifying_an_end_node_connecting_two_segments_as_a_node_other_than_a_fake_node_or_turning_loop_node()
    {
        var endPoint = new Point(new CoordinateM(10.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var startPoint2 = new Point(new CoordinateM(20.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        ModifyEndNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
            .First(type => type != RoadNodeType.FakeNode && type != RoadNodeType.TurningLoopNode)
            .ToString();
        ModifyEndNode1.Geometry = GeometryTranslator.Translate(endPoint);

        StartNode1Added.Type = RoadNodeType.EndNode;
        StartNode1Added.Geometry = GeometryTranslator.Translate(startPoint1);
        StartNode2Added.Type = RoadNodeType.EndNode;
        StartNode2Added.Geometry = GeometryTranslator.Translate(startPoint2);
        EndNode1Added.Type = RoadNodeType.FakeNode;
        EndNode1Added.Geometry = GeometryTranslator.Translate(endPoint);

        Segment1Added.StartNodeId = StartNode1Added.Id;
        Segment1Added.EndNodeId = EndNode1Added.Id;
        Segment1Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment1Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment1Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment1Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    startPoint1.Coordinate, endPoint.Coordinate
                }), GeometryConfiguration.GeometryFactory)
            })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            });
        Segment2Added.StartNodeId = StartNode2Added.Id;
        Segment2Added.EndNodeId = EndNode1Added.Id;
        Segment2Added.Lanes = Array.Empty<RoadSegmentLaneAttributes>();
        Segment2Added.Widths = Array.Empty<RoadSegmentWidthAttributes>();
        Segment2Added.Surfaces = Array.Empty<RoadSegmentSurfaceAttributes>();
        Segment2Added.Geometry = GeometryTranslator.Translate(
            new MultiLineString(new[]
            {
                new LineString(new CoordinateArraySequence(new[]
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
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    RequestId = RequestId,
                    Reason = ReasonForChange,
                    Operator = ChangedByOperator,
                    OrganizationId = ChangedByOrganization,
                    Organization = ChangedByOrganizationName,
                    TransactionId = new TransactionId(1),
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = StartNode2Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = EndNode1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment1Added, Problems = Array.Empty<Problem>()
                        },
                        new AcceptedChange
                        {
                            RoadSegmentAdded = Segment2Added, Problems = Array.Empty<Problem>()
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
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                Changes = new[]
                {
                    new RejectedChange
                    {
                        ModifyRoadNode = ModifyEndNode1,
                        Problems = new[]
                        {
                            new Problem
                            {
                                Reason = "RoadNodeTypeMismatch",
                                Parameters = new[]
                                {
                                    new ProblemParameter
                                    {
                                        Name = "RoadNodeId",
                                        Value = ModifyEndNode1.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentCount",
                                        Value = "2"
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = Segment1Added.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "ConnectedSegmentId",
                                        Value = Segment2Added.Id.ToString()
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Actual",
                                        Value = ModifyEndNode1.Type
                                    },
                                    new ProblemParameter
                                    {
                                        Name = "Expected",
                                        Value = "FakeNode"
                                    },
                                    new ProblemParameter
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
    public Task when_no_modifications()
    {
        ModifyStartNode1.Type = new Generator<RoadNodeType>(ObjectProvider)
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
            .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
            {
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeAdded = StartNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = EndNode1Added,
                        Problems = Array.Empty<Problem>()
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = Segment1Added,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }, new RoadNetworkChangesAccepted
            {
                RequestId = "test",
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(1),
                Changes = new[]
                {
                    new AcceptedChange
                    {
                        RoadNodeModified = StartNode1Modified,
                        Problems = Array.Empty<Problem>()
                    }
                },
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheOperator.ChangesTheRoadNetwork(RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization))
            .Then(RoadNetworks.Stream, new NoRoadNetworkChanges
            {
                RequestId = RequestId,
                Reason = ReasonForChange,
                Operator = ChangedByOperator,
                OrganizationId = ChangedByOrganization,
                Organization = ChangedByOrganizationName,
                TransactionId = new TransactionId(2),
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }));
    }
}
