namespace RoadRegistry.Tests.BackOffice.Scenarios;

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

public class RoadNetworkTestData
{
    public RoadNetworkTestData(Action<Fixture> customize = null)
    {
        ObjectProvider = new Fixture();
        ObjectProvider.CustomizePoint();
        ObjectProvider.CustomizePolylineM();

        ObjectProvider.CustomizeOrganisation();
        ObjectProvider.CustomizeProvenanceData();

        ObjectProvider.CustomizeAttributeId();
        ObjectProvider.CustomizeOrganizationId();
        ObjectProvider.CustomizeOrganizationName();
        ObjectProvider.CustomizeRoadNodeId();
        ObjectProvider.CustomizeRoadNodeVersion();
        ObjectProvider.CustomizeRoadNodeType();
        ObjectProvider.CustomizeRoadSegmentId();
        ObjectProvider.CustomizeRoadSegmentCategory();
        ObjectProvider.CustomizeRoadSegmentMorphology();
        ObjectProvider.CustomizeRoadSegmentStatus();
        ObjectProvider.CustomizeRoadSegmentAccessRestriction();
        ObjectProvider.CustomizeRoadSegmentLaneAttribute();
        ObjectProvider.CustomizeRoadSegmentLaneAttributes();
        ObjectProvider.CustomizeRoadSegmentLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadDirection();
        ObjectProvider.CustomizeRoadSegmentGeometry();
        ObjectProvider.CustomizeRoadSegmentGeometryDrawMethod();
        ObjectProvider.CustomizeRoadSegmentNumberedRoadOrdinal();
        ObjectProvider.CustomizeRoadSegmentSurfaceAttribute();
        ObjectProvider.CustomizeRoadSegmentSurfaceAttributes();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentWidthAttribute();
        ObjectProvider.CustomizeRoadSegmentWidthAttributes();
        ObjectProvider.CustomizeRoadSegmentWidth();
        ObjectProvider.CustomizeEuropeanRoadNumber();
        ObjectProvider.CustomizeNationalRoadNumber();
        ObjectProvider.CustomizeNumberedRoadNumber();
        ObjectProvider.CustomizeOriginProperties();
        ObjectProvider.CustomizeGradeSeparatedJunctionId();
        ObjectProvider.CustomizeGradeSeparatedJunctionType();
        ObjectProvider.CustomizeArchiveId();
        ObjectProvider.CustomizeChangeRequestId();
        ObjectProvider.CustomizeReason();
        ObjectProvider.CustomizeOperatorName();
        ObjectProvider.CustomizeTransactionId();

        ObjectProvider.CustomizeRoadNetworkChangesAccepted();

        ObjectProvider.Customize<RoadSegmentEuropeanRoadAttributes>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<EuropeanRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNationalRoadAttributes>(composer =>
            composer.Do(instance =>
                {
                    instance.AttributeId = ObjectProvider.Create<AttributeId>();
                    instance.Number = ObjectProvider.Create<NationalRoadNumber>();
                })
                .OmitAutoProperties());
        ObjectProvider.Customize<RoadSegmentNumberedRoadAttributes>(composer =>
            composer.Do(instance =>
            {
                instance.AttributeId = ObjectProvider.Create<AttributeId>();
                instance.Number = ObjectProvider.Create<NumberedRoadNumber>();
                instance.Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>();
                instance.Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>();
            }).OmitAutoProperties());
        
        

        customize?.Invoke(ObjectProvider);

        ArchiveId = ObjectProvider.Create<ArchiveId>();
        RequestId = ChangeRequestId.FromArchiveId(ArchiveId);
        ReasonForChange = ObjectProvider.Create<Reason>();
        ChangedByOperator = ObjectProvider.Create<OperatorName>();
        ChangedByOrganization = ObjectProvider.Create<OrganizationId>();
        ChangedByOrganizationName = ObjectProvider.Create<OrganizationName>();
        TransactionId = ObjectProvider.Create<TransactionId>();

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
            Id = EndNode1Added.Id,
            Geometry = EndNode1Added.Geometry,
            Type = EndNode1Added.Type,
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

        var geometryDrawMethod1 = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>();
        var laneCount1 = geometryDrawMethod1 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        var widthCount1 = geometryDrawMethod1 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        var surfaceCount1 = geometryDrawMethod1 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);

        AddSegment1 = new AddRoadSegment
        {
            TemporaryId = ObjectProvider.Create<RoadSegmentId>(),
            StartNodeId = AddStartNode1.TemporaryId,
            EndNodeId = AddEndNode1.TemporaryId,
            Geometry = GeometryTranslator.Translate(MultiLineString1),
            MaintenanceAuthority = ChangedByOrganization,
            GeometryDrawMethod = geometryDrawMethod1,
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
                .ToArray()
        };

