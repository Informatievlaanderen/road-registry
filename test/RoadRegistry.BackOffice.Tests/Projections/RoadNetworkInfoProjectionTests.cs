namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Framework.Testing.Projections;
    using Messages;
    using Model;
    using NetTopologySuite.Geometries;
    using Schema;
    using Xunit;
    using GeometryTranslator = Model.GeometryTranslator;

    public class RoadNetworkInfoProjectionTests
    {
        private readonly Fixture _fixture;

        public RoadNetworkInfoProjectionTests()
        {
            _fixture = new Fixture();

            _fixture.CustomizeRoadNodeType();
            _fixture.CustomizePoint();
            _fixture.CustomizeImportedRoadNode();

            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeMaintenanceAuthorityId();
            _fixture.CustomizeMaintenanceAuthorityName();
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

            _fixture.CustomizeOriginProperties();
        }

        [Fact]
        public Task When_nothing_happened()
        {
            return new RoadNetworkInfoProjection(
                new WellKnownBinaryReader()
            )
            .Scenario()
            .ExpectNone();
        }

        [Fact]
        public Task When_the_import_began()
        {
            return new RoadNetworkInfoProjection(
                new WellKnownBinaryReader()
            )
            .Scenario()
            .Given(new BeganRoadNetworkImport())
            .Expect(
                new RoadNetworkInfo {
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
                    GradeSeparatedJunctionCount = 0
                }
            );
        }

        [Fact]
        public Task When_the_import_completed()
        {
            return new RoadNetworkInfoProjection(
                new WellKnownBinaryReader()
            )
            .Scenario()
            .Given(
                    new BeganRoadNetworkImport(),
                    new CompletedRoadNetworkImport()
            )
            .Expect(
                new RoadNetworkInfo {
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
                    GradeSeparatedJunctionCount = 0
                }
            );
        }

        [Fact]
        public Task When_an_organization_was_imported()
        {
            return new RoadNetworkInfoProjection(
                    new WellKnownBinaryReader()
                )
                .Scenario()
                .Given(
                    new BeganRoadNetworkImport(),
                    new ImportedOrganization
                    {
                        Code = _fixture.Create<MaintenanceAuthorityId>(),
                        Name = _fixture.Create<MaintenanceAuthorityName>()
                    }
                )
                .Expect(
                    new RoadNetworkInfo {
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
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }

        [Fact]
        public Task When_organizations_were_imported()
        {
            var imported_organizations = Enumerable
                .Range(0, new Random().Next(10))
                .Select(index => new ImportedOrganization
                {
                    Code = _fixture.Create<MaintenanceAuthorityId>(),
                    Name = _fixture.Create<MaintenanceAuthorityName>()
                })
                .Cast<object>()
                .ToArray();
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
                .Scenario()
                .Given(
                    new BeganRoadNetworkImport()
                )
                .Given(imported_organizations)
                .Expect(
                    new RoadNetworkInfo {
                        Id = 0,
                        CompletedImport = false,
                        OrganizationCount = imported_organizations.Length,
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
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }

        [Fact]
        public Task When_a_grade_separated_junction_was_imported()
        {
            return new RoadNetworkInfoProjection(
                    new WellKnownBinaryReader()
                )
                .Scenario()
                .Given(
                    new BeganRoadNetworkImport(),
                    new ImportedGradeSeparatedJunction
                    {
                        Id = _fixture.Create<int>(),
                        Type = _fixture.Create<GradeSeparatedJunctionType>(),
                        LowerRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                        UpperRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                        Origin = _fixture.Create<OriginProperties>()
                    }
                )
                .Expect(
                    new RoadNetworkInfo {
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
                        GradeSeparatedJunctionCount = 1
                    }
                );
        }

        [Fact]
        public Task When_grade_separated_junctions_were_imported()
        {
            var imported_junctions = Enumerable
                .Range(0, new Random().Next(10))
                .Select(index => new ImportedGradeSeparatedJunction
                {
                    Id = _fixture.Create<int>(),
                    Type = _fixture.Create<GradeSeparatedJunctionType>(),
                    LowerRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                    UpperRoadSegmentId = _fixture.Create<RoadSegmentId>(),
                    Origin = _fixture.Create<OriginProperties>()
                })
                .Cast<object>()
                .ToArray();
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
                .Scenario()
                .Given(
                    new BeganRoadNetworkImport()
                )
                .Given(imported_junctions)
                .Expect(
                    new RoadNetworkInfo {
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
                        GradeSeparatedJunctionCount = imported_junctions.Length
                    }
                );
        }

        [Fact]
        public Task When_a_road_node_was_imported()
        {
            var geometry = _fixture.Create<NetTopologySuite.Geometries.Point>();
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
                .Scenario()
                .Given(
                    new BeganRoadNetworkImport(),
                    new ImportedRoadNode
                    {
                        Id = _fixture.Create<int>(),
                        Type = _fixture.Create<RoadNodeType>(),
                        Geometry = GeometryTranslator.Translate(geometry),
                        Origin = _fixture.Create<OriginProperties>()
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
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }

        [Fact]
        public Task When_road_nodes_were_imported()
        {
            var imported_nodes = Enumerable
                .Range(0, new Random().Next(10))
                .Select(index => new ImportedRoadNode
                {
                    Id = _fixture.Create<int>(),
                    Type = _fixture.Create<RoadNodeType>(),
                    Geometry = GeometryTranslator.Translate(_fixture.Create<NetTopologySuite.Geometries.Point>()),
                    Origin = _fixture.Create<OriginProperties>()
                })
                .ToArray();
            var givens = Array.ConvertAll(imported_nodes, imported => (object) imported);
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
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
                        RoadNodeCount = imported_nodes.Length,
                        TotalRoadNodeShapeLength = imported_nodes.Aggregate(
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
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }

        [Fact]
        public Task When_a_road_segment_was_imported()
        {
            var geometry = _fixture.Create<MultiLineString>();
            var european_roads = _fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttributes>().ToArray();
            var numbered_roads = _fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttributes>().ToArray();
            var national_roads = _fixture.CreateMany<ImportedRoadSegmentNationalRoadAttributes>().ToArray();
            var lanes = _fixture.CreateMany<ImportedRoadSegmentLaneAttributes>().ToArray();
            var widths = _fixture.CreateMany<ImportedRoadSegmentWidthAttributes>().ToArray();
            var hardenings = _fixture.CreateMany<ImportedRoadSegmentSurfaceAttributes>().ToArray();
            var content = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
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
                        LeftSide = _fixture.Create<ImportedRoadSegmentSideAttributes>(),
                        RightSide = _fixture.Create<ImportedRoadSegmentSideAttributes>(),
                        PartOfEuropeanRoads = european_roads,
                        PartOfNationalRoads = national_roads,
                        PartOfNumberedRoads = numbered_roads,
                        Lanes = lanes,
                        Widths = widths,
                        Surfaces = hardenings,
                        Version = _fixture.Create<int>(),
                        RecordingDate = _fixture.Create<DateTime>(),
                        Origin = _fixture.Create<OriginProperties>()
                    }
                )
                .Expect(
                    new RoadNetworkInfo {
                        Id = 0,
                        CompletedImport = false,
                        OrganizationCount = 0,
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 1,
                        RoadSegmentSurfaceAttributeCount = hardenings.Length,
                        RoadSegmentLaneAttributeCount = lanes.Length,
                        RoadSegmentWidthAttributeCount = widths.Length,
                        RoadSegmentEuropeanRoadAttributeCount = european_roads.Length,
                        RoadSegmentNationalRoadAttributeCount = national_roads.Length,
                        RoadSegmentNumberedRoadAttributeCount = numbered_roads.Length,
                        TotalRoadSegmentShapeLength = ShapeRecord.HeaderLength.Plus(content.ContentLength).ToInt32(),
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }

        [Fact]
        public Task When_road_segments_were_imported()
        {
            var writer = new WellKnownBinaryWriter();
            var imported_segments = Enumerable
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
                    LeftSide = _fixture.Create<ImportedRoadSegmentSideAttributes>(),
                    RightSide = _fixture.Create<ImportedRoadSegmentSideAttributes>(),
                    PartOfEuropeanRoads = _fixture.CreateMany<ImportedRoadSegmentEuropeanRoadAttributes>().ToArray(),
                    PartOfNationalRoads = _fixture.CreateMany<ImportedRoadSegmentNationalRoadAttributes>().ToArray(),
                    PartOfNumberedRoads = _fixture.CreateMany<ImportedRoadSegmentNumberedRoadAttributes>().ToArray(),
                    Lanes = _fixture.CreateMany<ImportedRoadSegmentLaneAttributes>().ToArray(),
                    Widths = _fixture.CreateMany<ImportedRoadSegmentWidthAttributes>().ToArray(),
                    Surfaces = _fixture.CreateMany<ImportedRoadSegmentSurfaceAttributes>().ToArray(),
                    Version = _fixture.Create<int>(),
                    RecordingDate = _fixture.Create<DateTime>(),
                    Origin = _fixture.Create<OriginProperties>()
                })
                .ToArray();
            var givens = Array.ConvertAll(imported_segments, imported => (object) imported);
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
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
                        RoadSegmentCount = imported_segments.Length,
                        RoadSegmentSurfaceAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.Surfaces.Length),
                        RoadSegmentLaneAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.Lanes.Length),
                        RoadSegmentWidthAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.Widths.Length),
                        RoadSegmentEuropeanRoadAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.PartOfEuropeanRoads.Length),
                        RoadSegmentNationalRoadAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.PartOfNationalRoads.Length),
                        RoadSegmentNumberedRoadAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.PartOfNumberedRoads.Length),
                        TotalRoadSegmentShapeLength = imported_segments.Aggregate(new WordLength(0),
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
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }
    }
}
