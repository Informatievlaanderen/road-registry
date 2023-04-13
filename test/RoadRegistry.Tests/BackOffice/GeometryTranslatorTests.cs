namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using Xunit;
using Point = Be.Vlaanderen.Basisregisters.Shaperon.Point;

public class GeometryTranslatorTests
{
    private readonly WKTReader _reader;

    public GeometryTranslatorTests()
    {
        _reader = new WKTReader();
    }

    [Theory]
    [InlineData(GeometryTranslatorTestCases.ValidPolygon)]
    [InlineData(GeometryTranslatorTestCases.ValidMultiPolygon)]
    [InlineData(GeometryTranslatorTestCases.ValidPolygonWithHoles)]
    [InlineData(GeometryTranslatorTestCases.ValidMultiPolygonWithHoles)]
    [InlineData(GeometryTranslatorTestCases.ValidGeometryWithHoles)]
    public void TranslateToRoadNetworkExtractGeometryCanHandleValidGeometries(string geometryString)
    {
        var geometry = _reader.Read(geometryString) as IPolygonal;

        var result = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(geometry);

        result.Should().NotBeNull();
    }

    //[Theory]
    //[InlineData("MULTIPOLYGON (((47182.31631790832 213756.1620241264, 49909.981260447836 211088.40229899064, 49769.196609037084 213469.54159113392, 49351.315239327145 211770.4327189494, 47886.50513430138 211201.36943419836, 48903.27133641396 213676.3703008592, 48704.6976248272 212475.69851671532, 48079.82802056009 211632.79854945187, 48062.83646131064 213610.05794444587, 47106.47782324614 211192.25161007792, 47182.31631790832 213756.1620241264)))")]
    //public void TranslateGeometryBackAndForth(string wkt)
    //{
    //    var geometry = (IPolygonal)_reader.Read(wkt);

    //    var roadNetworkExtractGeometry = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(geometry, 0);
    //    var result = GeometryTranslator.Translate(roadNetworkExtractGeometry);
    //    var resultWkt = result.ToString();

    //    Assert.Equal(wkt, resultWkt);
    //}

    [Fact]
    public void MissingMeasuresAreFilledInCorrectly()
    {
        var points = new[] { new Point(0, 0), new Point(0, 3), new Point(0, 10) };
        var polyline = new PolyLineM(
            new BoundingBox2D(points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y)),
            new[] { 0 },
            points,
            points.Select(x => double.NaN).ToArray()
        );
        
        var geometryLineString = GeometryTranslator.ToMultiLineString(polyline);

        var actualMeasures = geometryLineString.GetOrdinates(Ordinate.M);
        var expectedMeasures = points.Select(x => x.Y).ToArray();

        Assert.Equal(expectedMeasures, actualMeasures);
    }

    [Theory]
    [InlineData(GeometryTranslatorTestCases.ValidGmlMultiLineString)]
    [InlineData(GeometryTranslatorTestCases.ValidGmlLineString)]
    public void ParseGmlLineString(string gml)
    {
        var geometry = GeometryTranslator.ParseGmlLineString(gml);

        Assert.NotNull(geometry);
        Assert.Equal(31370, geometry.SRID);
    }

    [Theory]
    [InlineData(@"<gml:MultiLineString srsName=""https://www.opengis.net/def/crs/EPSG/0/10000"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:lineStringMember srsName=""https://www.opengis.net/def/crs/EPSG/0/10000"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
</gml:lineStringMember>
</gml:MultiLineString>")]
    [InlineData(@"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/10000"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368 181577 217400 181499</gml:posList>
</gml:LineString>")]
    public void ParseGmlLineStringWithInvalidSrid(string gml)
    {
        var geometry = GeometryTranslator.ParseGmlLineString(gml);

        Assert.NotNull(geometry);
        Assert.Equal(10000, geometry.SRID);
    }
}