        ModifySegment1 = new ModifyRoadSegment
        {
            AccessRestriction = Segment1Added.AccessRestriction,
            Category = Segment1Added.Category,
            EndNodeId = Segment1Added.EndNodeId,
            Geometry = Segment1Added.Geometry,
            GeometryDrawMethod = Segment1Added.GeometryDrawMethod,
            Id = Segment1Added.Id,
            Lanes = Segment1Added.Lanes.Select(lane => new RequestedRoadSegmentLaneAttribute
            {
                AttributeId = lane.AttributeId,
                Count = lane.Count,
                Direction = lane.Direction,
                FromPosition = lane.FromPosition,
                ToPosition = lane.ToPosition
            }).ToArray(),
            LeftSideStreetNameId = Segment1Added.LeftSide.StreetNameId,
            MaintenanceAuthority = Segment1Added.MaintenanceAuthority.Code,
            Morphology = Segment1Added.Morphology,
            RightSideStreetNameId = Segment1Added.RightSide.StreetNameId,
            StartNodeId = Segment1Added.StartNodeId,
            Status = Segment1Added.Status,
            Surfaces = Segment1Added.Surfaces.Select(surface => new RequestedRoadSegmentSurfaceAttribute
            {
                AttributeId = surface.AttributeId,
                FromPosition = surface.FromPosition,
                ToPosition = surface.ToPosition,
                Type = surface.Type
            }).ToArray(),
            Widths = Segment1Added.Widths.Select(width => new RequestedRoadSegmentWidthAttribute
            {
                AttributeId = width.AttributeId,
                FromPosition = width.FromPosition,
                ToPosition = width.ToPosition,
                Width = width.Width
            }).ToArray()
        };

        Segment1Modified = new RoadSegmentModified
        {
            Version = 2,
            GeometryVersion = 1,
            AccessRestriction = Segment1Added.AccessRestriction,
            Category = Segment1Added.Category,
            EndNodeId = Segment1Added.EndNodeId,
            Geometry = Segment1Added.Geometry,
            GeometryDrawMethod = Segment1Added.GeometryDrawMethod,
            Id = Segment1Added.Id,
            Lanes = Segment1Added.Lanes,
            LeftSide = Segment1Added.LeftSide,
            MaintenanceAuthority = Segment1Added.MaintenanceAuthority,
            Morphology = Segment1Added.Morphology,
            RightSide = Segment1Added.RightSide,
            StartNodeId = Segment1Added.StartNodeId,
            Status = Segment1Added.Status,
            Surfaces = Segment1Added.Surfaces,
            Widths = Segment1Added.Widths
        };

        var geometryDrawMethod2 = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>();
        var laneCount2 = geometryDrawMethod2 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        var widthCount2 = geometryDrawMethod2 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        var surfaceCount2 = geometryDrawMethod2 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        AddSegment2 = new AddRoadSegment
        {
            TemporaryId = AddSegment1.TemporaryId + 1,
            StartNodeId = AddStartNode2.TemporaryId,
            EndNodeId = AddEndNode2.TemporaryId,
            Geometry = GeometryTranslator.Translate(MultiLineString2),
            MaintenanceAuthority = ChangedByOrganization,
            GeometryDrawMethod = geometryDrawMethod2,
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
                .ToArray()
        };

