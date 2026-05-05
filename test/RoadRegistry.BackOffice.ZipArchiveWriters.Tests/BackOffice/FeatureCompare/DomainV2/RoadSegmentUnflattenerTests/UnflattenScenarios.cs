namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.RoadSegmentUnflattenerTests;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

public class UnflattenScenarios
{
    private ITestOutputHelper TestOutputHelper { get; }

    public UnflattenScenarios(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    [Fact]
    public void With2FlatSegmentsAnd1Schijnknoop_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0)
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0)
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(50, 0),
                Grensknoop = false
            }
        };

        // Act
        var unflattenResult = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();
        dynamicRecord.Attributes.AccessRestriction!.Values.Should().HaveCount(2);

        dynamicRecord.Attributes.AccessRestriction!.Values[0].Coverage.Should().BeEquivalentTo(new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(50)));
        dynamicRecord.Attributes.AccessRestriction!.Values[0].Value.Should().Be(flatSegment1.AccessRestriction);
        dynamicRecord.Attributes.AccessRestriction!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.AccessRestriction!.Values[1].Coverage.Should().BeEquivalentTo(new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(50), new RoadSegmentPositionV2(100)));
        dynamicRecord.Attributes.AccessRestriction!.Values[1].Value.Should().Be(flatSegment2.AccessRestriction);
        dynamicRecord.Attributes.AccessRestriction!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);
    }

    [Fact]
    public void With2FlatSegmentsInOppositeDirectionAndFlippedAttributeSidesAnd1Schijnknoop_ThenSuccessWithAttributesOnCorrectSides()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            LeftMaintenanceAuthorityId = new OrganizationId("1"),
            RightMaintenanceAuthorityId = new OrganizationId("2"),
            LeftSideStreetNameId = new StreetNameLocalId(1),
            RightSideStreetNameId = new StreetNameLocalId(2),
            CarAccessBackward = false,
            CarAccessForward = true,
            BikeAccessBackward = false,
            BikeAccessForward = true
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(100, 0, 50, 0),
            // attributes below are flipped of segment1
            LeftMaintenanceAuthorityId = new OrganizationId("2"),
            RightMaintenanceAuthorityId = new OrganizationId("1"),
            LeftSideStreetNameId = new StreetNameLocalId(2),
            RightSideStreetNameId = new StreetNameLocalId(1),
            CarAccessBackward = true,
            CarAccessForward = false,
            BikeAccessBackward = true,
            BikeAccessForward = false
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(50, 0)
            }
        };

        // Act
        var unflattenResult = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();

        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[0].Value.Should().Be(flatSegment1.LeftMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[1].Value.Should().Be(flatSegment1.RightMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[2].Value.Should().Be(flatSegment1.LeftMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[3].Value.Should().Be(flatSegment1.RightMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[3].Side.Should().Be(RoadSegmentAttributeSide.Right);

        dynamicRecord.Attributes.StreetNameId!.Values[0].Value.Should().Be(flatSegment1.LeftSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Value.Should().Be(flatSegment1.RightSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.StreetNameId!.Values[2].Value.Should().Be(flatSegment1.LeftSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.StreetNameId!.Values[3].Value.Should().Be(flatSegment1.RightSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[3].Side.Should().Be(RoadSegmentAttributeSide.Right);

        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Value.Should().Be(flatSegment1.CarAccessBackward);
        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessBackward!.Values[1].Value.Should().Be(flatSegment1.CarAccessBackward);
        dynamicRecord.Attributes.CarAccessBackward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.CarAccessForward!.Values[0].Value.Should().Be(flatSegment1.CarAccessForward);
        dynamicRecord.Attributes.CarAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessForward!.Values[1].Value.Should().Be(flatSegment1.CarAccessForward);
        dynamicRecord.Attributes.CarAccessForward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessBackward);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[1].Value.Should().Be(flatSegment1.BikeAccessBackward);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessForward);
        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessForward!.Values[1].Value.Should().Be(flatSegment1.BikeAccessForward);
        dynamicRecord.Attributes.BikeAccessForward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);
    }

    [Fact]
    public void With3FlatSegmentsWhereMiddleIsInOppositeDirection_ThenSuccessWithAttributesOnCorrectSides()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze<RoadSegmentGeometryDrawMethodV2>();
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            TempId = new RoadSegmentTempId(1),
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            LeftMaintenanceAuthorityId = new OrganizationId("1"),
            RightMaintenanceAuthorityId = new OrganizationId("2"),
            LeftSideStreetNameId = new StreetNameLocalId(1),
            RightSideStreetNameId = new StreetNameLocalId(2),
            CarAccessBackward = false,
            CarAccessForward = true,
            BikeAccessBackward = false,
            BikeAccessForward = true
        };
        var flatSegment2 = flatSegment1 with
        {
            TempId = new RoadSegmentTempId(2),
            Geometry = BuildRoadSegmentGeometry(100, 0, 50, 0),
            // attributes below are flipped of segment1
            LeftMaintenanceAuthorityId = new OrganizationId("2"),
            RightMaintenanceAuthorityId = new OrganizationId("1"),
            LeftSideStreetNameId = new StreetNameLocalId(2),
            RightSideStreetNameId = new StreetNameLocalId(1),
            CarAccessBackward = true,
            CarAccessForward = false,
            BikeAccessBackward = true,
            BikeAccessForward = false
        };
        var flatSegment3 = flatSegment1 with
        {
            TempId = new RoadSegmentTempId(3),
            Geometry = BuildRoadSegmentGeometry(100, 0, 200, 0)
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2,
            flatSegment3
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(50, 0)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(2),
                Geometry = new Point(100, 0)
            },
        };

        // Act
        var unflattenResult = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(1);
        var dynamicRecord = records.Single();

        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[0].Value.Should().Be(flatSegment1.LeftMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[1].Value.Should().Be(flatSegment1.RightMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[2].Value.Should().Be(flatSegment1.LeftMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[3].Value.Should().Be(flatSegment1.RightMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[3].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[4].Value.Should().Be(flatSegment1.LeftMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[4].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[5].Value.Should().Be(flatSegment1.RightMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[5].Side.Should().Be(RoadSegmentAttributeSide.Right);

        dynamicRecord.Attributes.StreetNameId!.Values[0].Value.Should().Be(flatSegment1.LeftSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Value.Should().Be(flatSegment1.RightSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.StreetNameId!.Values[2].Value.Should().Be(flatSegment1.LeftSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.StreetNameId!.Values[3].Value.Should().Be(flatSegment1.RightSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[3].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.StreetNameId!.Values[4].Value.Should().Be(flatSegment1.LeftSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[4].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.StreetNameId!.Values[5].Value.Should().Be(flatSegment1.RightSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[5].Side.Should().Be(RoadSegmentAttributeSide.Right);

        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Value.Should().Be(flatSegment1.CarAccessBackward);
        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessBackward!.Values[1].Value.Should().Be(flatSegment1.CarAccessBackward);
        dynamicRecord.Attributes.CarAccessBackward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessBackward!.Values[2].Value.Should().Be(flatSegment1.CarAccessBackward);
        dynamicRecord.Attributes.CarAccessBackward!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.CarAccessForward!.Values[0].Value.Should().Be(flatSegment1.CarAccessForward);
        dynamicRecord.Attributes.CarAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessForward!.Values[1].Value.Should().Be(flatSegment1.CarAccessForward);
        dynamicRecord.Attributes.CarAccessForward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessForward!.Values[2].Value.Should().Be(flatSegment1.CarAccessForward);
        dynamicRecord.Attributes.CarAccessForward!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessBackward);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[1].Value.Should().Be(flatSegment1.BikeAccessBackward);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[2].Value.Should().Be(flatSegment1.BikeAccessBackward);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessForward);
        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessForward!.Values[1].Value.Should().Be(flatSegment1.BikeAccessForward);
        dynamicRecord.Attributes.BikeAccessForward!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessForward!.Values[2].Value.Should().Be(flatSegment1.BikeAccessForward);
        dynamicRecord.Attributes.BikeAccessForward!.Values[2].Side.Should().Be(RoadSegmentAttributeSide.Both);
    }

    [Fact]
    public void WhenSegmentsConnectToIntegrationSegment_ThenNotMerged()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentGeometryDrawMethodV2>();
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            TempId = new(1),
            RoadSegmentId = new (1),
            Geometry = BuildRoadSegmentGeometry(650000, 650000, 650010, 650000),
            CarAccessForward = false
        };
        var flatSegment2 = flatSegment1 with
        {
            TempId = new (2),
            RoadSegmentId = new (2),
            Geometry = BuildRoadSegmentGeometry(650010, 650000, 650010, 650010),
            CarAccessForward = true
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(650000, 650000)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(2),
                Geometry = new Point(650010, 650000)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(3),
                Geometry = new Point(650010, 650010)
            },
        };

        var nodeLocationsConnectedToIntegrationSegment = new[]
        {
            new Point(650010.01, 650000)
        };

        // Act
        var unflattenResult = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes,
            overrideStructuralNodeLocations: nodeLocationsConnectedToIntegrationSegment);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(2);
        var segment1 = records[0];
        segment1.Attributes.RoadSegmentId.Should().Be(new RoadSegmentId(1));

        var segment2 = records[1];
        segment2.Attributes.RoadSegmentId.Should().Be(new RoadSegmentId(2));
    }

    [Fact]
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocationAndSegmentsHaveEqualGeometryDirections_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (0 0)
LINESTRING (0 0, 10 0, 10 -30)
POINT (10 -30)
LINESTRING (20 -10, 0 -10, 0 0)
POINT (20 -10)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 10 0, 10 -30))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((20 -10, 0 -10, 0 0))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocationAndSegmentsHaveEqualGeometryDirections_AntwerpScenario_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (654110.9845983858 709308.409759216)
MULTILINESTRING ((654110.9845983858 709308.409759216, 654106.2698538858 709315.295130346, 654102.9703328109 709320.1136902105, 654099.6148028328 709325.0142425802, 654075.9001037186 709359.2410821617, 654050.1220157774 709396.9986401321, 654036.2948218792 709417.2527932338, 654031.690104179 709423.8861791464))
POINT (654031.690104179 709423.8861791464)
MULTILINESTRING ((654031.690104179 709423.8861791464, 654030.4679315636 709425.4940176131, 654027.3657301211 709427.55262681, 654023.1457201465 709428.0971166426, 654019.2458442166 709427.461656021, 654015.6482527723 709424.363255037, 654013.6718209455 709419.7250627205, 654013.5392840113 709415.7840844085, 654015.0766352502 709412.6162985321, 654017.9599208105 709409.8626697445, 654021.2730181363 709408.6700775102, 654025.2849518942 709408.798556312, 654029.3776839994 709410.6410284732, 654031.5443122011 709413.5812597377, 654032.7020145183 709415.9983752668, 654033.0137061053 709418.5993878245, 654033.2672993005 709422.0473853722, 654033.3262946636 709430.6243108483, 654032.813361918 709438.6491731349, 654032.4523215394 709447.5770450002))
POINT (654032.4523215394 709447.5770450002)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((654110.9845983858 709308.409759216, 654106.2698538858 709315.295130346, 654102.9703328109 709320.1136902105, 654099.6148028328 709325.0142425802, 654075.9001037186 709359.2410821617, 654050.1220157774 709396.9986401321, 654036.2948218792 709417.2527932338, 654031.690104179 709423.8861791464))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((654031.690104179 709423.8861791464, 654030.4679315636 709425.4940176131, 654027.3657301211 709427.55262681, 654023.1457201465 709428.0971166426, 654019.2458442166 709427.461656021, 654015.6482527723 709424.363255037, 654013.6718209455 709419.7250627205, 654013.5392840113 709415.7840844085, 654015.0766352502 709412.6162985321, 654017.9599208105 709409.8626697445, 654021.2730181363 709408.6700775102, 654025.2849518942 709408.798556312, 654029.3776839994 709410.6410284732, 654031.5443122011 709413.5812597377, 654032.7020145183 709415.9983752668, 654033.0137061053 709418.5993878245, 654033.2672993005 709422.0473853722, 654033.3262946636 709430.6243108483, 654032.813361918 709438.6491731349, 654032.4523215394 709447.5770450002))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocationAndSegmentsHaveOpposingGeometryDirections_GentScenario_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (608304.6375825119 697855.6307120398)
MULTILINESTRING ((608304.6375825119 697855.6307120398, 608298.2752848073 697855.1448272364, 608287.9568126021 697853.3973479671, 608281.4483186782 697851.6503311321, 608275.5748868118 697849.4271395225, 608269.0667003578 697845.2988826083, 608260.6539988992 697837.5191725586, 608191.6080791075 697760.3588751908, 608172.7196256953 697739.5604928425, 608167.6404963041 697733.0511729671, 608163.9900652813 697726.8595268913, 608160.4985937799 697719.0804082463, 608158.1183023715 697711.7776689511, 608157.0078008454 697706.0625686944, 608156.0562898325 697698.4425035194, 608155.2636765533 697689.7112167468, 608152.0075813096 697629.6400306793, 608148.6121068295 697566.997454701, 608147.6614857704 697552.5511820819, 608148.7740425299 697542.3913808484, 608150.8390345882 697532.7079397924, 608153.5388567147 697524.2945691571, 608157.6673805519 697516.1988621261, 608162.5894830176 697509.3732495336, 608169.0992156528 697501.436581566, 608177.5138595379 697494.1351365745, 608184.4994314689 697489.373503916, 608193.0723869532 697485.4058048604, 608203.3915617382 697481.5970687643, 608213.2342029925 697480.0107569052, 608223.0767005223 697479.5356907155, 608233.8715053163 697480.4894767385, 608242.7612034376 697482.3955314364, 608252.9205629488 697486.8417221503, 608261.9686207823 697491.7640257087, 608269.2703518271 697497.3211146714, 608274.667124762 697502.7192266257, 608279.2701036652 697508.4347407976, 608284.8252981207 697516.3728606347, 608288.4754579151 697524.628244469, 608291.0144590792 697532.2485003993, 608293.5531738271 697542.0912427995, 608294.1867152667 697553.3624948896, 608293.3914980118 697564.7923293281, 608290.6915982716 697573.8407001952, 608287.3568058637 697582.0952465078, 608283.387136233 697589.3972245976, 608278.3062253387 697596.6990693863, 608271.7965501436 697604.1594914608, 608264.0169462006 697611.1435101507, 608254.4912697193 697616.8573349612, 608246.553341647 697620.5076073976, 608236.0755565866 697623.2050831262, 608226.3916585746 697624.7914133398, 608213.5328832857 697625.5836143773, 608201.150333593 697626.5346261933))
POINT (608201.150333593 697626.5346261933)
MULTILINESTRING ((608201.150333593 697626.5346261933, 608182.7353217475 697627.4849094395, 608152.0075813096 697629.6400306793, 608114.1947919748 697632.2920728372, 608097.5257272108 697635.465056682, 608077.6816134318 697639.1668151608, 608057.5730242337 697642.0748035433, 608043.8853908601 697648.009161504))
POINT (608043.8853908601 697648.009161504)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((608304.6375825119 697855.6307120398, 608298.2752848073 697855.1448272364, 608287.9568126021 697853.3973479671, 608281.4483186782 697851.6503311321, 608275.5748868118 697849.4271395225, 608269.0667003578 697845.2988826083, 608260.6539988992 697837.5191725586, 608191.6080791075 697760.3588751908, 608172.7196256953 697739.5604928425, 608167.6404963041 697733.0511729671, 608163.9900652813 697726.8595268913, 608160.4985937799 697719.0804082463, 608158.1183023715 697711.7776689511, 608157.0078008454 697706.0625686944, 608156.0562898325 697698.4425035194, 608155.2636765533 697689.7112167468, 608152.0075813096 697629.6400306793, 608148.6121068295 697566.997454701, 608147.6614857704 697552.5511820819, 608148.7740425299 697542.3913808484, 608150.8390345882 697532.7079397924, 608153.5388567147 697524.2945691571, 608157.6673805519 697516.1988621261, 608162.5894830176 697509.3732495336, 608169.0992156528 697501.436581566, 608177.5138595379 697494.1351365745, 608184.4994314689 697489.373503916, 608193.0723869532 697485.4058048604, 608203.3915617382 697481.5970687643, 608213.2342029925 697480.0107569052, 608223.0767005223 697479.5356907155, 608233.8715053163 697480.4894767385, 608242.7612034376 697482.3955314364, 608252.9205629488 697486.8417221503, 608261.9686207823 697491.7640257087, 608269.2703518271 697497.3211146714, 608274.667124762 697502.7192266257, 608279.2701036652 697508.4347407976, 608284.8252981207 697516.3728606347, 608288.4754579151 697524.628244469, 608291.0144590792 697532.2485003993, 608293.5531738271 697542.0912427995, 608294.1867152667 697553.3624948896, 608293.3914980118 697564.7923293281, 608290.6915982716 697573.8407001952, 608287.3568058637 697582.0952465078, 608283.387136233 697589.3972245976, 608278.3062253387 697596.6990693863, 608271.7965501436 697604.1594914608, 608264.0169462006 697611.1435101507, 608254.4912697193 697616.8573349612, 608246.553341647 697620.5076073976, 608236.0755565866 697623.2050831262, 608226.3916585746 697624.7914133398, 608213.5328832857 697625.5836143773, 608201.150333593 697626.5346261933))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((608201.150333593 697626.5346261933, 608182.7353217475 697627.4849094395, 608152.0075813096 697629.6400306793, 608114.1947919748 697632.2920728372, 608097.5257272108 697635.465056682, 608077.6816134318 697639.1668151608, 608057.5730242337 697642.0748035433, 608043.8853908601 697648.009161504))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocationAndSegmentsHaveOpposingGeometryDirections_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (0 0)
LINESTRING (0 0, 10 0, 10 -30)
POINT (10 -30)
LINESTRING (0 0, 0 -10, 20 -10)
POINT (20 -10)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 10 0, 10 -30))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((20 -10, 0 -10, 0 0))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNodesFoundNearExpectedLocations_MultipleTimes_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (0 0)
LINESTRING (0 0, 10 0, 10 -30)
POINT (10 -30)
LINESTRING (0 0, 0 -10, 20 -10)
POINT (20 -10)
LINESTRING (20 -10, 40 -10, 40 0)
POINT (40 0)
LINESTRING (40 0, 30 0, 30 -30)
POINT (30 -30)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(3);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 10 0, 10 -30))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((40 0, 40 -10, 20 -10, 0 -10, 0 0))");

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((30 -30, 30 0, 40 0))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNoNodeNearExpectedLocation_ThenImpossibleSituationBecauseSelfIntersectingGeometryIsValidatedEarlier()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
LINESTRING (20 -10, 0 -10, 0 0, 10 0, 10 -30)
POINT (10 -30)
LINESTRING (20 -10, 30 -10)
POINT (20 -10)
POINT (30 -10)
";

        // Act
        var act = () => Unflatten(fixture, scenario);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WhenSameStartEndNodeAndNodeFoundInBetween_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (10 0)
LINESTRING (10 0, 0 0, 0 10, 10 10)
POINT (10 10)
LINESTRING (10 10, 10 0)
LINESTRING (10 0, 20 0)
POINT (20 0)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(3);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 0, 0 0, 0 10, 10 10))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 10, 10 0))");

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 0, 20 0))");
    }

    [Fact]
    public void WhenSameStartEndNodeAndMultipleNodesFoundInBetween_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (0 10)
