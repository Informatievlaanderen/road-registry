namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

public class OgcFeaturesCacheTests
{
    [Fact]
    public void HasOverlapWithFeature_True()
    {
        var fixture = new Fixture();
        var lineString = new LineString([new Coordinate(0, 5), new Coordinate(100, 5)]).ToMultiLineString();

        var cache = new OgcFeaturesCache([
            new OgcFeature(fixture.Create<string>(), fixture.Create<string>(), new WKTReader().Read("POLYGON ((0 0, 0 10, 76 10, 76 0, 0 0)))"), [])
        ]);

        var hasOverlap = cache.HasOverlapWithFeatures(lineString, 0.75);
        hasOverlap.Should().BeTrue();
    }

    [Fact]
    public void HasOverlapWithMultipleFeature_True()
    {
        var fixture = new Fixture();
        var lineString = new LineString([new Coordinate(0, 5), new Coordinate(100, 5)]).ToMultiLineString();

        var cache = new OgcFeaturesCache([
            new OgcFeature(fixture.Create<string>(), fixture.Create<string>(), new WKTReader().Read("POLYGON ((0 0, 0 10, 30 10, 30 0, 0 0)))"), []),
            new OgcFeature(fixture.Create<string>(), fixture.Create<string>(), new WKTReader().Read("POLYGON ((30 0, 30 10, 76 10, 76 0, 30 0)))"), []),
        ]);

        var hasOverlap = cache.HasOverlapWithFeatures(lineString, 0.75);
        hasOverlap.Should().BeTrue();
    }

    [Fact]
    public void HasNoOverlapWithFeature_False()
    {
        var fixture = new Fixture();
        var lineString = new LineString([new Coordinate(0, 5), new Coordinate(100, 5)]).ToMultiLineString();

        var cache = new OgcFeaturesCache([
            new OgcFeature(fixture.Create<string>(), fixture.Create<string>(), new WKTReader().Read("POLYGON ((0 0, 0 10, 74 10, 74 0, 0 0)))"), [])
        ]);

        var hasOverlap = cache.HasOverlapWithFeatures(lineString, 0.75);
        hasOverlap.Should().BeFalse();
    }
}
