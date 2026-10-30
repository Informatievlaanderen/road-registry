namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments;

using System.Linq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;
using RoadRegistry.Tests;

public class V1GmlLineStringTests
{
    private const string GmlAttributes = @"srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2""";
    private const string FirstLine = "<gml:posList>217368.75 181577.01 217400.11 181499.51</gml:posList>";
    private const string SecondLine = "<gml:posList>217400.11 181499.51 217500.11 181599.51</gml:posList>";

    private static readonly Coordinate[] FirstLineCoordinates =
    [
        new(217368.75, 181577.01),
        new(217400.11, 181499.51)
    ];

    // What the stricter general validation refuses, but the existing users of the v1 endpoints can keep sending.
    public static TheoryData<string> GmlOnlyAcceptedByV1 => new()
    {
        $"<gml:LineString {GmlAttributes}>{FirstLine}{SecondLine}</gml:LineString>",
        $"<gml:LineString {GmlAttributes}>{FirstLine}</gml:LineString><gml:LineString {GmlAttributes}>{SecondLine}</gml:LineString>",
        $"<gml:LineString {GmlAttributes}>{FirstLine}</gml:LineString>garbage",
        $"<gml:LineString {GmlAttributes}>{FirstLine}</gml:LineString><gml:",
        $"<gml:LineString {GmlAttributes}><gml:posList>217368.75 181577.01 217400.11 181499.51 a b</gml:posList></gml:LineString>"
    };

    [Theory]
    [MemberData(nameof(GmlOnlyAcceptedByV1))]
    public void ParseUsesTheFirstGeometry(string gml)
    {
        Assert.True(V1GmlLineString.IsValid(gml));
        Assert.Equal(FirstLineCoordinates, V1GmlLineString.Parse(gml).Coordinates.Select(x => new Coordinate(x.X, x.Y)));
    }

    [Theory]
    [MemberData(nameof(GmlOnlyAcceptedByV1))]
    public void GeneralValidationRefusesIt(string gml)
    {
        Assert.False(GeometryTranslator.GmlIsValidLineString(gml));
    }

    [Theory]
    [MemberData(nameof(GmlOnlyAcceptedByV1))]
    public async Task CreateOutlineValidatorAcceptsIt(string gml)
    {
        var validator = new PostRoadSegmentOutlineParametersValidator(new FakeOrganizationCache());

        var result = await validator.ValidateAsync(new PostRoadSegmentOutlineParameters { MiddellijnGeometrie = gml });

        Assert.DoesNotContain(result.Errors, x => x.PropertyName == nameof(PostRoadSegmentOutlineParameters.MiddellijnGeometrie));
    }

    [Theory]
    [MemberData(nameof(GmlOnlyAcceptedByV1))]
    public async Task ChangeOutlineGeometryValidatorAcceptsIt(string gml)
    {
        var validator = new PostChangeOutlineGeometryParametersValidator();

        var result = await validator.ValidateAsync(new PostChangeOutlineGeometryParameters { MiddellijnGeometrie = gml });

        Assert.DoesNotContain(result.Errors, x => x.PropertyName == nameof(PostChangeOutlineGeometryParameters.MiddellijnGeometrie));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData($"<gml:LineString {GmlAttributes}>{FirstLine}")]
    [InlineData($"<gml:Point {GmlAttributes}><gml:pos>217368.75 181577.01</gml:pos></gml:Point>")]
    public void WhatWasNeverAcceptedStaysRefused(string gml)
    {
        Assert.False(V1GmlLineString.IsValid(gml));
    }
}