LINESTRING (0 10, 10 10)
POINT (10 10)
LINESTRING (10 10, 10 0)
POINT (10 0)
LINESTRING (10 0, 0 0)
POINT (0 0)
LINESTRING (0 0, 0 10)

LINESTRING (10 0, 20 0)
POINT (20 0)
LINESTRING (10 0, 20 -10)
POINT (20 -10)
";
        var flatSegment = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>();

        // Act
        var unflattenResult = Unflatten(fixture, scenario, flatSegmentFactory: () => flatSegment);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(4);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 0, 0 0))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 0 10, 10 10, 10 0))");

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 0, 20 0))");

        var dynamicRecord4 = records[3];
        dynamicRecord4.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 0, 20 -10))");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WhenSameStartEndNodeAndNodeFoundInBetween_RegardlessTheOrderOfSegments_LebbekeScenario_ThenSuccess(bool swapSegments)
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var geometry1 = "MULTILINESTRING ((630236.3268711214 685179.3550339863, 630226.2159030889 685179.6738534672, 630223.9267510582 685181.0035841567, 630222.5944110844 685183.7634241804, 630222.3710468117 685186.6523934267, 630222.8543037425 685192.4924403522, 630224.618005435 685202.6436297903, 630225.5646459815 685213.3237230312, 630225.9860405741 685218.0797645478, 630226.0734171955 685222.9967667991))";
        var geometry2 = "MULTILINESTRING ((630226.0734171955 685222.9967667991, 630231.3261840874 685216.6413904391, 630232.2073024663 685215.6564949136, 630243.3778040812 685203.1658190899, 630250.4817780753 685195.0746611916, 630252.1661178141 685192.297862173, 630252.6453894484 685190.1269215252, 630252.7617249074 685187.4729392678, 630251.9130670413 685184.8208443839, 630250.5833514191 685182.651692599, 630249.013545644 685181.2075116644, 630245.5137229114 685180.0061051091, 630238.276864998 685179.2932616565, 630236.3268711214 685179.3550339863))";

        var firstSegment = swapSegments ? geometry2 : geometry1;
        var secondSegment = swapSegments ? geometry1 : geometry2;
        var scenario = @$"
