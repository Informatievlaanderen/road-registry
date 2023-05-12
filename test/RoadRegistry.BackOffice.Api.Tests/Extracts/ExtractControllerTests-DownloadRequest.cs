namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Api.Extracts;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit.Sdk;
using GeometryTranslator = BackOffice.GeometryTranslator;
using Point = NetTopologySuite.Geometries.Point;

public partial class ExtractControllerTests
{
    [Fact]
    public async Task When_requesting_an_extract_for_the_first_time()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var contour = _fixture.Create<RoadNetworkExtractGeometry>();
        var response = await Controller.RequestDownload(new DownloadExtractRequestBody(
            externalExtractRequestId,
            writer.Write((Geometry)GeometryTranslator.Translate(contour))
        ), CancellationToken.None);
        var result = Assert.IsType<AcceptedResult>(response);
        Assert.IsType<DownloadExtractResponseBody>(result.Value);
    }

    [Fact]
    public async Task When_requesting_an_extract_with_a_contour_that_is_not_a_multipolygon()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };
        //var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
        //var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
        //var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
        //var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        try
        {
            await Controller.RequestDownload(new DownloadExtractRequestBody
            (
                externalExtractRequestId,
                writer.Write(new Point(1.0, 2.0))
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
        //var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
        //var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
        //var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
        //var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        try
        {
            await Controller.RequestDownload(new DownloadExtractRequestBody
            (
                null,
                externalExtractRequestId
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
        //var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
        //var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
        //var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
        //var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

        var contour = _fixture.Create<RoadNetworkExtractGeometry>();
        try
        {
            await Controller.RequestDownload(new DownloadExtractRequestBody
            (
                writer.Write((Geometry)GeometryTranslator.Translate(contour)),
                null
            ), CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }
}