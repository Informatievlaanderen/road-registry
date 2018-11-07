namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Messages;
    using Aiv.Vbr.Shaperon;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class RoadNetworkInfoProjectionTests
    {
        private readonly ScenarioFixture _fixture;

        public RoadNetworkInfoProjectionTests()
        {
            _fixture = new ScenarioFixture();
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
                    ReferencePointCount = 0,
                    TotalReferencePointShapeLength = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentDynamicHardeningAttributeCount = 0,
                    RoadSegmentDynamicLaneAttributeCount = 0,
                    RoadSegmentDynamicWidthAttributeCount = 0,
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
                    ReferencePointCount = 0,
                    TotalReferencePointShapeLength = 0,
                    RoadNodeCount = 0,
                    TotalRoadNodeShapeLength = 0,
                    RoadSegmentCount = 0,
                    RoadSegmentDynamicHardeningAttributeCount = 0,
                    RoadSegmentDynamicLaneAttributeCount = 0,
                    RoadSegmentDynamicWidthAttributeCount = 0,
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
                        Code = _fixture.Create<string>(),
                        Name = _fixture.Create<string>()
                    }
                )
                .Expect(
                    new RoadNetworkInfo {
                        Id = 0,
                        CompletedImport = false,
                        OrganizationCount = 1,
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
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
                    Code = _fixture.Create<string>(),
                    Name = _fixture.Create<string>()
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
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
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
                        LowerRoadSegmentId = _fixture.Create<int>(),
                        UpperRoadSegmentId = _fixture.Create<int>(),
                        Origin = _fixture.Create<OriginProperties>()
                    }
                )
                .Expect(
                    new RoadNetworkInfo {
                        Id = 0,
                        CompletedImport = false,
                        OrganizationCount = 0,
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
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
                    LowerRoadSegmentId = _fixture.Create<int>(),
                    UpperRoadSegmentId = _fixture.Create<int>(),
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
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
                        RoadSegmentEuropeanRoadAttributeCount = 0,
                        RoadSegmentNationalRoadAttributeCount = 0,
                        RoadSegmentNumberedRoadAttributeCount = 0,
                        TotalRoadSegmentShapeLength = 0,
                        GradeSeparatedJunctionCount = imported_junctions.Length
                    }
                );
        }

        [Fact]
        public Task When_a_reference_point_was_imported()
        {
            var writer = new WellKnownBinaryWriter();
            var geometry = _fixture.Create<PointM>();
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
                .Scenario()
                .Given(
                    new BeganRoadNetworkImport(),
                    new ImportedReferencePoint
                    {
                        Id = _fixture.Create<int>(),
                        Type = _fixture.Create<ReferencePointType>(),
                        Geometry = writer.Write(geometry),
                        Caption = _fixture.Create<double>(),
                        Ident8 = _fixture.Create<string>(),
                        Version = _fixture.Create<int>(),
                        Origin = _fixture.Create<OriginProperties>()
                    }
                )
                .Expect(
                    new RoadNetworkInfo
                    {
                        Id = 0,
                        CompletedImport = false,
                        OrganizationCount = 0,
                        ReferencePointCount = 1,
                        TotalReferencePointShapeLength = ShapeRecord.HeaderLength
                            .Plus(new PointShapeContent(geometry).ContentLength).ToInt32(),
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
                        RoadSegmentEuropeanRoadAttributeCount = 0,
                        RoadSegmentNationalRoadAttributeCount = 0,
                        RoadSegmentNumberedRoadAttributeCount = 0,
                        TotalRoadSegmentShapeLength = 0,
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }

        [Fact]
        public Task When_reference_points_were_imported()
        {
            var writer = new WellKnownBinaryWriter();
            var imported_reference_points = Enumerable
                .Range(0, new Random().Next(10))
                .Select(index => new ImportedReferencePoint
                {
                    Id = _fixture.Create<int>(),
                    Type = _fixture.Create<ReferencePointType>(),
                    Geometry = writer.Write(_fixture.Create<PointM>()),
                    Caption = _fixture.Create<double>(),
                    Ident8 = _fixture.Create<string>(),
                    Version = _fixture.Create<int>(),
                    Origin = _fixture.Create<OriginProperties>()
                })
                .ToArray();
            var givens = Array.ConvertAll(imported_reference_points, imported => (object) imported);
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
                        ReferencePointCount = imported_reference_points.Length,
                        TotalReferencePointShapeLength = imported_reference_points.Aggregate(
                            new WordLength(0),
                            (current, imported) =>
                                current
                                    .Plus(
                                        new PointShapeContent(reader.ReadAs<PointM>(imported.Geometry))
                                            .ContentLength
                                            .Plus(ShapeRecord.HeaderLength))
                        ).ToInt32(),
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
                        RoadSegmentEuropeanRoadAttributeCount = 0,
                        RoadSegmentNationalRoadAttributeCount = 0,
                        RoadSegmentNumberedRoadAttributeCount = 0,
                        TotalRoadSegmentShapeLength = 0,
                        GradeSeparatedJunctionCount = 0
                    }
                );
        }

        [Fact]
        public Task When_a_road_node_was_imported()
        {
            var writer = new WellKnownBinaryWriter();
            var geometry = _fixture.Create<PointM>();
            var reader = new WellKnownBinaryReader();
            return new RoadNetworkInfoProjection(reader)
                .Scenario()
                .Given(
                    new BeganRoadNetworkImport(),
                    new ImportedRoadNode
                    {
                        Id = _fixture.Create<int>(),
                        Type = _fixture.Create<RoadNodeType>(),
                        Geometry = writer.Write(geometry),
                        Origin = _fixture.Create<OriginProperties>()
                    }
                )
                .Expect(
                    new RoadNetworkInfo
                    {
                        Id = 0,
                        CompletedImport = false,
                        OrganizationCount = 0,
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = 1,
                        TotalRoadNodeShapeLength = ShapeRecord.HeaderLength
                            .Plus(new PointShapeContent(geometry).ContentLength).ToInt32(),
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
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
            var writer = new WellKnownBinaryWriter();
            var imported_nodes = Enumerable
                .Range(0, new Random().Next(10))
                .Select(index => new ImportedRoadNode
                {
                    Id = _fixture.Create<int>(),
                    Type = _fixture.Create<RoadNodeType>(),
                    Geometry = writer.Write(_fixture.Create<PointM>()),
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
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = imported_nodes.Length,
                        TotalRoadNodeShapeLength = imported_nodes.Aggregate(
                            new WordLength(0),
                            (current, imported) =>
                                current
                                    .Plus(
                                        new PointShapeContent(reader.ReadAs<PointM>(imported.Geometry))
                                            .ContentLength
                                            .Plus(ShapeRecord.HeaderLength))
                        ).ToInt32(),
                        RoadSegmentCount = 0,
                        RoadSegmentDynamicHardeningAttributeCount = 0,
                        RoadSegmentDynamicLaneAttributeCount = 0,
                        RoadSegmentDynamicWidthAttributeCount = 0,
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
            var writer = new WellKnownBinaryWriter();
            var geometry = _fixture.Create<MultiLineString>();
            var european_roads = _fixture.CreateMany<ImportedRoadSegmentEuropeanRoadProperties>().ToArray();
            var numbered_roads = _fixture.CreateMany<ImportedRoadSegmentNumberedRoadProperties>().ToArray();
            var national_roads = _fixture.CreateMany<ImportedRoadSegmentNationalRoadProperties>().ToArray();
            var lanes = _fixture.CreateMany<ImportedRoadSegmentLaneProperties>().ToArray();
            var widths = _fixture.CreateMany<ImportedRoadSegmentWidthProperties>().ToArray();
            var hardenings = _fixture.CreateMany<ImportedRoadSegmentHardeningProperties>().ToArray();
            var content = new PolyLineMShapeContent(geometry);
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
                        Geometry = writer.Write(geometry),
                        GeometryVersion = _fixture.Create<int>(),
                        Maintainer = _fixture.Create<Maintainer>(),
                        GeometryDrawMethod = _fixture.Create<RoadSegmentGeometryDrawMethod>(),
                        Morphology = _fixture.Create<RoadSegmentMorphology>(),
                        Status = _fixture.Create<RoadSegmentStatus>(),
                        Category = _fixture.Create<RoadSegmentCategory>(),
                        AccessRestriction = _fixture.Create<RoadSegmentAccessRestriction>(),
                        LeftSide = _fixture.Create<ImportedRoadSegmentSideProperties>(),
                        RightSide = _fixture.Create<ImportedRoadSegmentSideProperties>(),
                        PartOfEuropeanRoads = european_roads,
                        PartOfNationalRoads = national_roads,
                        PartOfNumberedRoads = numbered_roads,
                        Lanes = lanes,
                        Widths = widths,
                        Hardenings = hardenings,
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
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = 1,
                        RoadSegmentDynamicHardeningAttributeCount = hardenings.Length,
                        RoadSegmentDynamicLaneAttributeCount = lanes.Length,
                        RoadSegmentDynamicWidthAttributeCount = widths.Length,
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
                    Geometry = writer.Write(_fixture.Create<MultiLineString>()),
                    GeometryVersion = _fixture.Create<int>(),
                    Maintainer = _fixture.Create<Maintainer>(),
                    GeometryDrawMethod = _fixture.Create<RoadSegmentGeometryDrawMethod>(),
                    Morphology = _fixture.Create<RoadSegmentMorphology>(),
                    Status = _fixture.Create<RoadSegmentStatus>(),
                    Category = _fixture.Create<RoadSegmentCategory>(),
                    AccessRestriction = _fixture.Create<RoadSegmentAccessRestriction>(),
                    LeftSide = _fixture.Create<ImportedRoadSegmentSideProperties>(),
                    RightSide = _fixture.Create<ImportedRoadSegmentSideProperties>(),
                    PartOfEuropeanRoads = _fixture.CreateMany<ImportedRoadSegmentEuropeanRoadProperties>().ToArray(),
                    PartOfNationalRoads = _fixture.CreateMany<ImportedRoadSegmentNationalRoadProperties>().ToArray(),
                    PartOfNumberedRoads = _fixture.CreateMany<ImportedRoadSegmentNumberedRoadProperties>().ToArray(),
                    Lanes = _fixture.CreateMany<ImportedRoadSegmentLaneProperties>().ToArray(),
                    Widths = _fixture.CreateMany<ImportedRoadSegmentWidthProperties>().ToArray(),
                    Hardenings = _fixture.CreateMany<ImportedRoadSegmentHardeningProperties>().ToArray(),
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
                        ReferencePointCount = 0,
                        TotalReferencePointShapeLength = 0,
                        RoadNodeCount = 0,
                        TotalRoadNodeShapeLength = 0,
                        RoadSegmentCount = imported_segments.Length,
                        RoadSegmentDynamicHardeningAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.Hardenings.Length),
                        RoadSegmentDynamicLaneAttributeCount = imported_segments.Aggregate(0,
                            (current, imported) => current + imported.Lanes.Length),
                        RoadSegmentDynamicWidthAttributeCount = imported_segments.Aggregate(0,
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
                                            reader.TryReadAs(imported.Geometry, out LineString line)
                                                ? new MultiLineString(new ILineString[] {line})
                                                : reader.ReadAs<MultiLineString>(imported.Geometry)
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