POINT (630200 685222)
LINESTRING (630200 685222, 630226.0734171955 685222.9967667991)
POINT (630226.0734171955 685222.9967667991)
{firstSegment}
{secondSegment}
POINT (630236.3268711214 685179.3550339863)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;
        var consumedRoadNodeIds = unflattenResult.ConsumedRoadNodeIds;

        // Assert
        records.Should().HaveCount(3);
        consumedRoadNodeIds.Should().BeEmpty();

        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((630200 685222, 630226.0734171955 685222.9967667991))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be(firstSegment);

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be(secondSegment);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void WhenSameStartEndNodeButNotConnectedToStructuralNode_ThenRoadSegmentIdsAreUsedForSplittingCoordinatesRegardlessOfTheOrderOfSegments(int moveSegmentsCount)
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            TempId = new(1),
            RoadSegmentId = new(1),
            Geometry = BuildRoadSegmentGeometry((0, 0), (2.5, 2.5), (5, 5))
        };
        var flatSegment2a = flatSegment1 with
        {
            TempId = new(2),
            RoadSegmentId = new(2),
            Geometry = BuildRoadSegmentGeometry((5, 5), (7.5, 7.5), (10, 0))
        };
        var flatSegment2b = flatSegment2a with
        {
            TempId = new(3),
            Geometry = BuildRoadSegmentGeometry(10, 0, 5, -5)
        };
        var flatSegment2c = flatSegment2a with
        {
            TempId = new(4),
            Geometry = BuildRoadSegmentGeometry(5, -5, 0, 0)
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2a,
            flatSegment2b,
            flatSegment2c,
        };
        // to ensure the order of records does not matter
        flatSegments = flatSegments.Skip(flatSegments.Length - moveSegmentsCount).Take(moveSegmentsCount)
            .Concat(flatSegments.Take(flatSegments.Length - moveSegmentsCount))
            .ToArray();

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(0, 0)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(2),
                Geometry = new Point(5, 5)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(3),
                Geometry = new Point(10, 0)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(4),
                Geometry = new Point(5, -5)
            },
        };

        // Act
        var unflattenResult = Unflatten(FeatureType.Extract, flatSegments, nodes,
            roadSegmentIdProvider: new ExtractRoadSegmentIdProvider());
        var records = unflattenResult.RoadSegments;
        var consumedRoadNodeIds = unflattenResult.ConsumedRoadNodeIds;

        // Assert
        records.Should().HaveCount(2);
        consumedRoadNodeIds.Should().NotContain(new RoadNodeId(1));
        consumedRoadNodeIds.Should().NotContain(new RoadNodeId(2));
        consumedRoadNodeIds.Should().Contain(new RoadNodeId(3));
        consumedRoadNodeIds.Should().Contain(new RoadNodeId(4));

        var dynamicRecord1 = records.Single(x => x.Attributes.RoadSegmentId == new RoadSegmentId(1));
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 2.5 2.5, 5 5))");

        var dynamicRecord2 = records.Single(x => x.Attributes.RoadSegmentId == new RoadSegmentId(2));
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((5 5, 7.5 7.5, 10 0, 5 -5, 0 0))");
    }

    [Fact]
    public void WhenSameStartEndNodeButNotConnectedToStructuralNode_AndOriginalTemporarySchijnknoopBecomesStructuralNode_ThenTemporarySchijnknoopIsUsed()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            TempId = new(1),
            RoadSegmentId = new(1),
            Geometry = BuildRoadSegmentGeometry((0, 0), (2.5, 2.5), (5, 5))
        };
        var flatSegment2a = flatSegment1 with
        {
            TempId = new(2),
            RoadSegmentId = new(2),
            Geometry = BuildRoadSegmentGeometry((5, 5), (7.5, 7.5), (10, 0))
        };
        var flatSegment2b = flatSegment2a with
        {
            TempId = new(3),
            Geometry = BuildRoadSegmentGeometry(10, 0, 5, -5)
        };
        var flatSegment2c = flatSegment2a with
        {
            TempId = new(4),
            Geometry = BuildRoadSegmentGeometry(5, -5, 0, 0)
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2a,
            flatSegment2b,
            flatSegment2c,
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = RoadNodeConstants.InitialTemporarySchijnknoopId,
                Geometry = new Point(0, 0)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(2),
                Geometry = new Point(5, 5)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(3),
                Geometry = new Point(10, 0)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(4),
                Geometry = new Point(5, -5)
            },
        };

        // Act
        var unflattenResult = Unflatten(FeatureType.Extract, flatSegments, nodes,
            roadSegmentIdProvider: new ExtractRoadSegmentIdProvider());
        var records = unflattenResult.RoadSegments;
        var consumedRoadNodeIds = unflattenResult.ConsumedRoadNodeIds;
        var usedRoadNodeIds = unflattenResult.UsedRoadNodeIds;

        // Assert
        records.Should().HaveCount(2);
        consumedRoadNodeIds.Should().NotContain(new RoadNodeId(1_000_000_000));
        consumedRoadNodeIds.Should().NotContain(new RoadNodeId(2));
        consumedRoadNodeIds.Should().Contain(new RoadNodeId(3));
        consumedRoadNodeIds.Should().Contain(new RoadNodeId(4));
        usedRoadNodeIds.Should().HaveCount(2);
        usedRoadNodeIds.Should().Contain(new RoadNodeId(1_000_000_000));
        usedRoadNodeIds.Should().Contain(new RoadNodeId(2));

        var dynamicRecord1 = records.Single(x => x.Attributes.RoadSegmentId == new RoadSegmentId(1));
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 2.5 2.5, 5 5))");

        var dynamicRecord2 = records.Single(x => x.Attributes.RoadSegmentId == new RoadSegmentId(2));
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((5 5, 7.5 7.5, 10 0, 5 -5, 0 0))");
    }

    [Fact]
    public void WhenSameStartEndNodeAndNoNodeInBetween_ThenImpossibleSituationBecauseSameStartAndEndNodeIsValidatedEarlier()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (0 0)
