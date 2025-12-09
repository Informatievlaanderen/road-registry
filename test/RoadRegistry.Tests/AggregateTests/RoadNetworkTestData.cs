namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;
using RoadNodeAdded = RoadRegistry.RoadNode.Events.V2.RoadNodeAdded;
using RoadSegmentAdded = RoadRegistry.RoadSegment.Events.V2.RoadSegmentAdded;

public class RoadNetworkTestData
{
    public Fixture Fixture { get; }

    public Provenance Provenance { get; }
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

    public AddRoadNodeChange AddSegment1StartNode { get; }
    public RoadNodeAdded Segment1StartNodeAdded { get; }
    public AddRoadNodeChange AddSegment2StartNode { get; }
    public RoadNodeAdded Segment2StartNodeAdded { get; }
    public AddRoadNodeChange AddSegment3StartNode { get; }
    public RoadNodeAdded Segment3StartNodeAdded { get; }

    public AddRoadNodeChange AddSegment1EndNode { get; }
    public RoadNodeAdded Segment1EndNodeAdded { get; }
    public AddRoadNodeChange AddSegment2EndNode { get; }
    public RoadNodeAdded Segment2EndNodeAdded { get; }
    public AddRoadNodeChange AddSegment3EndNode { get; }
    public RoadNodeAdded Segment3EndNodeAdded { get; }

    public AddRoadSegmentChange AddSegment1 { get; }
    public RoadSegmentAdded Segment1Added { get; }
    public AddRoadSegmentChange AddSegment2 { get; }
    public RoadSegmentAdded Segment2Added { get; }
    public AddRoadSegmentChange AddSegment3 { get; }
    public RoadSegmentAdded Segment3Added { get; }

    public RoadNetworkTestData(Action<Fixture> customize = null)
    {
        Fixture = new Fixture();
        Fixture.CustomizePoint();
        Fixture.CustomizePolylineM();

        Fixture.CustomizeOrganisation();
        Fixture.CustomizeProvenance();
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
        Fixture.CustomizeRoadSegmentAttributes();
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

        Provenance = Fixture.Create<Provenance>();

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

        AddSegment1StartNode = new AddRoadNodeChange
        {
            TemporaryId = Fixture.Create<RoadNodeId>(),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = StartPoint1,
            Type = RoadNodeType.EndNode
        };

        Segment1StartNodeAdded = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(1),
            OriginalId = AddSegment1StartNode.TemporaryId,
            Geometry = AddSegment1StartNode.Geometry.ToGeometryObject(),
            Type = AddSegment1StartNode.Type,
            Provenance = new ProvenanceData(Provenance)
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

        AddSegment1EndNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment1StartNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = EndPoint1,
            Type = RoadNodeType.EndNode
        };

        Segment1EndNodeAdded = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(2),
            OriginalId = AddSegment1EndNode.TemporaryId,
            Geometry = AddSegment1EndNode.Geometry.ToGeometryObject(),
            Type = AddSegment1EndNode.Type,
            Provenance = new ProvenanceData(Provenance)
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

        AddSegment2StartNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment1EndNode.TemporaryId + 1),
            OriginalId = new RoadNodeId(AddSegment1EndNode.TemporaryId + 1),
            Geometry = StartPoint2,
            Type = RoadNodeType.EndNode
        };

        Segment2StartNodeAdded = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(3),
            OriginalId = AddSegment2StartNode.TemporaryId,
            Geometry = AddSegment2StartNode.Geometry.ToGeometryObject(),
            Type = AddSegment2StartNode.Type,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment2EndNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment2StartNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = EndPoint2,
            Type = RoadNodeType.EndNode
        };

        Segment2EndNodeAdded = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(4),
            OriginalId = AddSegment2EndNode.TemporaryId,
            Geometry = AddSegment2EndNode.Geometry.ToGeometryObject(),
            Type = AddSegment2EndNode.Type,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment3StartNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment2EndNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = StartPoint3,
            Type = RoadNodeType.EndNode
        };

        Segment3StartNodeAdded = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(5),
            OriginalId = AddSegment3StartNode.TemporaryId,
            Geometry = AddSegment3StartNode.Geometry.ToGeometryObject(),
            Type = AddSegment3StartNode.Type,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment3EndNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment3StartNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = EndPoint3,
            Type = RoadNodeType.EndNode
        };

        Segment3EndNodeAdded = new RoadNodeAdded
        {
            RoadNodeId = new RoadNodeId(6),
            OriginalId = AddSegment3EndNode.TemporaryId,
            Geometry = AddSegment3EndNode.Geometry.ToGeometryObject(),
            Type = AddSegment3EndNode.Type,
            Provenance = new ProvenanceData(Provenance)
        };

        var segment1TemporaryId = Fixture.Create<RoadSegmentId>();
        AddSegment1 = new AddRoadSegmentChange
        {
            TemporaryId = segment1TemporaryId,
            OriginalId = segment1TemporaryId,
            StartNodeId = AddSegment1StartNode.TemporaryId,
            EndNodeId = AddSegment1EndNode.TemporaryId,
            Geometry = MultiLineString1,
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethod>(),
            MaintenanceAuthorityId = Fixture.Create<RoadSegmentDynamicAttributeValues<OrganizationId>>(),
            Morphology = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>>(),
            Status = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentStatus>>(),
            Category = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentCategory>>(),
            AccessRestriction = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>>(),
            StreetNameId = Fixture.Create<RoadSegmentDynamicAttributeValues<StreetNameLocalId>>(),
            SurfaceType = Fixture.Create<RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>>(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        Segment1Added = new RoadSegmentAdded
        {
            RoadSegmentId = new RoadSegmentId(1),
            OriginalId = AddSegment1.OriginalId,
            StartNodeId = Segment1StartNodeAdded.RoadNodeId,
            EndNodeId = Segment1EndNodeAdded.RoadNodeId,
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
            NationalRoadNumbers = AddSegment1.NationalRoadNumbers,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment2 = new AddRoadSegmentChange
        {
            TemporaryId = new RoadSegmentId(AddSegment1.TemporaryId + 1),
            OriginalId = new RoadSegmentId(AddSegment1.TemporaryId + 1),
            StartNodeId = AddSegment2StartNode.TemporaryId,
            EndNodeId = AddSegment2EndNode.TemporaryId,
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
            OriginalId = AddSegment2.OriginalId,
            StartNodeId = Segment2StartNodeAdded.RoadNodeId,
            EndNodeId = Segment2EndNodeAdded.RoadNodeId,
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
            NationalRoadNumbers = AddSegment2.NationalRoadNumbers,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment3 = new AddRoadSegmentChange
        {
            TemporaryId = new RoadSegmentId(AddSegment2.TemporaryId + 1),
            OriginalId = new RoadSegmentId(AddSegment2.TemporaryId + 1),
            StartNodeId = AddSegment3StartNode.TemporaryId,
            EndNodeId = AddSegment3EndNode.TemporaryId,
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
            OriginalId = AddSegment3.OriginalId,
            StartNodeId = Segment3StartNodeAdded.RoadNodeId,
            EndNodeId = Segment3EndNodeAdded.RoadNodeId,
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
            NationalRoadNumbers = AddSegment3.NationalRoadNumbers,
            Provenance = new ProvenanceData(Provenance)
        };
    }
}
