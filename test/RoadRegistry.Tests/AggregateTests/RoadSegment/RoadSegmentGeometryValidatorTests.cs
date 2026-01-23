namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Extensions;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment;

public class RoadSegmentGeometryValidatorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RoadSegmentGeometryValidatorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void WithOutlined_WhenGeometryLengthIsLessThanMinimum_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(1.99, 0)]).ToMultiLineString(),
            RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            expectedErrorCodes: ["RoadSegmentGeometryLengthLessThanMinimum"]);
    }

    [Fact]
    public void WithOutlined_WhenGeometryLengthIsTooLong_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(100000.01, 0)]).ToMultiLineString(),
            RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            expectedErrorCodes: ["RoadSegmentGeometryLengthTooLong"]);
    }

    [Fact]
    public void WithMeasured_WhenGeometryLengthIsZero_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString(),
            RoadSegmentGeometryDrawMethodV2.Ingemeten,
            expectedErrorCodes: ["RoadSegmentGeometryLengthIsZero"]);
    }

    [Fact]
    public void WithMeasured_WhenGeometryLengthIsTooLong_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(100000.01, 0)]).ToMultiLineString(),
            RoadSegmentGeometryDrawMethodV2.Ingemeten,
            expectedErrorCodes: ["RoadSegmentGeometryLengthTooLong"]);
    }

    [Fact]
    public void WithMeasured_WhenGeometrySelfOverlaps_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(0, 0), new Coordinate(5, 0)]).ToMultiLineString(),
            RoadSegmentGeometryDrawMethodV2.Ingemeten,
            expectedErrorCodes: ["RoadSegmentGeometrySelfOverlaps"]);
    }

    [Fact]
    public void WithMeasured_WhenGeometrySelfIntersects_ThenError()
    {
        AssertValidateResult(
            new LineString([new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 1), new Coordinate(0, -1)]).ToMultiLineString(),
            RoadSegmentGeometryDrawMethodV2.Ingemeten,
            expectedErrorCodes: ["RoadSegmentGeometrySelfIntersects"]);
    }

    private void AssertValidateResult(MultiLineString geometry, RoadSegmentGeometryDrawMethodV2 geometryDrawMethod, string[] expectedErrorCodes)
    {
        var problems = new RoadSegmentGeometryValidator().Validate(
            new RoadSegmentId(1),
            geometryDrawMethod,
            geometry);
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