LINESTRING (0 0, 10 0, 10 10, 0 10, 0 0)
";

        // Act
        var act = () => Unflatten(fixture, scenario);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WhenMultipleIntersectionsWithSameOtherGeometryAndNodeFoundInBetween_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
LINESTRING (0 0, 40 0)
POINT (0 0)
POINT (40 0)
LINESTRING (10 -10, 10 10, 15 10)
POINT (15 10)
LINESTRING (15 10, 20 10, 20 -10)
POINT (20 -10)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(3);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 40 0))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 -10, 10 10, 15 10))");

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((15 10, 20 10, 20 -10))");
    }

    [Fact]
    public void WhenMultipleIntersectionsWithSameOtherGeometryAndNodesFoundInBetween_MultipleTimes_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
LINESTRING (0 0, 100 0)
POINT (0 0)
POINT (100 0)
LINESTRING (10 -10, 10 10, 15 10)
POINT (15 10)
LINESTRING (15 10, 20 10, 20 -10)
POINT (20 -10)
LINESTRING (50 -10, 50 10, 55 10)
POINT (55 10)
LINESTRING (55 10, 60 10, 60 -10)
POINT (60 -10)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(5);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 100 0))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((10 -10, 10 10, 15 10))");

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((15 10, 20 10, 20 -10))");

        var dynamicRecord4 = records[3];
        dynamicRecord4.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((50 -10, 50 10, 55 10))");

        var dynamicRecord5 = records[4];
        dynamicRecord5.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((55 10, 60 10, 60 -10))");
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    public void With2FlatSegmentsWithStatusNotGerealiseerd_ThenReadWithoutNodes(string status)
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            Status = RoadSegmentStatusV2.Gerealiseerd
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0),
            Status = RoadSegmentStatusV2.Parse(status)
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(0, 0)
            },
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(2),
                Geometry = new Point(50, 0)
            },
        };

        // Act
        var unflattenResult = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);
        var records = unflattenResult.RoadSegments;
        var consumedRoadNodeIds = unflattenResult.ConsumedRoadNodeIds;

        // Assert
        consumedRoadNodeIds.Should().NotContain(new RoadNodeId(1));
        consumedRoadNodeIds.Should().NotContain(new RoadNodeId(2));

        records.Should().HaveCount(2);

        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 50 0))");
        dynamicRecord1.Attributes.Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((50 0, 100 0))");
        dynamicRecord2.Attributes.Status!.ToString().Should().Be(status);
    }

    [Fact]
    public void With2FlatSegmentsWithDifferentMethod_ThenIngemetenIfLongPartIsMoreThan75PercentOfMergedLength()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 80, 0),
            Method = RoadSegmentGeometryDrawMethodV2.Ingemeten
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(80, 0, 100, 0),
            Method = RoadSegmentGeometryDrawMethodV2.Ingeschetst
        };
        var flatSegments = new[]
        {
            flatSegment1,
            flatSegment2
        };

        var nodes = new[]
        {
            new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new RoadNodeId(1),
                Geometry = new Point(80, 0),
                Grensknoop = false
            }
        };

        // Act
        var unflattenResult = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);
        var result = unflattenResult.RoadSegments;

        // Assert
        result.Should().HaveCount(1);
        result[0].Attributes.Method.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
    }

    private (List<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, List<RoadNodeId> ConsumedRoadNodeIds, List<RoadNodeId> UsedRoadNodeIds) Unflatten(
        IFixture fixture,
        string scenarioWkt,
        Func<RoadSegmentFeatureCompareWithFlatAttributes> flatSegmentFactory = null)
    {
        var idCounter = 0;
        var scenarioData = scenarioWkt
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var id = ++idCounter;
                var geometry = new WKTReader().Read(line);
                return (id, geometry);
            })
            .ToArray();

        var nodes = scenarioData
            .Where(x => x.geometry is Point)
            .Select(x => new RoadNodeFeatureCompareAttributes
            {
                RoadNodeId = new(x.id),
                Geometry = (Point)x.geometry
            })
            .ToList();

        var flatSegments = scenarioData
            .Where(x => x.geometry is LineString or MultiLineString)
            .Select(x => (flatSegmentFactory ?? fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>)() with
            {
                TempId = new(x.id),
                RoadSegmentId = new(x.id),
                Geometry = x.geometry.ToMultiLineString()
            })
            .ToList();

        return Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);
    }

    private (List<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, List<RoadNodeId> ConsumedRoadNodeIds, List<RoadNodeId> UsedRoadNodeIds) Unflatten(
        FeatureType featureType,
        IReadOnlyList<RoadSegmentFeatureCompareWithFlatAttributes> flatSegments,
        IReadOnlyList<RoadNodeFeatureCompareAttributes> roadNodes,
        IRoadSegmentIdProvider? roadSegmentIdProvider = null,
        IReadOnlyCollection<Point>? overrideStructuralNodeLocations = null)
    {
        var flatRecords = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
        for (var i = 0; i < flatSegments.Count; i++)
        {
            flatRecords.Add(new(new RecordNumber(i + 1), flatSegments[i]));
            TestOutputHelper.WriteLine($"Flat segment {flatSegments[i].RoadSegmentId}: {JsonConvert.SerializeObject(flatSegments[i], Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        }

        roadSegmentIdProvider ??= new NextRoadSegmentIdProvider(new RoadSegmentId(10000));
        var translateContext = new ZipArchiveEntryFeatureCompareTranslateContext(null, ZipArchiveMetadata.Empty);

        for (var i = 0; i < roadNodes.Count; i++)
        {
            translateContext.AddRoadNodeRecords([
                new RoadNodeFeatureCompareRecord(
                    featureType,
                    new RecordNumber(i + 1),
                    roadNodes[i],
                    roadNodes[i].RoadNodeId,
                    RecordType.Identical)
            ]);
            TestOutputHelper.WriteLine($"Node {roadNodes[i].RoadNodeId}: {JsonConvert.SerializeObject(roadNodes[i], Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        }

        var unflattenedRecords = RoadSegmentUnflattener.Unflatten(
            featureType,
            flatRecords,
            overrideStructuralNodeLocations ?? [],
            roadSegmentIdProvider,
            new OgcFeaturesCache([]),
            translateContext,
            CancellationToken.None);

        TestOutputHelper.WriteLine(string.Empty);
        TestOutputHelper.WriteLine("-->");
        TestOutputHelper.WriteLine($"Unflatten result: {JsonConvert.SerializeObject(unflattenedRecords, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");

        unflattenedRecords.Problems.ThrowIfError();

        return (unflattenedRecords.RoadSegments, unflattenedRecords.ConsumedRoadNodeIds.ToList(), unflattenedRecords.UsedRoadNodeIds.ToList());
    }

    private static MultiLineString BuildRoadSegmentGeometry(int x1, int y1, int x2, int y2)
    {
        return BuildRoadSegmentGeometry(new Coordinate(x1, y1), new Coordinate(x2, y2));
    }

    private static MultiLineString BuildRoadSegmentGeometry(params (int X, int Y)[] coordinates)
    {
        return BuildRoadSegmentGeometry(coordinates.Select(c => new Coordinate(c.X, c.Y)).ToArray());
    }

    private static MultiLineString BuildRoadSegmentGeometry(Coordinate start, Coordinate end)
    {
        return BuildRoadSegmentGeometry([start, end]);
    }

    private static MultiLineString BuildRoadSegmentGeometry(params Coordinate[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates)]);
    }
}
