namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Extensions;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.ValueObjects.Problems;

public class RoadSegmentGeometryTests
{
    private Fixture _fixture;

    public RoadSegmentGeometryTests()
    {
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Fact]
    public void GivenValidGeometry_ThenNoProblems()
    {
        var geometry = GeometryTranslator.Translate(_fixture.Create<RoadSegmentGeometry>()).GetSingleLineString();

        var problems = geometry.ValidateRoadSegmentGeometry(_fixture.Create<RoadSegmentId>());
        problems.Should().BeEmpty();
    }

    [Theory]
    [InlineData("MULTILINESTRING((150034.72 156214.97,150279.62 156184.82,150034.72 156214.97))")]
    public void GivenSelfOverlap_ThenRoadSegmentGeometrySelfOverlaps(string wkt)
    {
        var geometry = new WKTReader().Read(wkt).ToMultiLineString().GetSingleLineString();

        var problems = geometry.ValidateRoadSegmentGeometry(_fixture.Create<RoadSegmentId>());
        problems.Should().ContainItemsAssignableTo<RoadSegmentGeometrySelfOverlaps>();
    }
}
