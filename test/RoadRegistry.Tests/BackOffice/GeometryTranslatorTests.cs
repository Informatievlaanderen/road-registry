namespace RoadRegistry.Tests.BackOffice;

using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using System.Linq;
using RoadRegistry.BackOffice.Extensions;
using LineString = NetTopologySuite.Geometries.LineString;
using MultiPolygon = GeoJSON.Net.Geometry.MultiPolygon;
using Point = Be.Vlaanderen.Basisregisters.Shaperon.Point;

public class GeometryTranslatorTests
{
    private readonly WKTReader _reader;

    public GeometryTranslatorTests()
    {
        _reader = new WKTReader();
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
    public void MeasuresAreUpdatedCorrectly_PolylineM()
    {
        var points = new[] { new Point(0, 0), new Point(0, 3), new Point(0, 10) };
        var invalidMeasures = points.Select(x => 1.0).ToArray();
        var polyline = new PolyLineM(
            new BoundingBox2D(points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y)),
            new[] { 0 },
            points,
            invalidMeasures
        );

        var geometryLineString = GeometryTranslator.ToMultiLineString(polyline);

        var actualMeasures = geometryLineString.GetOrdinates(Ordinate.M);
        var expectedMeasures = points.Select(x => x.Y).ToArray();

        Assert.Equal(expectedMeasures, actualMeasures);
    }

    [Fact]
    public void MeasuresAreUpdatedCorrectly_RoadSegmentGeometry()
    {
        var points = new[] { new Point(0, 0), new Point(0, 3), new Point(0, 10) };
        var invalidMeasures = points.Select(x => 1.0).ToArray();
        var polyline = new RoadSegmentGeometry
        {
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
            MultiLineString = new[]
            {
                new RoadRegistry.BackOffice.Messages.LineString
                {
                    Measures = invalidMeasures,
                    Points = points.Select(point => new RoadRegistry.BackOffice.Messages.Point { X = point.X, Y = point.Y }).ToArray()
                }
            }
        };

        var geometryLineString = GeometryTranslator.Translate(polyline);

        var actualMeasures = geometryLineString.GetOrdinates(Ordinate.M);
        var expectedMeasures = points.Select(x => x.Y).ToArray();

        Assert.Equal(expectedMeasures, actualMeasures);
    }

    [Fact]
    public void MeasuresAreUpdatedCorrectly_MultiLineString()
    {
        var points = new[] { new Point(0, 0), new Point(0, 3), new Point(0, 10) };
        
        var polyline = new MultiLineString(new [] { new LineString(points.Select(point => (Coordinate)new CoordinateM(point.X, point.Y, 1.0)).ToArray()) })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        var geometryLineString = GeometryTranslator.Translate(polyline);
        
        var actualMeasures = geometryLineString.MultiLineString[0].Measures;
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
        Assert.Equal(SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(), geometry.SRID);
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

    [Theory]
    [InlineData("MULTILINESTRING ((0 0, 1 0))", new[] { 0.0, 1.0 })]
    [InlineData("MULTILINESTRING ((0 0, 1 0, 2 0))", new[] { 0.0, 1.0, 2.0 })]
    [InlineData("MULTILINESTRING ((0 0, 1 0, 2 0, 1 0))", new[] { 0.0, 1.0, 2.0, 3.0 })]
    public void WithMeasureOrdinatesIsCorrect(string wkt, double[] expectedMeasures)
    {
        var geometry = (MultiLineString)new WKTReader().Read(wkt);
        var geometryWithMeasures = geometry.WithMeasureOrdinates();
        var actualMeasures = geometryWithMeasures.GetOrdinates(Ordinate.M);

        Assert.Equal(expectedMeasures, actualMeasures);
    }

    //[Fact]
    [Fact(Skip = "For debugging purposes, convert roadsegment geometry from read endpoint to wkt")]
    public void ConvertGeoJsonToWkt()
    {
        var path = "contour.geojson";
        var json = File.ReadAllText(path);
        var geojson = JsonConvert.DeserializeObject<MultiPolygon>(json);
        var wkt = geojson.ToMultiPolygon().AsText();
    }
}
