namespace RoadRegistry.Tests.BackOffice;

using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using Xunit;

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
}