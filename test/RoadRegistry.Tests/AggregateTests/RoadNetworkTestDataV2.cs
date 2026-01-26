namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Extensions;
using Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNetworkTestDataV2
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
    public RoadNodeWasAdded Segment1StartNodeAdded { get; }
    public AddRoadNodeChange AddSegment2StartNode { get; }
    public RoadNodeWasAdded Segment2StartNodeAdded { get; }
    public AddRoadNodeChange AddSegment3StartNode { get; }
    public RoadNodeWasAdded Segment3StartNodeAdded { get; }

    public AddRoadNodeChange AddSegment1EndNode { get; }
    public RoadNodeWasAdded Segment1EndNodeAdded { get; }
    public AddRoadNodeChange AddSegment2EndNode { get; }
    public RoadNodeWasAdded Segment2EndNodeAdded { get; }
    public AddRoadNodeChange AddSegment3EndNode { get; }
    public RoadNodeWasAdded Segment3EndNodeAdded { get; }

    public AddRoadSegmentChange AddSegment1 { get; }
    public RoadSegmentWasAdded Segment1Added { get; }
    public AddRoadSegmentChange AddSegment2 { get; }
    public RoadSegmentWasAdded Segment2Added { get; }
    public AddRoadSegmentChange AddSegment3 { get; }
    public RoadSegmentWasAdded Segment3Added { get; }

    public RoadNetworkTestDataV2(Action<Fixture> customize = null)
    {
        Fixture = FixtureFactory.Create();
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
        Fixture.CustomizeRoadNodeTypeV2();
        Fixture.CustomizeRoadNodeGeometry();
        Fixture.CustomizeRoadSegmentId();
        Fixture.CustomizeRoadSegmentCategoryV2();
        Fixture.CustomizeRoadSegmentMorphologyV2();
        Fixture.CustomizeRoadSegmentStatusV2();
        Fixture.CustomizeRoadSegmentAccessRestrictionV2();
        Fixture.CustomizeRoadSegmentGeometry();
        Fixture.CustomizeRoadSegmentGeometryDrawMethodV2();
        Fixture.CustomizeRoadSegmentSurfaceTypeV2();
        Fixture.CustomizeEuropeanRoadNumber();
        Fixture.CustomizeNationalRoadNumber();
        Fixture.CustomizeOriginProperties();
        Fixture.CustomizeGradeSeparatedJunctionId();
        Fixture.CustomizeGradeSeparatedJunctionTypeV2();
        Fixture.CustomizeArchiveId();
        Fixture.CustomizeChangeRequestId();
        Fixture.CustomizeReason();
        Fixture.CustomizeOperatorName();
        Fixture.CustomizeTransactionId();
        Fixture.CustomizeExtractRequestId();

        Fixture.CustomizeRoadNodeWasAdded();
        Fixture.CustomizeRoadNodeWasModified();
        Fixture.CustomizeRoadSegmentAttributes();
        Fixture.CustomizeRoadSegmentWasAdded();
        Fixture.CustomizeRoadSegmentWasModified();
        Fixture.CustomizeRoadSegmentWasMerged();
        Fixture.CustomizeRoadSegmentWasMigrated();

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
            Geometry = StartPoint1.ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };

        Segment1StartNodeAdded = new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(1),
            OriginalId = AddSegment1StartNode.TemporaryId,
            Geometry = AddSegment1StartNode.Geometry,
            Type = AddSegment1StartNode.Type,
            Grensknoop = AddSegment1StartNode.Grensknoop,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment1EndNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment1StartNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = EndPoint1.ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };

        Segment1EndNodeAdded = new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(2),
            OriginalId = AddSegment1EndNode.TemporaryId,
            Geometry = AddSegment1EndNode.Geometry,
            Type = AddSegment1EndNode.Type,
            Grensknoop = AddSegment1EndNode.Grensknoop,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment2StartNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment1EndNode.TemporaryId + 1),
            OriginalId = new RoadNodeId(AddSegment1EndNode.TemporaryId + 1),
            Geometry = StartPoint2.ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };

        Segment2StartNodeAdded = new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(3),
            OriginalId = AddSegment2StartNode.TemporaryId,
            Geometry = AddSegment2StartNode.Geometry,
            Type = AddSegment2StartNode.Type,
            Grensknoop = AddSegment2StartNode.Grensknoop,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment2EndNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment2StartNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = EndPoint2.ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };

        Segment2EndNodeAdded = new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(4),
            OriginalId = AddSegment2EndNode.TemporaryId,
            Geometry = AddSegment2EndNode.Geometry,
            Type = AddSegment2EndNode.Type,
            Grensknoop = AddSegment2EndNode.Grensknoop,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment3StartNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment2EndNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = StartPoint3.ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };

        Segment3StartNodeAdded = new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(5),
            OriginalId = AddSegment3StartNode.TemporaryId,
            Geometry = AddSegment3StartNode.Geometry,
            Type = AddSegment3StartNode.Type,
            Grensknoop = AddSegment3StartNode.Grensknoop,
            Provenance = new ProvenanceData(Provenance)
        };

        AddSegment3EndNode = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(AddSegment3StartNode.TemporaryId + 1),
            OriginalId = Fixture.Create<RoadNodeId>(),
            Geometry = EndPoint3.ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };

        Segment3EndNodeAdded = new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(6),
            OriginalId = AddSegment3EndNode.TemporaryId,
            Geometry = AddSegment3EndNode.Geometry,
            Type = AddSegment3EndNode.Type,
            Grensknoop = AddSegment3EndNode.Grensknoop,
            Provenance = new ProvenanceData(Provenance)
        };

        var segment1TemporaryId = Fixture.Create<RoadSegmentId>();
        AddSegment1 = new AddRoadSegmentChange
        {
            TemporaryId = segment1TemporaryId,
            OriginalId = segment1TemporaryId,
            StartNodeId = AddSegment1StartNode.TemporaryId,
            EndNodeId = AddSegment1EndNode.TemporaryId,
            Geometry = MultiLineString1.ToRoadSegmentGeometry(),
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
            MaintenanceAuthorityId = CreateDynamicAttribute<OrganizationId>(Fixture, MultiLineString1),
            Morphology = CreateDynamicAttribute<RoadSegmentMorphologyV2>(Fixture, MultiLineString1),
            Status = CreateDynamicAttribute<RoadSegmentStatusV2>(Fixture, MultiLineString1),
            Category = CreateDynamicAttribute<RoadSegmentCategoryV2>(Fixture, MultiLineString1),
            AccessRestriction = CreateDynamicAttribute<RoadSegmentAccessRestrictionV2>(Fixture, MultiLineString1),
            StreetNameId = CreateDynamicAttribute<StreetNameLocalId>(Fixture, MultiLineString1),
            SurfaceType = CreateDynamicAttribute<RoadSegmentSurfaceTypeV2>(Fixture, MultiLineString1),
            CarAccess = CreateDynamicAttribute<VehicleAccess>(Fixture, MultiLineString1),
            BikeAccess = CreateDynamicAttribute<VehicleAccess>(Fixture, MultiLineString1),
            PedestrianAccess = CreateDynamicAttribute<bool>(Fixture, MultiLineString1),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        Segment1Added = new RoadSegmentWasAdded
        {
            RoadSegmentId = new RoadSegmentId(1),
            OriginalId = AddSegment1.OriginalId,
            StartNodeId = Segment1StartNodeAdded.RoadNodeId,
            EndNodeId = Segment1EndNodeAdded.RoadNodeId,
            Geometry = AddSegment1.Geometry,
            MaintenanceAuthorityId = AddSegment1.MaintenanceAuthorityId,
            GeometryDrawMethod = AddSegment1.GeometryDrawMethod,
            Morphology = AddSegment1.Morphology,
            Status = AddSegment1.Status,
            Category = AddSegment1.Category,
            AccessRestriction = AddSegment1.AccessRestriction,
            StreetNameId = AddSegment1.StreetNameId,
            SurfaceType = AddSegment1.SurfaceType,
            CarAccess = AddSegment1.CarAccess,
            BikeAccess = AddSegment1.BikeAccess,
            PedestrianAccess = AddSegment1.PedestrianAccess,
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
            Geometry = MultiLineString2.ToRoadSegmentGeometry(),
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
            MaintenanceAuthorityId = CreateDynamicAttribute<OrganizationId>(Fixture, MultiLineString2),
            Morphology = CreateDynamicAttribute<RoadSegmentMorphologyV2>(Fixture, MultiLineString2),
            Status = CreateDynamicAttribute<RoadSegmentStatusV2>(Fixture, MultiLineString2),
            Category = CreateDynamicAttribute<RoadSegmentCategoryV2>(Fixture, MultiLineString2),
            AccessRestriction = CreateDynamicAttribute<RoadSegmentAccessRestrictionV2>(Fixture, MultiLineString2),
            StreetNameId = CreateDynamicAttribute<StreetNameLocalId>(Fixture, MultiLineString2),
            SurfaceType = CreateDynamicAttribute<RoadSegmentSurfaceTypeV2>(Fixture, MultiLineString2),
            CarAccess = CreateDynamicAttribute<VehicleAccess>(Fixture, MultiLineString2),
            BikeAccess = CreateDynamicAttribute<VehicleAccess>(Fixture, MultiLineString2),
            PedestrianAccess = CreateDynamicAttribute<bool>(Fixture, MultiLineString2),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        Segment2Added = new RoadSegmentWasAdded
        {
            RoadSegmentId = new RoadSegmentId(2),
            OriginalId = AddSegment2.OriginalId,
            StartNodeId = Segment2StartNodeAdded.RoadNodeId,
            EndNodeId = Segment2EndNodeAdded.RoadNodeId,
            Geometry = AddSegment2.Geometry,
            MaintenanceAuthorityId = AddSegment2.MaintenanceAuthorityId,
            GeometryDrawMethod = AddSegment2.GeometryDrawMethod,
            Morphology = AddSegment2.Morphology,
            Status = AddSegment2.Status,
            Category = AddSegment2.Category,
            AccessRestriction = AddSegment2.AccessRestriction,
            StreetNameId = AddSegment2.StreetNameId,
            SurfaceType = AddSegment2.SurfaceType,
            CarAccess = AddSegment2.CarAccess,
            BikeAccess = AddSegment2.BikeAccess,
            PedestrianAccess = AddSegment2.PedestrianAccess,
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
            Geometry = MultiLineString3.ToRoadSegmentGeometry(),
            GeometryDrawMethod = Fixture.Create<RoadSegmentGeometryDrawMethodV2>(),
            MaintenanceAuthorityId = CreateDynamicAttribute<OrganizationId>(Fixture, MultiLineString2),
            Morphology = CreateDynamicAttribute<RoadSegmentMorphologyV2>(Fixture, MultiLineString2),
            Status = CreateDynamicAttribute<RoadSegmentStatusV2>(Fixture, MultiLineString2),
            Category = CreateDynamicAttribute<RoadSegmentCategoryV2>(Fixture, MultiLineString2),
            AccessRestriction = CreateDynamicAttribute<RoadSegmentAccessRestrictionV2>(Fixture, MultiLineString2),
            StreetNameId = CreateDynamicAttribute<StreetNameLocalId>(Fixture, MultiLineString2),
            SurfaceType = CreateDynamicAttribute<RoadSegmentSurfaceTypeV2>(Fixture, MultiLineString2),
            CarAccess = CreateDynamicAttribute<VehicleAccess>(Fixture, MultiLineString2),
            BikeAccess = CreateDynamicAttribute<VehicleAccess>(Fixture, MultiLineString2),
            PedestrianAccess = CreateDynamicAttribute<bool>(Fixture, MultiLineString2),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        Segment3Added = new RoadSegmentWasAdded
        {
            RoadSegmentId = new RoadSegmentId(3),
            OriginalId = AddSegment3.OriginalId,
            StartNodeId = Segment3StartNodeAdded.RoadNodeId,
            EndNodeId = Segment3EndNodeAdded.RoadNodeId,
            Geometry = AddSegment3.Geometry,
            MaintenanceAuthorityId = AddSegment3.MaintenanceAuthorityId,
            GeometryDrawMethod = AddSegment3.GeometryDrawMethod,
            Morphology = AddSegment3.Morphology,
            Status = AddSegment3.Status,
            Category = AddSegment3.Category,
            AccessRestriction = AddSegment3.AccessRestriction,
            StreetNameId = AddSegment3.StreetNameId,
            SurfaceType = AddSegment3.SurfaceType,
            CarAccess = AddSegment3.CarAccess,
            BikeAccess = AddSegment3.BikeAccess,
            PedestrianAccess = AddSegment3.PedestrianAccess,
            EuropeanRoadNumbers = AddSegment3.EuropeanRoadNumbers,
            NationalRoadNumbers = AddSegment3.NationalRoadNumbers,
            Provenance = new ProvenanceData(Provenance)
        };
    }

    private RoadSegmentDynamicAttributeValues<T> CreateDynamicAttribute<T>(IFixture fixture, MultiLineString geometry)
    {
        return new RoadSegmentDynamicAttributeValues<T>()
            .Add(fixture.Create<T>(), RoadSegmentGeometry.Create(geometry));
    }
}
