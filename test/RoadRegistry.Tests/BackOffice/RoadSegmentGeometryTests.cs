namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Scenarios;

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

        var problems = geometry.GetProblemsForRoadSegmentGeometry(_fixture.Create<RoadSegmentId>(), VerificationContextTolerances.Default);
        problems.Should().BeEmpty();
    }

    [Theory]
    [InlineData("MULTILINESTRING((150034.72 156214.97,150279.62 156184.82,150034.72 156214.97))")]
    public void GivenSelfOverlap_ThenRoadSegmentGeometrySelfOverlaps(string wkt)
    {
        var geometry = new WKTReader().Read(wkt).ToMultiLineString().GetSingleLineString();

        var problems = geometry.GetProblemsForRoadSegmentGeometry(_fixture.Create<RoadSegmentId>(), VerificationContextTolerances.Default);
        problems.Should().ContainItemsAssignableTo<RoadSegmentGeometrySelfOverlaps>();
    }
}
