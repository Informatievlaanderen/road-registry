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
        fixture.Freeze<RoadSegmentStatusV2>();

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
        var records = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);

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
        fixture.Freeze<RoadSegmentStatusV2>();

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
        var records = Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);

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
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocationAndSegmentsHaveEqualGeometryDirections_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentStatusV2>();

        var scenario = @"
POINT (0 0)
LINESTRING (0 0, 10 0, 10 -30)
POINT (10 -30)
LINESTRING (20 -10, 0 -10, 0 0)
POINT (20 -10)
";

        // Act
        var records = Unflatten(fixture, scenario);

        // Assert
        records.Should().HaveCount(2);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 10 0, 10 -30))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((20 -10, 0 -10, 0 0))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNodeFoundNearExpectedLocationAndSegmentsHaveOpposingGeometryDirections_ThenSuccess()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentStatusV2>();

        var scenario = @"
POINT (0 0)
LINESTRING (0 0, 10 0, 10 -30)
POINT (10 -30)
LINESTRING (0 0, 0 -10, 20 -10)
POINT (20 -10)
";

        // Act
        var records = Unflatten(fixture, scenario);

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
        fixture.Freeze<RoadSegmentStatusV2>();

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
        var records = Unflatten(fixture, scenario);

        // Assert
        records.Should().HaveCount(4);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((0 0, 10 0, 10 -30))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((20 -10, 0 -10, 0 0))");

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((20 -10, 40 -10, 40 0))");

        var dynamicRecord4 = records[3];
        dynamicRecord4.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((40 0, 30 0, 30 -30))");
    }

    [Fact]
    public void WhenSelfIntersectsAndNoNodeNearExpectedLocation_ThenImpossibleSituationBecauseSelfIntersectingGeometryIsValidatedEarlier()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentStatusV2>();

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
        fixture.Freeze<RoadSegmentStatusV2>();

        var scenario = @"
POINT (10 0)
LINESTRING (10 0, 0 0, 0 10, 10 10)
POINT (10 10)
LINESTRING (10 10, 10 0)
LINESTRING (10 0, 20 0)
POINT (20 0)
";

        // Act
        var records = Unflatten(fixture, scenario);

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
        var records = Unflatten(fixture, scenario, flatSegmentFactory: () => flatSegment);

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
        fixture.Freeze<RoadSegmentStatusV2>();

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
        var records = Unflatten(fixture, scenario);

        // Assert
        records.Should().HaveCount(3);
        var dynamicRecord1 = records[0];
        dynamicRecord1.Attributes.Geometry.AsText().Should().Be("MULTILINESTRING ((630200 685222, 630226.0734171955 685222.9967667991))");

        var dynamicRecord2 = records[1];
        dynamicRecord2.Attributes.Geometry.AsText().Should().Be(firstSegment);

        var dynamicRecord3 = records[2];
        dynamicRecord3.Attributes.Geometry.AsText().Should().Be(secondSegment);
    }

    [Fact]
    public void WhenSameStartEndNodeAndNoNodeInBetween_ThenImpossibleSituationBecauseSameStartAndEndNodeIsValidatedEarlier()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentStatusV2>();

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
        fixture.Freeze<RoadSegmentStatusV2>();

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
        var records = Unflatten(fixture, scenario);

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
        fixture.Freeze<RoadSegmentStatusV2>();

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
        var records = Unflatten(fixture, scenario);

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

    [Fact]
    public void With2FlatSegmentsWithDifferentStatus_ThenProblem()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            Status = RoadSegmentStatusV2.NietGerealiseerd
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0),
            Status = RoadSegmentStatusV2.BuitenGebruik
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
        var act = () => Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);

        // Assert
        var ex = act.Should().Throw<ZipArchiveValidationException>().Which;
        ex.Problems.Should().Contain(x => x.Reason == "RoadNodeIsNotAllowed");
    }

    [Fact]
    public void With2FlatSegmentsWithDifferentMethod_ThenProblem()
    {
        // Arrange
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadSegmentId>();

        var flatSegment1 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(0, 0, 50, 0),
            Method = RoadSegmentGeometryDrawMethodV2.Ingeschetst
        };
        var flatSegment2 = fixture.Create<RoadSegmentFeatureCompareWithFlatAttributes>() with
        {
            Geometry = BuildRoadSegmentGeometry(50, 0, 100, 0),
            Method = null
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
        var act = () => Unflatten(fixture.Create<FeatureType>(), flatSegments, nodes);

        // Assert
        var ex = act.Should().Throw<ZipArchiveValidationException>().Which;
        ex.Problems.Should().Contain(x => x.Reason == "RoadNodeIsNotAllowed");
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> Unflatten(
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

    private List<RoadSegmentFeatureWithDynamicAttributes> Unflatten(
        FeatureType featureType,
        IReadOnlyList<RoadSegmentFeatureCompareWithFlatAttributes> flatSegments,
        IReadOnlyList<RoadNodeFeatureCompareAttributes> roadNodes)
    {
        var flatRecords = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
        for (var i = 0; i < flatSegments.Count; i++)
        {
            flatRecords.Add(new(new RecordNumber(i + 1), flatSegments[i]));
            TestOutputHelper.WriteLine($"Flat segment {flatSegments[i].RoadSegmentId}: {JsonConvert.SerializeObject(flatSegments[i], Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");
        }

        var roadSegmentIdProver = new NextRoadSegmentIdProvider(new RoadSegmentId(10000));
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
            roadSegmentIdProver,
            new OgcFeaturesCache([]),
            translateContext,
            CancellationToken.None).RoadSegments;

        TestOutputHelper.WriteLine(string.Empty);
        TestOutputHelper.WriteLine("-->");
        TestOutputHelper.WriteLine($"Dynamic segments: {JsonConvert.SerializeObject(unflattenedRecords, Formatting.Indented, SqsJsonSerializerSettingsProvider.CreateSerializerSettings())}");

        return unflattenedRecords;
    }

    private static MultiLineString BuildRoadSegmentGeometry(int x1, int y1, int x2, int y2)
    {
        return BuildRoadSegmentGeometry(new Coordinate(x1, y1), new Coordinate(x2, y2));
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
