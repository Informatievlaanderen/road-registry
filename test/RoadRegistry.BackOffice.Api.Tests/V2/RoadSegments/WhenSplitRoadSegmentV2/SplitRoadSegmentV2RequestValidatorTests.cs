namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenSplitRoadSegmentV2;

using FluentValidation.TestHelper;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;

public class SplitRoadSegmentV2RequestValidatorTests
{
    // Horizontal 10m segment in Lambert 2008 (EPSG:3812): (217368.75, 181577.02) -> (217378.75, 181577.02)
    private const string ValidCutPositionLambert08 = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>217373.75 181577.02</gml:pos></gml:Point>";
    private const string CutPositionLambert72 = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>217373.75 181577.02</gml:pos></gml:Point>";
    private const string CutPositionTooFar = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>217373.75 181580.02</gml:pos></gml:Point>";

    private readonly SplitRoadSegmentV2RequestValidator _validator = new();

    private static MultiLineString SegmentGeometry()
    {
        var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(WellknownSrids.Lambert08);
        return factory.CreateLineString([
            new Coordinate(217368.75, 181577.02),
            new Coordinate(217378.75, 181577.02)
        ]).ToMultiLineString();
    }

    private static SplitRoadSegmentV2Request BuildRequest(
        bool exists = true,
        string status = null,
        string knippositie = ValidCutPositionLambert08,
        bool withGeometry = true)
    {
        return new SplitRoadSegmentV2Request
        {
            RoadSegmentId = new RoadSegmentId(100),
            RoadSegmentExists = exists,
            RoadSegmentStatus = status ?? RoadSegmentStatusV2.Gerealiseerd.ToString(),
            RoadSegmentGeometry = withGeometry ? SegmentGeometry() : null,
            Knippositie = knippositie
        };
    }

    [Fact]
    public void ParseGmlPoint_ReadsLambert08PointGeometry()
    {
        var point = GeometryTranslator.ParseGmlPoint(ValidCutPositionLambert08);

        Assert.Equal(WellknownSrids.Lambert08, point.SRID);
        Assert.Equal(217373.75, point.X);
        Assert.Equal(181577.02, point.Y);
    }

    [Fact]
    public void ValidRequest_IsValid()
    {
        var result = _validator.TestValidate(BuildRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(nameof(RoadSegmentStatusV2.Gerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public void AllowedStatus_IsValid(string status)
    {
        var result = _validator.TestValidate(BuildRequest(status: status));
        result.ShouldNotHaveValidationErrorFor(x => x.RoadSegmentStatus);
    }

    [Fact]
    public void RoadSegmentDoesNotExist_HasNotFoundError()
    {
        var result = _validator.TestValidate(BuildRequest(exists: false, withGeometry: false));
        result.ShouldHaveValidationErrorFor(x => x.RoadSegmentExists)
            .WithErrorCode(ProblemCode.RoadSegment.Split.NotFound);
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    public void StatusNotAllowed_HasStatusNotValidError(string status)
    {
        var result = _validator.TestValidate(BuildRequest(status: status));
        result.ShouldHaveValidationErrorFor(x => x.RoadSegmentStatus)
            .WithErrorCode(ProblemCode.RoadSegment.Split.StatusNotValid);
    }

    [Fact]
    public void KnippositieNull_HasPositionIsRequiredError()
    {
        var result = _validator.TestValidate(BuildRequest(knippositie: null));
        result.ShouldHaveValidationErrorFor(x => x.Knippositie)
            .WithErrorCode(ProblemCode.RoadSegment.Split.PositionIsRequired);
    }

    [Fact]
    public void KnippositieInvalidGml_HasPositionGeometryNotValidError()
    {
        var result = _validator.TestValidate(BuildRequest(knippositie: "not a valid gml"));
        result.ShouldHaveValidationErrorFor(x => x.Knippositie)
            .WithErrorCode(ProblemCode.RoadSegment.Split.PositionGeometryNotValid);
    }

    [Fact]
    public void KnippositieNotLambert08_HasSridError()
    {
        var result = _validator.TestValidate(BuildRequest(knippositie: CutPositionLambert72));
        result.ShouldHaveValidationErrorFor(x => x.Knippositie)
            .WithErrorCode(ProblemCode.RoadSegment.Split.PositionSridNotLambert08);
    }

    [Fact]
    public void KnippositieTooFarFromRoadSegment_HasTooFarError()
    {
        var result = _validator.TestValidate(BuildRequest(knippositie: CutPositionTooFar));
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode(ProblemCode.RoadSegment.Split.PositionTooFarFromRoadSegment);
    }

    [Fact]
    public void KnippositieOnRoadSegment_HasNoDistanceError()
    {
        var result = _validator.TestValidate(BuildRequest());
        result.ShouldNotHaveValidationErrorFor(x => x);
    }
}
