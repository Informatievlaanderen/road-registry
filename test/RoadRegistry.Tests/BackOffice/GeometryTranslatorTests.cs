namespace RoadRegistry.Tests.BackOffice;

using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Extensions;
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
    public void WhenConvertingPolylineMToMultiLineString_ThenMeasuresAreUpdatedCorrectly()
    {
        var points = new[] { new Point(0, 0), new Point(0, 3), new Point(0, 10) };
        var invalidMeasures = points.Select(x => 1.0).ToArray();
        var polyline = new PolyLineM(
            new BoundingBox2D(points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y)),
            new[] { 0 },
            points,
            invalidMeasures
        );

        var geometryLineString = GeometryTranslator.ToMultiLineStringLambert72(polyline);

        var actualMeasures = geometryLineString.GetOrdinates(Ordinate.M);
        var expectedMeasures = points.Select(x => x.Y).ToArray();

        Assert.Equal(expectedMeasures, actualMeasures);
    }

    [Fact]
    public void WhenTranslatingRoadSegmentGeometry_ThenMeasuresAreUpdatedCorrectly()
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
    public void WhenTranslatingMultiLineString_ThenMeasuresAreUpdatedCorrectly()
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

    [Fact]
    public void WhenTranslatingMultiLineString_ThenDuplicateCoordinatesAreRemoved()
    {
        var points = new[] { new Coordinate(0, 0), new Coordinate(0, 3), new Coordinate(0, 10), new Coordinate(0, 10) };

        var polyline = new MultiLineString([new LineString(points)])
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };

        var geometryLineString = GeometryTranslator.Translate(polyline);

        Assert.Equal(3, geometryLineString.MultiLineString.Single().Points.Length);
    }

    [Theory]
    [InlineData(GeometryTranslatorTestCases.ValidGmlLineString)]
    public void ParseGmlLineString(string gml)
    {
        var geometry = GeometryTranslator.ParseGmlLineString(gml);

        Assert.NotNull(geometry);
        Assert.Equal(SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(), geometry.SRID);
    }

    [Theory]
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

    [Theory]
    [InlineData("MULTILINESTRING ((0 0, 0.00001 0, 1 0))", "MULTILINESTRING ((0 0, 1 0))")]
    [InlineData("MULTILINESTRING ((0 0, 0 0, 1 0))", "MULTILINESTRING ((0 0, 1 0))")]
    [InlineData("MULTILINESTRING ((0 0, 1 0, 1 0, 2 0))", "MULTILINESTRING ((0 0, 1 0, 2 0))")]
    [InlineData("MULTILINESTRING ((0 0, 1 0, 2 0, 1 0, 1 0))", "MULTILINESTRING ((0 0, 1 0, 2 0, 1 0))")]
    [InlineData("MULTILINESTRING ((183741.826185003 187911.293315005, 183741.827610008 187911.292680003, 183744.407305002 187910.895800006, 183747.251580007 187910.961950004, 183750.228150003 187911.358825006, 183753.359000005 187912.001000006, 183818.656000003 187928.063000005, 183963.344000004 187963.673000004, 184025.094000004 187978.860000003, 184047.047000006 187984.266000003, 184054.406000003 187986.079000004))", "MULTILINESTRING ((183741.826185003 187911.293315005, 183744.407305002 187910.895800006, 183747.251580007 187910.961950004, 183750.228150003 187911.358825006, 183753.359000005 187912.001000006, 183818.656000003 187928.063000005, 183963.344000004 187963.673000004, 184025.094000004 187978.860000003, 184047.047000006 187984.266000003, 184054.406000003 187986.079000004))")]
    public void WhenRemovingDuplicateCoordinatesWithinTolerance_ThenSuccess(string wkt, string expectedWkt)
    {
        var geometry = (MultiLineString)new WKTReader().Read(wkt);
        var wktWithoutDuplicateCoordinates = geometry.WithoutDuplicateCoordinates().AsText();

        Assert.Equal(expectedWkt, wktWithoutDuplicateCoordinates);
    }

    [Theory]
    [InlineData("MULTILINESTRING ((0 0, 1.00001 0, 1 0))", "MULTILINESTRING ((0 0, 1 0))")]
    public void WhenRemovingDuplicateCoordinatesWithinToleranceAtLastTwoCoordinates_ThenEndPointIsKept(string wkt, string expectedWkt)
    {
        var geometry = (MultiLineString)new WKTReader().Read(wkt);
        var wktWithoutDuplicateCoordinates = geometry.WithoutDuplicateCoordinates().AsText();

        Assert.Equal(expectedWkt, wktWithoutDuplicateCoordinates);
    }

    [Theory]
    [InlineData("MULTILINESTRING ((0 0, 0 0.01, 1 0))")]
    public void WhenRemovingDuplicateCoordinatesOutsideTolerance_ThenNone(string wkt)
    {
        var geometry = (MultiLineString)new WKTReader().Read(wkt);
        var wktWithoutDuplicateCoordinates = geometry.WithoutDuplicateCoordinates().AsText();

        Assert.Equal(wkt, wktWithoutDuplicateCoordinates);
    }

    private const string GmlAttributes = @"srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2""";

    [Theory]
    [InlineData("posList", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList></gml:LineString>")]
    [InlineData("coordinates", $"<gml:LineString {GmlAttributes}><gml:coordinates>1,2 3,4</gml:coordinates></gml:LineString>")]
    [InlineData("whitespace", $"\r\n<gml:LineString {GmlAttributes}>\r\n  <gml:posList>1 2 3 4</gml:posList>\r\n</gml:LineString>\r\n")]
    [InlineData("xml declaration", $"<?xml version=\"1.0\" encoding=\"utf-8\"?><gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList></gml:LineString>")]
    public void GmlIsValidLineStringAcceptsSingleLineString(string _, string gml)
    {
        Assert.True(GeometryTranslator.GmlIsValidLineString(gml));
        Assert.Equal(new[] { new Coordinate(1, 2), new Coordinate(3, 4) }, GeometryTranslator.ParseGmlLineString(gml).Coordinates.Select(x => new Coordinate(x.X, x.Y)));
    }

    [Fact]
    public void GmlIsValidLineStringAcceptsGmlOfOwnApi()
    {
        var lineString = new LineString([new Coordinate(217368.75, 181577.01), new Coordinate(217400.11, 181499.51)]) { SRID = WellknownSrids.Lambert08 };

        var gml = lineString.ConvertToGml(useHttpsSchema: false, coordinatePrecision: Precisions.GeometryPrecisionV2);

        Assert.True(GeometryTranslator.GmlIsValidLineString(gml));
    }

    [Theory]
    [InlineData("empty string", "")]
    [InlineData("not xml", "LINESTRING (1 2, 3 4)")]
    [InlineData("no closing geometry tag", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList>")]
    [InlineData("no closing coordinates tag", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4")]
    [InlineData("truncated closing tag", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList></gml:LineString")]
    [InlineData("truncated after geometry", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList></gml:LineString><gml:")]
    [InlineData("text after geometry", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList></gml:LineString>garbage")]
    [InlineData("two line strings", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList></gml:LineString><gml:LineString {GmlAttributes}><gml:posList>5 6 7 8</gml:posList></gml:LineString>")]
    [InlineData("two posLists", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList><gml:posList>5 6 7 8</gml:posList></gml:LineString>")]
    [InlineData("two coordinates", $"<gml:LineString {GmlAttributes}><gml:coordinates>1,2 3,4</gml:coordinates><gml:coordinates>5,6 7,8</gml:coordinates></gml:LineString>")]
    [InlineData("coordinates and posList", $"<gml:LineString {GmlAttributes}><gml:coordinates>1,2 3,4</gml:coordinates><gml:posList>5 6 7 8</gml:posList></gml:LineString>")]
    [InlineData("unknown child", $"<gml:LineString {GmlAttributes}><gml:foo>x</gml:foo><gml:posList>1 2 3 4</gml:posList></gml:LineString>")]
    [InlineData("nested element", $"<gml:LineString {GmlAttributes}><gml:posList><gml:foo>1 2 3 4</gml:foo></gml:posList></gml:LineString>")]
    [InlineData("text next to coordinates", $"<gml:LineString {GmlAttributes}>garbage<gml:posList>1 2 3 4</gml:posList></gml:LineString>")]
    [InlineData("no coordinates element", $"<gml:LineString {GmlAttributes}></gml:LineString>")]
    [InlineData("empty posList", $"<gml:LineString {GmlAttributes}><gml:posList></gml:posList></gml:LineString>")]
    [InlineData("blank posList", $"<gml:LineString {GmlAttributes}><gml:posList>   </gml:posList></gml:LineString>")]
    [InlineData("single point", $"<gml:LineString {GmlAttributes}><gml:posList>1 2</gml:posList></gml:LineString>")]
    [InlineData("odd number of ordinates", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3</gml:posList></gml:LineString>")]
    [InlineData("non numeric ordinates", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4 a b</gml:posList></gml:LineString>")]
    [InlineData("non numeric coordinates", $"<gml:LineString {GmlAttributes}><gml:coordinates>1,2 3,4 a,b</gml:coordinates></gml:LineString>")]
    [InlineData("infinite ordinate", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 Infinity</gml:posList></gml:LineString>")]
    [InlineData("incomplete coordinate pair", $"<gml:LineString {GmlAttributes}><gml:coordinates>1,2 3,4 5</gml:coordinates></gml:LineString>")]
    [InlineData("dtd", $"<!DOCTYPE foo [<!ENTITY x \"1 2 3 4\">]><gml:LineString {GmlAttributes}><gml:posList>&x;</gml:posList></gml:LineString>")]
    [InlineData("point", $"<gml:Point {GmlAttributes}><gml:pos>1 2</gml:pos></gml:Point>")]
    [InlineData("multi line string", $"<gml:MultiLineString {GmlAttributes}><gml:lineStringMember><gml:LineString><gml:posList>1 2 3 4</gml:posList></gml:LineString></gml:lineStringMember></gml:MultiLineString>")]
    public void GmlIsValidLineStringRejectsIncompleteOrAmbiguousGml(string _, string gml)
    {
        Assert.False(GeometryTranslator.GmlIsValidLineString(gml));
    }

    [Theory]
    [InlineData("pos", $"<gml:Point {GmlAttributes}><gml:pos>1 2</gml:pos></gml:Point>")]
    [InlineData("coordinates", $"<gml:Point {GmlAttributes}><gml:coordinates>1,2</gml:coordinates></gml:Point>")]
    public void GmlIsValidPointAcceptsSinglePoint(string _, string gml)
    {
        Assert.True(GeometryTranslator.GmlIsValidPoint(gml));
        Assert.Equal(new Coordinate(1, 2), GeometryTranslator.ParseGmlPoint(gml).Coordinate);
    }

    [Theory]
    [InlineData("empty string", "")]
    [InlineData("no closing geometry tag", $"<gml:Point {GmlAttributes}><gml:pos>1 2</gml:pos>")]
    [InlineData("no closing coordinates tag", $"<gml:Point {GmlAttributes}><gml:pos>1 2")]
    [InlineData("text after geometry", $"<gml:Point {GmlAttributes}><gml:pos>1 2</gml:pos></gml:Point>garbage")]
    [InlineData("two points", $"<gml:Point {GmlAttributes}><gml:pos>1 2</gml:pos></gml:Point><gml:Point {GmlAttributes}><gml:pos>3 4</gml:pos></gml:Point>")]
    [InlineData("two pos", $"<gml:Point {GmlAttributes}><gml:pos>1 2</gml:pos><gml:pos>3 4</gml:pos></gml:Point>")]
    [InlineData("unknown child", $"<gml:Point {GmlAttributes}><gml:foo>x</gml:foo><gml:pos>1 2</gml:pos></gml:Point>")]
    [InlineData("empty pos", $"<gml:Point {GmlAttributes}><gml:pos></gml:pos></gml:Point>")]
    [InlineData("non numeric pos", $"<gml:Point {GmlAttributes}><gml:pos>1 2 a</gml:pos></gml:Point>")]
    [InlineData("too many ordinates", $"<gml:Point {GmlAttributes}><gml:pos>1 2 3 4</gml:pos></gml:Point>")]
    [InlineData("line string", $"<gml:LineString {GmlAttributes}><gml:posList>1 2 3 4</gml:posList></gml:LineString>")]
    public void GmlIsValidPointRejectsIncompleteOrAmbiguousGml(string _, string gml)
    {
        Assert.False(GeometryTranslator.GmlIsValidPoint(gml));
    }

    //[Fact]
    [Fact(Skip = "For debugging purposes, convert roadsegment geometry from read endpoint to wkt")]
    public void ConvertGeoJsonToWkt()
    {
        var path = "contour.geojson";
        var json = File.ReadAllText(path);
        var geojson = JsonConvert.DeserializeObject<MultiPolygon>(json);
        var wkt = geojson.ToNetTopologySuiteGeometry().AsText();
    }
}
