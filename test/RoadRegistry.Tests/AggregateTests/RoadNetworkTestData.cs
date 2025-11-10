namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;
using RoadNodeAdded = RoadRegistry.RoadNode.Events.RoadNodeAdded;
using RoadSegmentAdded = RoadRegistry.RoadSegment.Events.RoadSegmentAdded;

public class RoadNetworkTestData
{
    public Fixture Fixture { get; }
    public Point StartPoint1 { get; }
    public Point StartPoint2 { get; }
    public Point StartPoint3 { get; }
    public Point MiddlePoint1 { get; }
    public Point MiddlePoint2 { get; }
    public Point MiddlePoint3 { get; }
    public Point EndPoint1 { get; }
    public Point EndPoint2 { get; }
    public Point EndPoint3 { get; }
    public MultiLineString MultiLineString1 { get; }
    public MultiLineString MultiLineString2 { get; }
    public MultiLineString MultiLineString3 { get; }
    public AddRoadNodeChange AddStartNode1 { get; }
    public AddRoadNodeChange AddStartNode2 { get; }
    public AddRoadNodeChange AddStartNode3 { get; }
    public RoadNodeAdded StartNode1Added { get; }
    public RoadNodeAdded StartNode2Added { get; }

    public RoadNodeAdded StartNode3Added { get; }
    //public ModifyRoadNodeChange ModifyStartNode1 { get; }
    //public RoadNodeModified StartNode1Modified { get; }

    public AddRoadNodeChange AddEndNode1 { get; }
    public AddRoadNodeChange AddEndNode2 { get; }
    public AddRoadNodeChange AddEndNode3 { get; }
    public RoadNodeAdded EndNode1Added { get; }
    public RoadNodeAdded EndNode2Added { get; }

    public RoadNodeAdded EndNode3Added { get; }
    // public ModifyRoadNodeChange ModifyEndNode1 { get; }
    // public RoadNodeModified EndNode1Modified { get; }

    public AddRoadSegmentChange AddSegment1 { get; }
    public AddRoadSegmentChange AddSegment2 { get; }
    public AddRoadSegmentChange AddSegment3 { get; }
    public RoadSegmentAdded Segment1Added { get; }
    public RoadSegmentAdded Segment2Added { get; }
    public RoadSegmentAdded Segment3Added { get; }