        var geometryDrawMethod3 = ObjectProvider.Create<RoadSegmentGeometryDrawMethod>();
        var laneCount3 = geometryDrawMethod3 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        var widthCount3 = geometryDrawMethod3 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        var surfaceCount3 = geometryDrawMethod3 == RoadSegmentGeometryDrawMethod.Outlined ? 1 : new Random().Next(1, 10);
        AddSegment3 = new AddRoadSegment
        {
            TemporaryId = AddSegment2.TemporaryId + 1,
            StartNodeId = AddStartNode3.TemporaryId,
            EndNodeId = AddEndNode3.TemporaryId,
            Geometry = GeometryTranslator.Translate(MultiLineString3),
            MaintenanceAuthority = ObjectProvider.Create<OrganizationId>(),
            GeometryDrawMethod = geometryDrawMethod3,
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
                .ToArray()
        };
    }

    public Fixture ObjectProvider { get; }
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
    public ModifyRoadSegment ModifySegment1 { get; }
    public RoadSegmentModified Segment1Modified { get; }
    public MultiLineString MultiLineString1 { get; }
    public MultiLineString MultiLineString2 { get; }
    public MultiLineString MultiLineString3 { get; }

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
                new Problem
                {
                    Reason = "RoadSegmentLaneAttributeFromPositionNotEqualToZero",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId.ToInt32().ToString()
                        },
                        new ProblemParameter
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
                new Problem
                {
                    Reason = "RoadSegmentLaneAttributesNotAdjacent",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId1.ToInt32().ToString()
                        },
                        new ProblemParameter
                        {
                            Name = "ToPosition",
                            Value = "50.0"
                        },
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId2.ToInt32().ToString()
                        },
                        new ProblemParameter
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
                    new RequestedRoadSegmentLaneAttribute
                    {
                        AttributeId = attributeId3,
                        FromPosition = 0.0m,
                        ToPosition = 50.0m,
                        Count = fixture.Create<RoadSegmentLaneCount>(),
                        Direction = fixture.Create<RoadSegmentLaneDirection>()
                    }
                },
                new Problem
                {
                    Reason = "RoadSegmentLaneAttributeToPositionNotEqualToLength",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId3.ToInt32().ToString()
                        },
                        new ProblemParameter
                        {
                            Name = "ToPosition",
                            Value = "50.0"
                        },
                        new ProblemParameter
                        {
                            Name = "Length",
                            Value = "141.421"
                        }
                    }
                }
            };
        }
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
                new Problem
                {
                    Reason = "RoadSegmentSurfaceAttributeFromPositionNotEqualToZero",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId.ToInt32().ToString()
                        },
                        new ProblemParameter
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
                new Problem
                {
                    Reason = "RoadSegmentSurfaceAttributesNotAdjacent",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId1.ToInt32().ToString()
                        },
                        new ProblemParameter
                        {
                            Name = "ToPosition",
                            Value = "50.0"
                        },
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId2.ToInt32().ToString()
                        },
                        new ProblemParameter
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
                new Problem
                {
                    Reason = "RoadSegmentSurfaceAttributeToPositionNotEqualToLength",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId3.ToInt32().ToString()
                        },
                        new ProblemParameter
                        {
                            Name = "ToPosition",
                            Value = "50.0"
                        },
                        new ProblemParameter
                        {
                            Name = "Length",
                            Value = "141.421"
                        }
                    }
                }
            };
        }
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
                new Problem
                {
                    Reason = "RoadSegmentWidthAttributeFromPositionNotEqualToZero",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId.ToInt32().ToString()
                        },
                        new ProblemParameter
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
                new Problem
                {
                    Reason = "RoadSegmentWidthAttributesNotAdjacent",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId1.ToInt32().ToString()
                        },
                        new ProblemParameter
                        {
                            Name = "ToPosition",
                            Value = "50.0"
                        },
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId2.ToInt32().ToString()
                        },
                        new ProblemParameter
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
                new Problem
                {
                    Reason = "RoadSegmentWidthAttributeToPositionNotEqualToLength",
                    Parameters = new[]
                    {
                        new ProblemParameter
                        {
                            Name = "AttributeId",
                            Value = attributeId3.ToInt32().ToString()
                        },
                        new ProblemParameter
                        {
                            Name = "ToPosition",
                            Value = "50.0"
                        },
                        new ProblemParameter
                        {
                            Name = "Length",
                            Value = "141.421"
                        }
                    }
                }
            };
        }
    }

    public static IEnumerable<object[]> SelfOverlapsCases
    {
        get
        {
            var startPoint1 = new Point(new CoordinateM(0.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint1 = new Point(new CoordinateM(10.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint1 = new Point(new CoordinateM(5.0, 0.0, 15.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var multiLineString1 = new MultiLineString(
                new[]
                {
                    new LineString(
                        new CoordinateArraySequence(new[] { startPoint1.Coordinate, middlePoint1.Coordinate, endPoint1.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
                })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            //covers itself
            yield return new object[] { startPoint1, endPoint1, multiLineString1 };

            var startPoint2 = new Point(new CoordinateM(5.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint2A = new Point(new CoordinateM(20.0, 0.0, 15.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint2B = new Point(new CoordinateM(20.0, 10.0, 25.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint2C = new Point(new CoordinateM(0.0, 10.0, 45.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint2D = new Point(new CoordinateM(0.0, 0.0, 55.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint2 = new Point(new CoordinateM(8.0, 0.0, 63.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var multiLineString2 = new MultiLineString(
                new[]
                {
                    new LineString(
                        new CoordinateArraySequence(new[] { startPoint2.Coordinate, middlePoint2A.Coordinate, middlePoint2B.Coordinate, middlePoint2C.Coordinate, middlePoint2D.Coordinate, endPoint2.Coordinate }),
                        GeometryConfiguration.GeometryFactory
                    )
                })
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };

            //overlaps itself
            yield return new object[] { startPoint2, endPoint2, multiLineString2 };
        }
    }

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

    public RoadNetworkTestData CopyCustomizationsTo(Fixture target)
    {
        foreach (var customization in ObjectProvider.Customizations)
        {
            target.Customizations.Add(customization);
        }

        return this;
    }
}
