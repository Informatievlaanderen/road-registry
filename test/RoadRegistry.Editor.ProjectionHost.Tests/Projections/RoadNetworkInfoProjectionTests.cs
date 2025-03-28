namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Projections;
using NetTopologySuite.Geometries;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNetworkInfoProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;

    public RoadNetworkInfoProjectionTests(ProjectionTestServices services)
    {
        _fixture = new Fixture();

        _fixture.CustomizeRoadNodeType();
        _fixture.CustomizePoint();
        _fixture.CustomizeImportedRoadNode();

        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeReason();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizePolylineM();
        _fixture.CustomizeEuropeanRoadNumber();
        _fixture.CustomizeNationalRoadNumber();
        _fixture.CustomizeNumberedRoadNumber();
        _fixture.CustomizeRoadSegmentNumberedRoadDirection();
        _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        _fixture.CustomizeRoadSegmentLaneCount();
        _fixture.CustomizeRoadSegmentLaneDirection();
        _fixture.CustomizeRoadSegmentWidth();
        _fixture.CustomizeRoadSegmentSurfaceType();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();
        _fixture.CustomizeRoadSegmentGeometryVersion();

        _fixture.CustomizeImportedRoadSegment();
        _fixture.CustomizeImportedRoadSegmentEuropeanRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNationalRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNumberedRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentLaneAttributes();
        _fixture.CustomizeImportedRoadSegmentWidthAttributes();
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentSideAttributes();

        _fixture.CustomizeGradeSeparatedJunctionId();
        _fixture.CustomizeGradeSeparatedJunctionType();
        _fixture.CustomizeImportedGradeSeparatedJunction();

        _fixture.CustomizeRoadNodeAdded();
        _fixture.CustomizeRoadNodeModified();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();

        _fixture.CustomizeOriginProperties();
    }

    [Fact]
    public Task When_a_grade_separated_junction_was_imported()
    {
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                new ImportedGradeSeparatedJunction
                {
                    Id = _fixture.Create<int>(),
                    Type = _fixture.Create<GradeSeparatedJunctionType>(),
                    LowerRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                    UpperRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                    Origin = _fixture.Create<ImportedOriginProperties>()
                }
            )
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = 1,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_a_road_node_was_imported()
    {
        var geometry = _fixture.Create<Point>();
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                new ImportedRoadNode
                {
                    Id = _fixture.Create<int>(),
                    Type = _fixture.Create<RoadNodeType>(),
                    Geometry = GeometryTranslator.Translate(geometry),
                    Origin = _fixture.Create<ImportedOriginProperties>()
                }
            )
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 0,
                    RoadNodeCount = 1,
                    TotalRoadNodeShapeLength = ShapeRecord.HeaderLength
                        .Plus(new PointShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(geometry)).ContentLength).ToInt32(),
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_a_road_segment_was_imported()
    {
        var geometry = _fixture.Create<MultiLineString>();
        var europeanRoads = _fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>().ToArray();
        var numberedRoads = _fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttribute>().ToArray();
        var nationalRoads = _fixture.CreateMany<ImportedRoadSegmentNationalRoadAttribute>().ToArray();
        var lanes = _fixture.CreateMany<ImportedRoadSegmentLaneAttribute>().ToArray();
        var widths = _fixture.CreateMany<ImportedRoadSegmentWidthAttribute>().ToArray();
        var hardenings = _fixture.CreateMany<ImportedRoadSegmentSurfaceAttribute>().ToArray();
        var content = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                new ImportedRoadSegment
                {
                    Id = _fixture.Create<int>(),
                    StartNodeId = _fixture.Create<int>(),
                    EndNodeId = _fixture.Create<int>(),
                    Geometry = GeometryTranslator.Translate(geometry),
                    GeometryVersion = _fixture.Create<int>(),
                    MaintenanceAuthority = _fixture.Create<MaintenanceAuthority>(),
                    GeometryDrawMethod = _fixture.Create<RoadSegmentGeometryDrawMethod>(),
                    Morphology = _fixture.Create<RoadSegmentMorphology>(),
                    Status = _fixture.Create<RoadSegmentStatus>(),
                    Category = _fixture.Create<RoadSegmentCategory>(),
                    AccessRestriction = _fixture.Create<RoadSegmentAccessRestriction>(),
                    LeftSide = _fixture.Create<ImportedRoadSegmentSideAttribute>(),
                    RightSide = _fixture.Create<ImportedRoadSegmentSideAttribute>(),
                    PartOfEuropeanRoads = europeanRoads,
                    PartOfNationalRoads = nationalRoads,
                    PartOfNumberedRoads = numberedRoads,
                    Lanes = lanes,
                    Widths = widths,
                    Surfaces = hardenings,
                    Version = _fixture.Create<int>(),
                    RecordingDate = _fixture.Create<DateTime>(),
                    Origin = _fixture.Create<ImportedOriginProperties>()
                }
            )
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 1,
                    RoadSegmentSurfaceAttributeCount = hardenings.Length,
                    RoadSegmentLaneAttributeCount = lanes.Length,
                    RoadSegmentWidthAttributeCount = widths.Length,
                    RoadSegmentEuropeanRoadAttributeCount = europeanRoads.Length,
                    RoadSegmentNationalRoadAttributeCount = nationalRoads.Length,
                    RoadSegmentNumberedRoadAttributeCount = numberedRoads.Length,
                    TotalRoadSegmentShapeLength = ShapeRecord.HeaderLength.Plus(content.ContentLength).ToInt32(),
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_an_imported_road_node_was_modified_nothing_is_altered()
    {
        _fixture.Freeze<RoadNodeId>();

        var importedRoadNode = _fixture.Create<ImportedRoadNode>();

        var roadNodeModifiedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeModified>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 1,
            TotalRoadNodeShapeLength = new PointShapeContent(
                    Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(GeometryTranslator.Translate(importedRoadNode.Geometry))
                )
                .ContentLength.Plus(ShapeRecord.HeaderLength)
                .ToInt32(),
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                importedRoadNode,
                roadNodeModifiedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_an_imported_road_node_was_removed()
    {
        _fixture.Freeze<RoadNodeId>();

        var importedRoadNode = _fixture.Create<ImportedRoadNode>();

        var roadNodeRemovedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeRemoved>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                importedRoadNode,
                roadNodeRemovedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_an_imported_road_segment_was_modified()
    {
        var unrelatedRoadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var unrelatedRoadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(unrelatedRoadSegmentAdded);

        _fixture.Freeze<RoadSegmentId>();

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        var roadSegmentModifiedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 2,
            RoadSegmentSurfaceAttributeCount =
                roadSegmentModified.Surfaces.Length + unrelatedRoadSegmentAdded.Surfaces.Length,
            RoadSegmentLaneAttributeCount =
                roadSegmentModified.Lanes.Length + unrelatedRoadSegmentAdded.Lanes.Length,
            RoadSegmentWidthAttributeCount =
                roadSegmentModified.Widths.Length + unrelatedRoadSegmentAdded.Widths.Length,
            RoadSegmentEuropeanRoadAttributeCount = importedRoadSegment.PartOfEuropeanRoads.Length,
            RoadSegmentNationalRoadAttributeCount = importedRoadSegment.PartOfNationalRoads.Length,
            RoadSegmentNumberedRoadAttributeCount = importedRoadSegment.PartOfNumberedRoads.Length,
            TotalRoadSegmentShapeLength =
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(importedRoadSegment.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32()
                +
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(unrelatedRoadSegmentAdded.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                unrelatedRoadSegmentAddedChangesAccepted,
                importedRoadSegment,
                roadSegmentModifiedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_an_imported_road_segment_was_removed()
    {
        var unrelatedRoadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var unrelatedRoadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(unrelatedRoadSegmentAdded);

        _fixture.Freeze<RoadSegmentId>();

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        var roadSegmentModifiedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var roadSegmentRemoved = _fixture.Create<RoadSegmentRemoved>();
        var roadSegmentRemovedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentRemoved);

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 1,
            RoadSegmentSurfaceAttributeCount = unrelatedRoadSegmentAdded.Surfaces.Length,
            RoadSegmentLaneAttributeCount = unrelatedRoadSegmentAdded.Lanes.Length,
            RoadSegmentWidthAttributeCount = unrelatedRoadSegmentAdded.Widths.Length,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength =
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(unrelatedRoadSegmentAdded.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                unrelatedRoadSegmentAddedChangesAccepted,
                importedRoadSegment,
                roadSegmentModifiedChangesAccepted,
                roadSegmentRemovedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_an_organization_was_imported()
    {
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                new ImportedOrganization
                {
                    Code = _fixture.Create<OrganizationId>(),
                    Name = _fixture.Create<OrganizationName>()
                }
            )
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 1,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_grade_separated_junctions_were_added()
    {
        var roadNetworkChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 1,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNetworkChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_grade_separated_junctions_were_imported()
    {
        var importedJunctions = Enumerable
            .Range(0, new Random().Next(10))
            .Select(index => new ImportedGradeSeparatedJunction
            {
                Id = _fixture.Create<int>(),
                Type = _fixture.Create<GradeSeparatedJunctionType>(),
                LowerRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                UpperRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                Origin = _fixture.Create<ImportedOriginProperties>()
            })
            .Cast<object>()
            .ToArray();
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport()
            )
            .Given(importedJunctions)
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = importedJunctions.Length,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_grade_separated_junctions_were_removed()
    {
        var gradeSeparatedJunctionAddedAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var gradeSeparatedJunctionRemovedAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionRemoved>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                gradeSeparatedJunctionAddedAccepted,
                gradeSeparatedJunctionRemovedAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_nothing_happened()
    {
        return new RoadNetworkInfoProjection()
            .Scenario()
            .ExpectNone();
    }

    [Fact]
    public Task When_organizations_were_imported()
    {
        var importedOrganizations = Enumerable
            .Range(0, new Random().Next(10))
            .Select(index => new ImportedOrganization
            {
                Code = _fixture.Create<OrganizationId>(),
                Name = _fixture.Create<OrganizationName>()
            })
            .Cast<object>()
            .ToArray();
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport()
            )
            .Given(importedOrganizations)
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = importedOrganizations.Length,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_road_node_was_added()
    {
        var roadNodeAdded = _fixture.Create<RoadNodeAdded>();
        var roadNetworkChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadNodeAdded);

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 1,
            TotalRoadNodeShapeLength = new PointShapeContent(
                    Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(GeometryTranslator.Translate(roadNodeAdded.Geometry))
                )
                .ContentLength.Plus(ShapeRecord.HeaderLength)
                .ToInt32(),
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNetworkChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_node_was_modified_nothing_is_altered()
    {
        _fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = _fixture.Create<RoadNodeAdded>();
        var roadNodeAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadNodeAdded);

        var roadNodeModifiedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeModified>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 1,
            TotalRoadNodeShapeLength = new PointShapeContent(
                    Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(GeometryTranslator.Translate(roadNodeAdded.Geometry))
                )
                .ContentLength.Plus(ShapeRecord.HeaderLength)
                .ToInt32(),
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNodeAddedChangesAccepted,
                roadNodeModifiedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_node_was_removed()
    {
        _fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = _fixture.Create<RoadNodeAdded>();
        var roadNodeAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadNodeAdded);

        var roadNodeRemovedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeRemoved>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNodeAddedChangesAccepted,
                roadNodeRemovedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_nodes_were_imported()
    {
        var importedNodes = Enumerable
            .Range(0, new Random().Next(10))
            .Select(index => new ImportedRoadNode
            {
                Id = _fixture.Create<int>(),
                Type = _fixture.Create<RoadNodeType>(),
                Geometry = GeometryTranslator.Translate(_fixture.Create<Point>()),
                Origin = _fixture.Create<ImportedOriginProperties>()
            })
            .ToArray();
        var givens = Array.ConvertAll(importedNodes, imported => (object)imported);
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport()
            )
            .Given(givens)
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 0,
                    RoadNodeCount = importedNodes.Length,
                    TotalRoadNodeShapeLength = importedNodes.Aggregate(
                        new WordLength(0),
                        (current, imported) =>
                            current
                                .Plus(
                                    new PointShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(GeometryTranslator.Translate(imported.Geometry)))
                                        .ContentLength
                                        .Plus(ShapeRecord.HeaderLength))
                    ).ToInt32(),
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_road_segment_geometry_was_modified()
    {
        var unrelatedRoadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var unrelatedRoadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(unrelatedRoadSegmentAdded);

        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var roadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentGeometryModified = _fixture.Create<RoadSegmentGeometryModified>();
        var roadSegmentGeometryModifiedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentGeometryModified);

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 2,
            RoadSegmentSurfaceAttributeCount =
                roadSegmentGeometryModified.Surfaces.Length + unrelatedRoadSegmentAdded.Surfaces.Length,
            RoadSegmentLaneAttributeCount =
                roadSegmentGeometryModified.Lanes.Length + unrelatedRoadSegmentAdded.Lanes.Length,
            RoadSegmentWidthAttributeCount =
                roadSegmentGeometryModified.Widths.Length + unrelatedRoadSegmentAdded.Widths.Length,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength =
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(roadSegmentAdded.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32()
                +
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(unrelatedRoadSegmentAdded.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                unrelatedRoadSegmentAddedChangesAccepted,
                roadSegmentAddedChangesAccepted,
                roadSegmentGeometryModifiedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segment_was_added()
    {
        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var roadNetworkChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 1,
            RoadSegmentSurfaceAttributeCount = roadSegmentAdded.Surfaces.Length,
            RoadSegmentLaneAttributeCount = roadSegmentAdded.Lanes.Length,
            RoadSegmentWidthAttributeCount = roadSegmentAdded.Widths.Length,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = new PolyLineMShapeContent(
                    Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(GeometryTranslator.Translate(roadSegmentAdded.Geometry))
                )
                .ContentLength.Plus(ShapeRecord.HeaderLength)
                .ToInt32(),
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNetworkChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segment_was_modified()
    {
        var unrelatedRoadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var unrelatedRoadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(unrelatedRoadSegmentAdded);

        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var roadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        var roadSegmentModifiedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 2,
            RoadSegmentSurfaceAttributeCount =
                roadSegmentModified.Surfaces.Length + unrelatedRoadSegmentAdded.Surfaces.Length,
            RoadSegmentLaneAttributeCount =
                roadSegmentModified.Lanes.Length + unrelatedRoadSegmentAdded.Lanes.Length,
            RoadSegmentWidthAttributeCount =
                roadSegmentModified.Widths.Length + unrelatedRoadSegmentAdded.Widths.Length,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength =
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(roadSegmentAdded.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32()
                +
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(unrelatedRoadSegmentAdded.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                unrelatedRoadSegmentAddedChangesAccepted,
                roadSegmentAddedChangesAccepted,
                roadSegmentModifiedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segment_was_removed()
    {
        var unrelatedRoadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var unrelatedRoadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(unrelatedRoadSegmentAdded);

        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var roadSegmentAddedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        var roadSegmentModifiedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var roadSegmentRemoved = _fixture.Create<RoadSegmentRemoved>();
        var roadSegmentRemovedChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentRemoved);

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 1,
            RoadSegmentSurfaceAttributeCount = unrelatedRoadSegmentAdded.Surfaces.Length,
            RoadSegmentLaneAttributeCount = unrelatedRoadSegmentAdded.Lanes.Length,
            RoadSegmentWidthAttributeCount = unrelatedRoadSegmentAdded.Widths.Length,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength =
                new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                        GeometryTranslator.Translate(unrelatedRoadSegmentAdded.Geometry)))
                    .ContentLength.Plus(ShapeRecord.HeaderLength)
                    .ToInt32(),
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                unrelatedRoadSegmentAddedChangesAccepted,
                roadSegmentAddedChangesAccepted,
                roadSegmentModifiedChangesAccepted,
                roadSegmentRemovedChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_were_added_to_european_roads()
    {
        var roadNetworkChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 1,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNetworkChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_were_added_to_national_roads()
    {
        var roadNetworkChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 1,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNetworkChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_were_added_to_numbered_roads()
    {
        var roadNetworkChangesAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 1,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadNetworkChangesAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_were_imported()
    {
        var importedSegments = Enumerable
            .Range(0, new Random().Next(10))
            .Select(index => new ImportedRoadSegment
            {
                Id = _fixture.Create<int>(),
                StartNodeId = _fixture.Create<int>(),
                EndNodeId = _fixture.Create<int>(),
                Geometry = GeometryTranslator.Translate(_fixture.Create<MultiLineString>()),
                GeometryVersion = _fixture.Create<int>(),
                MaintenanceAuthority = _fixture.Create<MaintenanceAuthority>(),
                GeometryDrawMethod = _fixture.Create<RoadSegmentGeometryDrawMethod>(),
                Morphology = _fixture.Create<RoadSegmentMorphology>(),
                Status = _fixture.Create<RoadSegmentStatus>(),
                Category = _fixture.Create<RoadSegmentCategory>(),
                AccessRestriction = _fixture.Create<RoadSegmentAccessRestriction>(),
                LeftSide = _fixture.Create<ImportedRoadSegmentSideAttribute>(),
                RightSide = _fixture.Create<ImportedRoadSegmentSideAttribute>(),
                PartOfEuropeanRoads = _fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>().ToArray(),
                PartOfNationalRoads = _fixture.CreateMany<ImportedRoadSegmentNationalRoadAttribute>().ToArray(),
                PartOfNumberedRoads = _fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttribute>().ToArray(),
                Lanes = _fixture.CreateMany<ImportedRoadSegmentLaneAttribute>().ToArray(),
                Widths = _fixture.CreateMany<ImportedRoadSegmentWidthAttribute>().ToArray(),
                Surfaces = _fixture.CreateMany<ImportedRoadSegmentSurfaceAttribute>().ToArray(),
                Version = _fixture.Create<int>(),
                RecordingDate = _fixture.Create<DateTime>(),
                Origin = _fixture.Create<ImportedOriginProperties>()
            })
            .ToArray();
        var givens = Array.ConvertAll(importedSegments, imported => (object)imported);
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport()
            )
            .Given(givens)
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = importedSegments.Length,
                    RoadSegmentSurfaceAttributeCount = importedSegments.Aggregate(0,
                        (current, imported) => current + imported.Surfaces.Length),
                    RoadSegmentLaneAttributeCount = importedSegments.Aggregate(0,
                        (current, imported) => current + imported.Lanes.Length),
                    RoadSegmentWidthAttributeCount = importedSegments.Aggregate(0,
                        (current, imported) => current + imported.Widths.Length),
                    RoadSegmentEuropeanRoadAttributeCount = importedSegments.Aggregate(0,
                        (current, imported) => current + imported.PartOfEuropeanRoads.Length),
                    RoadSegmentNationalRoadAttributeCount = importedSegments.Aggregate(0,
                        (current, imported) => current + imported.PartOfNationalRoads.Length),
                    RoadSegmentNumberedRoadAttributeCount = importedSegments.Aggregate(0,
                        (current, imported) => current + imported.PartOfNumberedRoads.Length),
                    TotalRoadSegmentShapeLength = importedSegments.Aggregate(new WordLength(0),
                            (current, imported) => current
                                .Plus(
                                    new PolyLineMShapeContent(
                                            Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(
                                                GeometryTranslator.Translate(imported.Geometry)
                                            )
                                        )
                                        .ContentLength
                                        .Plus(ShapeRecord.HeaderLength)
                                )
                        )
                        .ToInt32(),
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_road_segments_were_removed_from_european_roads()
    {
        var roadSegmentAddedToEuRoadAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>());

        var roadSegmentRemovedFromEuRoadAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromEuropeanRoad>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadSegmentAddedToEuRoadAccepted,
                roadSegmentRemovedFromEuRoadAccepted
            )
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_were_removed_from_national_roads()
    {
        var roadSegmentAddedToNationalRoadAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

        var roadSegmentRemovedFromNationalRoadAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNationalRoad>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadSegmentAddedToNationalRoadAccepted,
                roadSegmentRemovedFromNationalRoadAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_were_removed_from_numbered_roads()
    {
        var roadSegmentAddedToNumberedRoadAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());

        var roadSegmentRemovedFromNumberedRoadAccepted = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNumberedRoad>());

        var expectedRecords = new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false,
            OrganizationCount = 0,
            RoadNodeCount = 0,
            TotalRoadNodeShapeLength = 0,
            RoadSegmentCount = 0,
            RoadSegmentSurfaceAttributeCount = 0,
            RoadSegmentLaneAttributeCount = 0,
            RoadSegmentWidthAttributeCount = 0,
            RoadSegmentEuropeanRoadAttributeCount = 0,
            RoadSegmentNationalRoadAttributeCount = 0,
            RoadSegmentNumberedRoadAttributeCount = 0,
            TotalRoadSegmentShapeLength = 0,
            GradeSeparatedJunctionCount = 0,
            LastChangedTimestamp = Moment.EnvelopeCreatedUtc
        };

        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                roadSegmentAddedToNumberedRoadAccepted,
                roadSegmentRemovedFromNumberedRoadAccepted)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_the_import_began()
    {
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(new BeganRoadNetworkImport())
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = false,
                    OrganizationCount = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }

    [Fact]
    public Task When_the_import_completed()
    {
        return new RoadNetworkInfoProjection()
            .Scenario()
            .Given(
                new BeganRoadNetworkImport(),
                new CompletedRoadNetworkImport()
            )
            .Expect(
                new RoadNetworkInfo
                {
                    Id = 0,
                    CompletedImport = true,
                    OrganizationCount = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentSurfaceAttributeCount = 0,
                    RoadSegmentLaneAttributeCount = 0,
                    RoadSegmentWidthAttributeCount = 0,
                    RoadSegmentEuropeanRoadAttributeCount = 0,
                    RoadSegmentNationalRoadAttributeCount = 0,
                    RoadSegmentNumberedRoadAttributeCount = 0,
                    TotalRoadSegmentShapeLength = 0,
                    GradeSeparatedJunctionCount = 0,
                    LastChangedTimestamp = Moment.EnvelopeCreatedUtc
                }
            );
    }
}