    public RoadNetworkTestData(Action<Fixture> customize = null)
    {
        Fixture = new Fixture();
        Fixture.CustomizePoint();
        Fixture.CustomizePolylineM();

        Fixture.CustomizeOrganisation();
        Fixture.CustomizeProvenanceData();

        Fixture.CustomizeAttributeId();
        Fixture.CustomizeOrganizationId();
        Fixture.CustomizeOrganizationName();
        Fixture.CustomizeOrganizationOvoCode();
        Fixture.CustomizeOrganizationKboNumber();
        Fixture.CustomizeRoadNodeId();
        Fixture.CustomizeRoadNodeVersion();
        Fixture.CustomizeRoadNodeType();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeRoadSegmentCategory();
        Fixture.CustomizeRoadSegmentMorphology();
        Fixture.CustomizeRoadSegmentStatus();
        Fixture.CustomizeRoadSegmentAccessRestriction();
        Fixture.CustomizeRoadSegmentLaneAttribute();
        Fixture.CustomizeRoadSegmentLaneAttributes();
        Fixture.CustomizeRoadSegmentLaneCount();
        Fixture.CustomizeRoadSegmentLaneDirection();
        Fixture.CustomizeRoadSegmentNumberedRoadDirection();
        Fixture.CustomizeRoadSegmentGeometry();
        Fixture.CustomizeRoadSegmentGeometryDrawMethod();
        Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        Fixture.CustomizeRoadSegmentSurfaceAttribute();
        Fixture.CustomizeRoadSegmentSurfaceAttributes();
        Fixture.CustomizeRoadSegmentSurfaceType();
        Fixture.CustomizeRoadSegmentWidthAttribute();
        Fixture.CustomizeRoadSegmentWidthAttributes();
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
        Fixture.CustomizeExtractRequestId();

        Fixture.CustomizeRoadNodeAdded();
        Fixture.CustomizeRoadNodeModified();
        Fixture.CustomizeRoadSegmentDynamicAttributeValues<OrganizationId>();
        Fixture.CustomizeRoadSegmentDynamicAttributeValues<RoadSegmentMorphology>();
        Fixture.CustomizeRoadSegmentDynamicAttributeValues<RoadSegmentStatus>();
        Fixture.CustomizeRoadSegmentDynamicAttributeValues<RoadSegmentCategory>();
        Fixture.CustomizeRoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>();
        Fixture.CustomizeRoadSegmentDynamicAttributeValues<StreetNameLocalId>();
        Fixture.CustomizeRoadSegmentAdded();
        Fixture.CustomizeRoadSegmentModified();
        //
        // ObjectProvider.Customize<RequestedRoadSegmentEuropeanRoadAttribute>(composer =>
        //     composer.Do(instance =>
        //         {
        //             instance.AttributeId = ObjectProvider.Create<AttributeId>();
        //             instance.Number = ObjectProvider.Create<EuropeanRoadNumber>();
        //         })
        //         .OmitAutoProperties());
        // ObjectProvider.Customize<RequestedRoadSegmentNationalRoadAttribute>(composer =>
        //     composer.Do(instance =>
        //         {
        //             instance.AttributeId = ObjectProvider.Create<AttributeId>();
        //             instance.Number = ObjectProvider.Create<NationalRoadNumber>();
        //         })
        //         .OmitAutoProperties());
        // ObjectProvider.Customize<RequestedRoadSegmentNumberedRoadAttribute>(composer =>
        //     composer.Do(instance =>
        //     {
        //         instance.AttributeId = ObjectProvider.Create<AttributeId>();
        //         instance.Number = ObjectProvider.Create<NumberedRoadNumber>();
        //         instance.Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>();
        //         instance.Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>();
        //     }).OmitAutoProperties());

        customize?.Invoke(Fixture);

        StartPoint1 = new Point(new CoordinateM(0.0, 0.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MiddlePoint1 = new Point(new CoordinateM(50.0, 50.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        EndPoint1 = new Point(new CoordinateM(100.0, 100.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MultiLineString1 = new MultiLineString(
            [
                new LineString(
                    new CoordinateArraySequence([StartPoint1.Coordinate, MiddlePoint1.Coordinate, EndPoint1.Coordinate]),
                    GeometryConfiguration.GeometryFactory
                )
            ])
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        StartPoint2 = new Point(new CoordinateM(0.0, 200.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MiddlePoint2 = new Point(new CoordinateM(50.0, 250.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        EndPoint2 = new Point(new CoordinateM(100.0, 300.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MultiLineString2 = new MultiLineString(
            [
                new LineString(
                    new CoordinateArraySequence([StartPoint2.Coordinate, MiddlePoint2.Coordinate, EndPoint2.Coordinate]),
                    GeometryConfiguration.GeometryFactory
                )
            ])
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        StartPoint3 = new Point(new CoordinateM(0.0, 500.0, 0.0)) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MiddlePoint3 = new Point(new CoordinateM(50.0, 550.0, 50.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        EndPoint3 = new Point(new CoordinateM(100.0, 600.0, 100.0 * Math.Sqrt(2.0))) { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };
        MultiLineString3 = new MultiLineString(
            [
                new LineString(
                    new CoordinateArraySequence([StartPoint3.Coordinate, MiddlePoint3.Coordinate, EndPoint3.Coordinate]),
                    GeometryConfiguration.GeometryFactory
                )
            ])
            { SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32() };

        AddStartNode1 = new AddRoadNodeChange
        {
            TemporaryId = Fixture.Create<RoadNodeId>(),
            Geometry = StartPoint1,
            Type = RoadNodeType.EndNode
        };

        StartNode1Added = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(1),
            TemporaryId = AddStartNode1.TemporaryId,
            Geometry = AddStartNode1.Geometry.ToGeometryObject(),
            Type = AddStartNode1.Type
        };

        // ModifyStartNode1 = new ModifyRoadNodeChange
        // {
        //     Id = 1,
        //     Geometry = GeometryTranslator.Translate(StartPoint1),
        //     Type = RoadNodeType.EndNode
        // };
        //
        // StartNode1Modified = new RoadNodeModified
        // {
        //     Id = StartNode1Added.Id,
        //     Geometry = StartNode1Added.Geometry.ToGeometryObject(),
        //     Type = StartNode1Added.Type
        // };

        AddEndNode1 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddStartNode1.TemporaryId + 1),
            Geometry = EndPoint1,
            Type = RoadNodeType.EndNode
        };

        EndNode1Added = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(2),
            TemporaryId = AddEndNode1.TemporaryId,
            Geometry = AddEndNode1.Geometry.ToGeometryObject(),
            Type = AddEndNode1.Type
        };

        // ModifyEndNode1 = new ModifyRoadNodeChange
        // {
        //     Id = 2,
        //     Geometry = StartPoint2,
        //     Type = RoadNodeType.EndNode
        // };
        //
        // EndNode1Modified = new RoadNodeModified
        // {
        //     Id = EndNode1Added.Id,
        //     Geometry = EndNode1Added.Geometry.ToGeometryObject(),
        //     Type = EndNode1Added.Type
        // };

        AddStartNode2 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddEndNode1.TemporaryId + 1),
            Geometry = StartPoint2,
            Type = RoadNodeType.EndNode
        };

        StartNode2Added = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(3),
            TemporaryId = AddStartNode2.TemporaryId,
            Geometry = AddStartNode2.Geometry.ToGeometryObject(),
            Type = AddStartNode2.Type
        };

        AddEndNode2 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddStartNode2.TemporaryId + 1),
            Geometry = EndPoint2,
            Type = RoadNodeType.EndNode
        };

        EndNode2Added = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(4),
            TemporaryId = AddEndNode2.TemporaryId,
            Geometry = AddEndNode2.Geometry.ToGeometryObject(),
            Type = AddEndNode2.Type
        };

        AddStartNode3 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddEndNode2.TemporaryId + 1),
            Geometry = StartPoint3,
            Type = RoadNodeType.EndNode
        };

        StartNode3Added = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(5),
            TemporaryId = AddStartNode3.TemporaryId,
            Geometry = AddStartNode3.Geometry.ToGeometryObject(),
            Type = AddStartNode3.Type
        };

        AddEndNode3 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddStartNode3.TemporaryId + 1),
            Geometry = EndPoint3,
            Type = RoadNodeType.EndNode
        };

