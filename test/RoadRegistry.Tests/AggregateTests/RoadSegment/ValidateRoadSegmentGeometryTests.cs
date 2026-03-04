namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Extensions;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment;

public class ValidateRoadSegmentGeometryTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ValidateRoadSegmentGeometryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void WhenGeometryLengthIsLessThan1Meter_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(0.99, 0)]).ToMultiLineString(),
            expectedErrorCodes: ["RoadSegmentGeometryLengthLessThanMinimum"]);
    }

    [Fact]
    public void WhenGeometryLengthIsTooLong_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(100000.01, 0)]).ToMultiLineString(),
            expectedErrorCodes: ["RoadSegmentGeometryLengthTooLong"]);
    }

    [Fact]
    public void WhenGeometrySelfOverlaps_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(2, 0), new Coordinate(1, 0), new Coordinate(5, 0)]).ToMultiLineString(),
            expectedErrorCodes: ["RoadSegmentGeometrySelfOverlaps"]);
    }

    [Fact]
    public void WhenGeometrySelfIntersects_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 1), new Coordinate(0, -1)]).ToMultiLineString(),
            expectedErrorCodes: ["RoadSegmentGeometrySelfIntersects"]);
    }

    [Fact]
    public void WhenVerticesAreTooClose_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(1.1, 0), new Coordinate(10, 0)]).ToMultiLineString(),
            expectedErrorCodes: ["RoadSegmentGeometryVerticesTooClose"]);
    }

    private void AssertValidateResult(MultiLineString geometry, string[] expectedErrorCodes)
    {
        var problems = geometry.ValidateRoadSegmentGeometryDomainV2(new RoadSegmentId(1));
        foreach (var problem in problems)
        {
            _testOutputHelper.WriteLine(problem.Describe());
        }

        if (expectedErrorCodes.Any())
        {
            foreach (var errorCode in expectedErrorCodes)
            {
                problems.Should().Contain(x => x.Reason == errorCode);
            }
        }
        else
        {
            problems.Should().BeEmpty();
        }
    }
}
