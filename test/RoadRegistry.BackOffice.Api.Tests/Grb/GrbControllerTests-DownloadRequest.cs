namespace RoadRegistry.BackOffice.Api.Tests.Grb;

using Api.Extracts;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentAssertions;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit.Sdk;
using DownloadExtractRequestBody = Api.Grb.DownloadExtractRequestBody;
using GeometryTranslator = BackOffice.GeometryTranslator;
using Point = NetTopologySuite.Geometries.Point;

public partial class GrbControllerTests
{
    [Fact]
    public async Task When_requesting_an_extract_for_the_first_time()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var contour = new RoadNetworkExtractGeometry
        {
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
            WKT = "MultiPolygon (((64699.86540096173121128 218247.15990484040230513, 91541.66608652254217304 211821.38593515567481518, 91541.66608652254217304 211821.38593515567481518, 91541.66608652254217304 211821.38593515567481518, 91523.78514424958848394 195503.55442933458834887, 88364.38692880698363297 166590.4106055349111557, 50936.49097712300135754 171639.109682597219944, 36050.97635232988977805 199580.85535924322903156, 64699.86540096173121128 218247.15990484040230513)))"
        };
        var response = await Controller.ExtractByContour(new Api.Grb.DownloadExtractRequestBody(
            writer.Write((Geometry)GeometryTranslator.Translate(contour)),
            externalExtractRequestId,
            false
        ), CancellationToken.None);
        var result = Assert.IsType<AcceptedResult>(response);
        Assert.IsType<Api.Grb.DownloadExtractResponseBody>(result.Value);
    }

    [Fact]
    public async Task When_requesting_an_informative_extract_using_the_requestid()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };

        var externalExtractRequestId = $"INF_{_fixture.Create<ExternalExtractRequestId>()}";
        var contour = new RoadNetworkExtractGeometry
        {
            SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
            WKT = "MultiPolygon (((64699.86540096173121128 218247.15990484040230513, 91541.66608652254217304 211821.38593515567481518, 91541.66608652254217304 211821.38593515567481518, 91541.66608652254217304 211821.38593515567481518, 91523.78514424958848394 195503.55442933458834887, 88364.38692880698363297 166590.4106055349111557, 50936.49097712300135754 171639.109682597219944, 36050.97635232988977805 199580.85535924322903156, 64699.86540096173121128 218247.15990484040230513)))"
        };
        var response = await Controller.ExtractByContour(new Api.Grb.DownloadExtractRequestBody(
            writer.Write((Geometry)GeometryTranslator.Translate(contour)),
            externalExtractRequestId,
            null
        ), CancellationToken.None);
        var result = Assert.IsType<AcceptedResult>(response);
        var responseBody = Assert.IsType<Api.Grb.DownloadExtractResponseBody>(result.Value);
        responseBody.IsInformative.Should().BeTrue();
    }

    [Fact]
    public async Task When_requesting_an_extract_with_a_contour_that_is_not_a_multipolygon()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        try
        {
            await Controller.ExtractByContour(new Api.Grb.DownloadExtractRequestBody
            (
                externalExtractRequestId,
                writer.Write(new Point(1.0, 2.0)), false
            ), CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_requesting_an_extract_without_contour()
    {
        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        try
        {
            await Controller.ExtractByContour(new Api.Grb.DownloadExtractRequestBody
            (
                null,
                externalExtractRequestId,
                false
            ), CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_requesting_an_extract_without_request_id()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };

        var contour = _fixture.Create<RoadNetworkExtractGeometry>();
        try
        {
            await Controller.ExtractByContour(new DownloadExtractRequestBody
            (
                writer.Write((Geometry)GeometryTranslator.Translate(contour)),
                null,
                false
            ), CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }
}