        EndNode3Added = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(6),
            TemporaryId = AddEndNode3.TemporaryId,
            Geometry = AddEndNode3.Geometry.ToGeometryObject(),
            Type = AddEndNode3.Type
        };

        AddSegment1 = new AddRoadSegmentChange
        {
            TemporaryId = Fixture.Create<RoadSegmentId>(),
            StartNodeId = AddStartNode1.TemporaryId,
            EndNodeId = AddEndNode1.TemporaryId,
            Geometry = MultiLineString1,
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
            MaintenanceAuthorityId = Fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
            Morphology = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>>(),
            Status = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatus>>(),
            Category = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategory>>(),
            AccessRestriction = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>>(),
            StreetNameId = Fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
            SurfaceType = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>>(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        Segment1Added = new RoadSegmentAdded
        {
            RoadSegmentId = new RoadSegmentId(1),
            OriginalId = AddSegment1.TemporaryId,
            StartNodeId = StartNode1Added.RoadNodeId,
            EndNodeId = EndNode1Added.RoadNodeId,
            Geometry = AddSegment1.Geometry.ToGeometryObject(),
            MaintenanceAuthorityId = AddSegment1.MaintenanceAuthorityId,
            GeometryDrawMethod = AddSegment1.GeometryDrawMethod,
            Morphology = AddSegment1.Morphology,
            Status = AddSegment1.Status,
            Category = AddSegment1.Category,
            AccessRestriction = AddSegment1.AccessRestriction,
            StreetNameId = AddSegment1.StreetNameId,
            SurfaceType = AddSegment1.SurfaceType,
            EuropeanRoadNumbers = AddSegment1.EuropeanRoadNumbers,
            NationalRoadNumbers = AddSegment1.NationalRoadNumbers
        };

        AddSegment2 = new AddRoadSegmentChange
        {
            TemporaryId = new RoadSegmentId(AddSegment1.TemporaryId + 1),
            StartNodeId = AddStartNode2.TemporaryId,
            EndNodeId = AddEndNode2.TemporaryId,
            Geometry = MultiLineString2,
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
            MaintenanceAuthorityId = Fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
            Morphology = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>>(),
            Status = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatus>>(),
            Category = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategory>>(),
            AccessRestriction = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>>(),
            StreetNameId = Fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
            SurfaceType = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>>(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        Segment2Added = new RoadSegmentAdded
        {
            RoadSegmentId = new RoadSegmentId(2),
            OriginalId = AddSegment2.TemporaryId,
            StartNodeId = StartNode2Added.RoadNodeId,
            EndNodeId = EndNode2Added.RoadNodeId,
            Geometry = AddSegment2.Geometry.ToGeometryObject(),
            MaintenanceAuthorityId = AddSegment2.MaintenanceAuthorityId,
            GeometryDrawMethod = AddSegment2.GeometryDrawMethod,
            Morphology = AddSegment2.Morphology,
            Status = AddSegment2.Status,
            Category = AddSegment2.Category,
            AccessRestriction = AddSegment2.AccessRestriction,
            StreetNameId = AddSegment2.StreetNameId,
            SurfaceType = AddSegment2.SurfaceType,
            EuropeanRoadNumbers = AddSegment2.EuropeanRoadNumbers,
            NationalRoadNumbers = AddSegment2.NationalRoadNumbers
        };

        AddSegment3 = new AddRoadSegmentChange
        {
            TemporaryId = new RoadSegmentId(AddSegment2.TemporaryId + 1),
            StartNodeId = AddStartNode3.TemporaryId,
            EndNodeId = AddEndNode3.TemporaryId,
            Geometry = MultiLineString3,
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
            MaintenanceAuthorityId = Fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
            Morphology = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>>(),
            Status = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatus>>(),
            Category = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategory>>(),
            AccessRestriction = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>>(),
            StreetNameId = Fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
            SurfaceType = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>>(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        Segment3Added = new RoadSegmentAdded
        {
            RoadSegmentId = new RoadSegmentId(3),
            OriginalId = AddSegment3.TemporaryId,
            StartNodeId = StartNode3Added.RoadNodeId,
            EndNodeId = EndNode3Added.RoadNodeId,
            Geometry = AddSegment3.Geometry.ToGeometryObject(),
            MaintenanceAuthorityId = AddSegment3.MaintenanceAuthorityId,
            GeometryDrawMethod = AddSegment3.GeometryDrawMethod,
            Morphology = AddSegment3.Morphology,
            Status = AddSegment3.Status,
            Category = AddSegment3.Category,
            AccessRestriction = AddSegment3.AccessRestriction,
            StreetNameId = AddSegment3.StreetNameId,
            SurfaceType = AddSegment3.SurfaceType,
            EuropeanRoadNumbers = AddSegment3.EuropeanRoadNumbers,
            NationalRoadNumbers = AddSegment3.NationalRoadNumbers
        };
    }
}
