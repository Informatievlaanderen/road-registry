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

public class UnflattenByTopologyScenarios
{
    private ITestOutputHelper TestOutputHelper { get; }

    public UnflattenByTopologyScenarios(ITestOutputHelper testOutputHelper)
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
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            AccessRestriction = RoadSegmentAccessRestrictionV2.OpenbareWeg
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0),
            AccessRestriction = RoadSegmentAccessRestrictionV2.PrivateWeg
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

        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[0].Value.Should().Be(flatSegment1.LeftMaintenanceAuthorityId);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.MaintenanceAuthorityId!.Values[1].Value.Should().Be(flatSegment1.RightMaintenanceAuthorityId);

        dynamicRecord.Attributes.StreetNameId!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.StreetNameId!.Values[0].Value.Should().Be(flatSegment1.LeftSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Right);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Value.Should().Be(flatSegment1.RightSideStreetNameId);

        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Value.Should().Be(flatSegment1.CarAccessBackward);

        dynamicRecord.Attributes.CarAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.CarAccessForward!.Values[0].Value.Should().Be(flatSegment1.CarAccessForward);

        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessBackward);

        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessForward);
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

        dynamicRecord.Attributes.StreetNameId!.Values[0].Value.Should().Be(flatSegment1.LeftSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Left);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Value.Should().Be(flatSegment1.RightSideStreetNameId);
        dynamicRecord.Attributes.StreetNameId!.Values[1].Side.Should().Be(RoadSegmentAttributeSide.Right);

        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Value.Should().Be(flatSegment1.CarAccessBackward);
        dynamicRecord.Attributes.CarAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.CarAccessForward!.Values[0].Value.Should().Be(flatSegment1.CarAccessForward);
        dynamicRecord.Attributes.CarAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessBackward);
        dynamicRecord.Attributes.BikeAccessBackward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);

        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Value.Should().Be(flatSegment1.BikeAccessForward);
        dynamicRecord.Attributes.BikeAccessForward!.Values[0].Side.Should().Be(RoadSegmentAttributeSide.Both);
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
POINT (654110 709308)
MULTILINESTRING ((654110 709308, 654106 709315, 654102 709320, 654099 709325, 654075 709359, 654050 709396, 654036 709417, 654031 709423))
POINT (654031 709423)
MULTILINESTRING ((654031 709423, 654030 709425, 654027 709427, 654023 709428, 654019 709427, 654015 709424, 654013 709419, 654013 709415, 654015 709412, 654017 709409, 654021 709408, 654025 709408, 654029 709410, 654031 709413, 654032 709415, 654033 709418, 654033 709422, 654033 709430, 654032 709438, 654032 709447))
POINT (654032 709447)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((654110 709308, 654106 709315, 654102 709320, 654099 709325, 654075 709359, 654050 709396, 654036 709417, 654031 709423))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((654031 709423, 654030 709425, 654027 709427, 654023 709428, 654019 709427, 654015 709424, 654013 709419, 654013 709415, 654015 709412, 654017 709409, 654021 709408, 654025 709408, 654029 709410, 654031 709413, 654032 709415, 654033 709418, 654033 709422, 654033 709430, 654032 709438, 654032 709447))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocationAndSegmentsHaveOpposingGeometryDirections_GentScenario_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd);

        var scenario = @"
POINT (608304 697855)
MULTILINESTRING ((608304 697855, 608298 697855, 608287 697853, 608281 697851, 608275 697849, 608269 697845, 608260 697837, 608191 697760, 608172 697739, 608167 697733, 608163 697726, 608160 697719, 608158 697711, 608157 697706, 608156 697698, 608155 697689, 608152 697629, 608148 697566, 608147 697552, 608148 697542, 608150 697532, 608153 697524, 608157 697516, 608162 697509, 608169 697501, 608177 697494, 608184 697489, 608193 697485, 608203 697481, 608213 697480, 608223 697479, 608233 697480, 608242 697482, 608252 697486, 608261 697491, 608269 697497, 608274 697502, 608279 697508, 608284 697516, 608288 697524, 608291 697532, 608293 697542, 608294 697553, 608293 697564, 608290 697573, 608287 697582, 608283 697589, 608278 697596, 608271 697604, 608264 697611, 608254 697616, 608246 697620, 608236 697623, 608226 697624, 608213 697625, 608201 697626))
POINT (608201 697626)
MULTILINESTRING ((608201 697626, 608182 697627, 608152 697629, 608114 697632, 608097 697635, 608077 697639, 608057 697642, 608043 697648))
POINT (608043 697648)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((608304 697855, 608298 697855, 608287 697853, 608281 697851, 608275 697849, 608269 697845, 608260 697837, 608191 697760, 608172 697739, 608167 697733, 608163 697726, 608160 697719, 608158 697711, 608157 697706, 608156 697698, 608155 697689, 608152 697629, 608148 697566, 608147 697552, 608148 697542, 608150 697532, 608153 697524, 608157 697516, 608162 697509, 608169 697501, 608177 697494, 608184 697489, 608193 697485, 608203 697481, 608213 697480, 608223 697479, 608233 697480, 608242 697482, 608252 697486, 608261 697491, 608269 697497, 608274 697502, 608279 697508, 608284 697516, 608288 697524, 608291 697532, 608293 697542, 608294 697553, 608293 697564, 608290 697573, 608287 697582, 608283 697589, 608278 697596, 608271 697604, 608264 697611, 608254 697616, 608246 697620, 608236 697623, 608226 697624, 608213 697625, 608201 697626))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((608201 697626, 608182 697627, 608152 697629, 608114 697632, 608097 697635, 608077 697639, 608057 697642, 608043 697648))");
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

        var geometry1 = "MULTILINESTRING ((630236 685179, 630226 685179, 630223 685181, 630222 685183, 630222 685186, 630222 685192, 630224 685202, 630225 685213, 630225 685218, 630226 685222))";
        var geometry2 = "MULTILINESTRING ((630226 685222, 630231 685216, 630232 685215, 630243 685203, 630250 685195, 630252 685192, 630252 685190, 630252 685187, 630251 685184, 630250 685182, 630249 685181, 630245 685180, 630238 685179, 630236 685179))";

        var firstSegment = swapSegments ? geometry2 : geometry1;
        var secondSegment = swapSegments ? geometry1 : geometry2;
        var scenario = @$"
POINT (630200 685222)
LINESTRING (630200 685222, 630226 685222)
POINT (630226 685222)
{firstSegment}
{secondSegment}
POINT (630236 685179)
";

        // Act
        var unflattenResult = Unflatten(fixture, scenario);
        var records = unflattenResult.RoadSegments;
        var consumedRoadNodeIds = unflattenResult.ConsumedRoadNodeIds;

        // Assert
        records.Should().HaveCount(3);
        consumedRoadNodeIds.Should().BeEmpty();

        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((630200 685222, 630226 685222))");

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

        var unflattenedRecords = RoadSegmentUnflattener.UnflattenByTopology(
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
